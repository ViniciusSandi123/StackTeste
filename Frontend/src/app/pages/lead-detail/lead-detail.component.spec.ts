import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { of } from 'rxjs';
import { LeadDetailComponent } from './lead-detail.component';
import { LeadService } from '../../core/services/lead.service';
import { TaskService } from '../../core/services/task.service';
import { NotificationService } from '../../core/services/notification.service';

const leadServiceMock = {
  getLeadById: () =>
    of({
      id: 1,
      name: 'Teste',
      email: 'teste@teste.com',
      status: 'New',
      createdAt: '',
      updatedAt: '',
      tasks: [],
    }),
  createLead: (dto: any) =>
    of({ id: 2, ...dto, status: 'New', createdAt: '', updatedAt: '' }),
  updateLead: (_id: number, dto: any) =>
    of({ id: 1, ...dto, status: 'New', createdAt: '', updatedAt: '' }),
};

const taskServiceMock = {
  getTasksByLead: () => of([]),
  createTask: (_leadId: number, dto: any) =>
    of({
      id: 1,
      leadId: 1,
      ...dto,
      status: 'Todo',
      createdAt: '',
      updatedAt: '',
    }),
  updateTask: () => of({}),
  deleteTask: () => of(void 0),
};

const notificationServiceMock = {
  success: jasmine.createSpy('success'),
  error: jasmine.createSpy('error'),
  extractError: (_err: any, fallback: string) => fallback,
};

const activatedRouteMock = {
  snapshot: { paramMap: convertToParamMap({}) },
};

describe('LeadDetailComponent', () => {
  let component: LeadDetailComponent;
  let fixture: ComponentFixture<LeadDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LeadDetailComponent, NoopAnimationsModule],
      providers: [
        { provide: LeadService, useValue: leadServiceMock },
        { provide: TaskService, useValue: taskServiceMock },
        { provide: NotificationService, useValue: notificationServiceMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock },
        {
          provide: MatDialogRef,
          useValue: { close: jasmine.createSpy('close') },
        },
        { provide: MAT_DIALOG_DATA, useValue: null },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LeadDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('deve iniciar sem lead em edição', () => {
    expect(component.isEditing).toBeFalse();
    expect(component.lead.name).toBe('');
    expect(component.lead.email).toBe('');
  });

  it('addTask deve adicionar uma task vazia', () => {
    component.addTask();
    expect(component.tasks.length).toBe(1);
    expect(component.tasks[0].title).toBe('');
    expect(component.tasks[0].status).toBe('Todo');
  });

  it('removeTask deve remover a task da lista', () => {
    component.addTask();
    component.addTask();
    component.removeTask(0);
    expect(component.tasks.length).toBe(1);
  });

  it('validateForm deve retornar erro se nome for vazio', () => {
    component.lead.name = '';
    component.lead.email = 'email@teste.com';
    component.save();
    expect(component.errorMessage).toBe('O nome é obrigatório.');
  });

  it('validateForm deve retornar erro se nome tiver menos de 3 caracteres', () => {
    component.lead.name = 'Ab';
    component.lead.email = 'email@teste.com';
    component.save();
    expect(component.errorMessage).toBe(
      'O nome deve ter ao menos 3 caracteres.',
    );
  });

  it('validateForm deve retornar erro se e-mail for inválido', () => {
    component.lead.name = 'Nome Válido';
    component.lead.email = 'emailinvalido';
    component.save();
    expect(component.errorMessage).toBe('Informe um e-mail válido.');
  });
});
