import { Component, Inject, OnInit, Optional } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import {
  MatDialogRef,
  MAT_DIALOG_DATA,
  MatDialogModule,
} from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { forkJoin, Observable, of } from 'rxjs';
import { LeadService, LeadDetail } from '../../core/services/lead.service';
import { TaskService } from '../../core/services/task.service';
import { NotificationService } from '../../core/services/notification.service';
import { Lead } from '../../core/models/Lead/Lead.model';
import { LeadStatus } from '../../core/models/Lead/LeadStatus.enum';
import { TaskStatus } from '../../core/models/TaskItem/TaskStatus.enum';

interface TaskForm {
  id?: number;
  title: string;
  dueDate: string | null;
  status: TaskStatus;
}

@Component({
  selector: 'app-lead-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './lead-detail.component.html',
  styleUrl: './lead-detail.component.scss',
})
export class LeadDetailComponent implements OnInit {
  constructor(
    @Optional() public dialogRef: MatDialogRef<LeadDetailComponent>,
    @Optional() @Inject(MAT_DIALOG_DATA) public data: Lead | null,
    private route: ActivatedRoute,
    private router: Router,
    private leadService: LeadService,
    private taskService: TaskService,
    private notification: NotificationService,
  ) {}

  leadId: number | null = null;

  lead = {
    name: '',
    email: '',
    status: 'New' as LeadStatus,
  };

  tasks: TaskForm[] = [];
  deletedTaskIds: number[] = [];
  fieldErrors: Record<string, string[]> = {};

  statuses: LeadStatus[] = ['New', 'Qualified', 'Won', 'Lost'];
  taskStatuses: TaskStatus[] = ['Todo', 'Doing', 'Done'];

  isLoadingLead = false;
  isSaving = false;
  errorMessage = '';

  get isEditing(): boolean {
    return !!this.leadId;
  }

  get isDialog(): boolean {
    return !!this.dialogRef;
  }

  ngOnInit(): void {
    const idFromData = this.data?.id;
    const idFromRoute = this.route.snapshot.paramMap.get('id');
    const id = idFromData ?? (idFromRoute ? +idFromRoute : null);

    if (id != null) {
      this.loadLead(id);
    }
  }

  private loadLead(id: number): void {
    this.isLoadingLead = true;
    this.leadService.getLeadById(id).subscribe({
      next: (detail) => {
        this.applyLeadData(detail);
        this.isLoadingLead = false;
      },
      error: (err) => {
        this.isLoadingLead = false;
        this.errorMessage = this.notification.extractError(
          err,
          'Erro ao carregar o lead.',
        );
      },
    });
  }

  private applyLeadData(detail: LeadDetail): void {
    this.leadId = detail.id;
    this.lead = {
      name: detail.name,
      email: detail.email,
      status: detail.status,
    };
    this.tasks = (detail.tasks ?? []).map((t) => ({
      id: t.id,
      title: t.title,
      dueDate: t.dueDate ? t.dueDate.substring(0, 10) : null,
      status: t.status,
    }));
  }

  addTask(): void {
    this.tasks.push({ title: '', dueDate: null, status: 'Todo' });
  }

  removeTask(index: number): void {
    const task = this.tasks[index];
    if (task.id) {
      this.deletedTaskIds.push(task.id);
    }
    this.tasks.splice(index, 1);
  }

  private validateForm(): string | null {
    if (!this.lead.name.trim()) return 'O nome é obrigatório.';
    if (this.lead.name.trim().length < 3)
      return 'O nome deve ter ao menos 3 caracteres.';
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!this.lead.email.trim()) return 'O e-mail é obrigatório.';
    if (!emailRegex.test(this.lead.email.trim()))
      return 'Informe um e-mail válido.';

    const taskSemTitulo = this.tasks.findIndex((t) => !t.title.trim());
    if (taskSemTitulo !== -1)
      return `O título da task ${taskSemTitulo + 1} é obrigatório.`;

    return null;
  }

  save(): void {
    const validationError = this.validateForm();
    if (validationError) {
      this.errorMessage = validationError;
      return;
    }

    this.errorMessage = '';
    this.fieldErrors = {};
    this.isSaving = true;

    const leadDto = {
      name: this.lead.name.trim(),
      email: this.lead.email.trim(),
      status: this.lead.status,
    };

    const leadObs$ = this.isEditing
      ? this.leadService.updateLead(this.leadId!, leadDto)
      : this.leadService.createLead(leadDto);

    leadObs$.subscribe({
      next: (savedLead) => {
        this.saveTasks(savedLead);
      },
      error: (err) => {
        this.handleHttpError(err, 'Erro ao salvar o lead.');
        this.isSaving = false;
      },
    });
  }

  private saveTasks(lead: Lead): void {
    const ops: Observable<any>[] = [];

    this.deletedTaskIds.forEach((id) => {
      ops.push(this.taskService.deleteTask(lead.id, id));
    });

    this.tasks.forEach((task) => {
      const dto = {
        title: task.title.trim(),
        dueDate: task.dueDate || null,
        status: task.status,
      };
      if (!task.id) {
        ops.push(this.taskService.createTask(lead.id, dto));
      } else {
        ops.push(this.taskService.updateTask(lead.id, task.id, dto));
      }
    });

    const allOps$: Observable<any> = ops.length > 0 ? forkJoin(ops) : of(null);

    allOps$.subscribe({
      next: () => {
        this.isSaving = false;
        const action = this.isEditing ? 'atualizado' : 'criado';
        this.notification.success(`Lead ${action} com sucesso.`);
        this.finalize(lead);
      },
      error: (err) => {
        this.isSaving = false;
        this.handleHttpError(err, 'Erro ao salvar as tasks.');
      },
    });
  }

  private handleHttpError(err: any, fallback: string): void {
    const problem = err?.error;
    if (problem?.errors && typeof problem.errors === 'object') {
      this.fieldErrors = problem.errors;
    }
    const msg = this.notification.extractError(err, fallback);
    this.errorMessage = msg;
    this.notification.error(msg);
  }

  private finalize(lead: Lead): void {
    if (this.dialogRef) {
      this.dialogRef.close(lead);
    } else {
      this.router.navigate(['/leads']);
    }
  }

  cancel(): void {
    if (this.dialogRef) {
      this.dialogRef.close(null);
    } else {
      this.router.navigate(['/leads']);
    }
  }
}
