import { TestBed } from '@angular/core/testing';
import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';

import { LeadService } from './lead.service';
import { environment } from '../../../environments/environment';

describe('LeadService', () => {
  let service: LeadService;
  let httpMock: HttpTestingController;

  const API = environment.apiBase;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
    });
    service = TestBed.inject(LeadService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getLeads deve chamar o endpoint correto e devolver os items', () => {
    const mockResponse = {
      items: [
        {
          id: 1,
          name: 'Alice',
          email: 'a@a.com',
          status: 'New',
          createdAt: '',
          updatedAt: '',
          tasksCount: 0,
        },
      ],
      totalCount: 1,
      page: 1,
      pageSize: 10,
      totalPages: 1,
    };

    service.getLeads().subscribe((result) => {
      expect(result.items.length).toBe(1);
      expect(result.items[0].status).toBe('New');
    });

    const req = httpMock.expectOne((r) => r.url === `${API}/leads`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('getLeadById deve retornar lead com tasks', () => {
    const mockLead = {
      id: 1,
      name: 'Bob',
      email: 'b@b.com',
      status: 'Won',
      createdAt: '',
      updatedAt: '',
      tasks: [
        {
          id: 10,
          leadId: 1,
          title: 'T1',
          dueDate: null,
          status: 'Todo',
          createdAt: '',
          updatedAt: '',
        },
      ],
    };

    service.getLeadById(1).subscribe((lead) => {
      expect(lead.status).toBe('Won');
      expect(lead.tasks.length).toBe(1);
      expect(lead.tasks[0].title).toBe('T1');
    });

    const req = httpMock.expectOne(`${API}/leads/1`);
    expect(req.request.method).toBe('GET');
    req.flush(mockLead);
  });

  it('createLead deve fazer POST com status string', () => {
    const dto = { name: 'Carol', email: 'c@c.com', status: 'New' as const };
    const mockResponse = {
      id: 3,
      name: 'Carol',
      email: 'c@c.com',
      status: 'Qualified',
      createdAt: '',
      updatedAt: '',
      tasksCount: 0,
    };

    service.createLead(dto).subscribe((lead) => {
      expect(lead.status).toBe('Qualified');
    });

    const req = httpMock.expectOne(`${API}/leads`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(dto);
    req.flush(mockResponse);
  });

  it('deleteLead deve fazer DELETE no endpoint correto', () => {
    service.deleteLead(5).subscribe();

    const req = httpMock.expectOne(`${API}/leads/5`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
