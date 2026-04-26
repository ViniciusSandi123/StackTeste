import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';
import { LeadsComponent } from './leads.component';
import { LeadService } from '../../core/services/lead.service';
import { NotificationService } from '../../core/services/notification.service';

const leadServiceMock = {
  getLeads: () =>
    of({ items: [], totalCount: 0, page: 1, pageSize: 10, totalPages: 0 }),
  deleteLead: () => of(void 0),
};

const matDialogMock = {
  open: () => ({ afterClosed: () => of(null) }),
};

const notificationServiceMock = {
  success: jasmine.createSpy('success'),
  error: jasmine.createSpy('error'),
  extractError: (_err: any, fallback: string) => fallback,
};

describe('LeadsComponent', () => {
  let component: LeadsComponent;
  let fixture: ComponentFixture<LeadsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LeadsComponent, NoopAnimationsModule],
      providers: [
        { provide: LeadService, useValue: leadServiceMock },
        { provide: MatDialog, useValue: matDialogMock },
        { provide: NotificationService, useValue: notificationServiceMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LeadsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('deve carregar leads ao inicializar', () => {
    expect(component.leads).toEqual([]);
    expect(component.totalCount).toBe(0);
  });

  it('deve resetar a página ao mudar o status', () => {
    component.page = 3;
    component.onStatusChange();
    expect(component.page).toBe(1);
  });

  it('deve atualizar página e pageSize ao paginar', () => {
    component.onPageChange({ pageIndex: 1, pageSize: 25, length: 100 } as any);
    expect(component.page).toBe(2);
    expect(component.pageSize).toBe(25);
  });
});
