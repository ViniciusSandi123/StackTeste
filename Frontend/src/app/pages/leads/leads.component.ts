import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { LeadDetailComponent } from '../lead-detail/lead-detail.component';
import { ConfirmDialogComponent } from '../../shared/ui/confirm-dialog/confirm-dialog.component';
import { LeadService } from '../../core/services/lead.service';
import { NotificationService } from '../../core/services/notification.service';
import { Lead } from '../../core/models/Lead/Lead.model';
import { LeadStatus } from '../../core/models/Lead/LeadStatus.enum';

@Component({
  selector: 'app-leads',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    FormsModule,
    MatDialogModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './leads.component.html',
  styleUrl: './leads.component.scss',
})
export class LeadsComponent implements OnInit, OnDestroy {
  constructor(
    private dialog: MatDialog,
    private leadService: LeadService,
    private notification: NotificationService,
  ) {}

  displayedColumns: string[] = [
    'name',
    'email',
    'status',
    'tasksCount',
    'createdAt',
    'actions',
  ];

  searchText = '';
  selectedStatus: LeadStatus | '' = '';
  statuses: LeadStatus[] = ['New', 'Qualified', 'Won', 'Lost'];

  leads: Lead[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 10;
  isLoading = false;

  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.loadLeads();

    this.searchSubject
      .pipe(debounceTime(400), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(() => {
        this.page = 1;
        this.loadLeads();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadLeads(): void {
    this.isLoading = true;
    this.leadService
      .getLeads(
        this.searchText || undefined,
        this.selectedStatus || undefined,
        this.page,
        this.pageSize,
      )
      .subscribe({
        next: (result) => {
          this.leads = result.items;
          this.totalCount = result.totalCount;
          this.isLoading = false;
        },
        error: (err) => {
          this.isLoading = false;
          this.notification.error(
            this.notification.extractError(err, 'Erro ao carregar leads.'),
          );
        },
      });
  }

  onSearchChange(): void {
    this.searchSubject.next(this.searchText);
  }

  onStatusChange(): void {
    this.page = 1;
    this.loadLeads();
  }

  onPageChange(event: PageEvent): void {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadLeads();
  }

  openNewLeadModal(): void {
    const dialogRef = this.dialog.open(LeadDetailComponent, {
      width: '720px',
      maxWidth: '720px',
      disableClose: false,
    });

    dialogRef.afterClosed().subscribe((created) => {
      if (created) this.loadLeads();
    });
  }

  openEditLeadModal(lead: Lead): void {
    const dialogRef = this.dialog.open(LeadDetailComponent, {
      width: '720px',
      maxWidth: '720px',
      data: lead,
      disableClose: false,
    });

    dialogRef.afterClosed().subscribe((updated) => {
      if (updated) this.loadLeads();
    });
  }

  deleteLead(lead: Lead): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: { message: `Deseja excluir o lead "${lead.name}"?` },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.leadService.deleteLead(lead.id).subscribe({
          next: () => {
            this.notification.success('Lead excluído com sucesso.');
            this.loadLeads();
          },
          error: (err) => {
            this.notification.error(
              this.notification.extractError(err, 'Erro ao excluir o lead.'),
            );
          },
        });
      }
    });
  }
}
