import { TestBed } from '@angular/core/testing';
import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';

import { TaskService } from './task.service';
import { environment } from '../../../environments/environment';

describe('TaskService', () => {
  let service: TaskService;
  let httpMock: HttpTestingController;

  const API = environment.apiBase;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
    });
    service = TestBed.inject(TaskService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getTasksByLead deve chamar o endpoint correto', () => {
    const mockResponse = [
      {
        id: 1,
        leadId: 1,
        title: 'Task A',
        dueDate: null,
        status: 'Todo',
        createdAt: '',
        updatedAt: '',
      },
      {
        id: 2,
        leadId: 1,
        title: 'Task B',
        dueDate: null,
        status: 'Doing',
        createdAt: '',
        updatedAt: '',
      },
    ];

    service.getTasksByLead(1).subscribe((tasks) => {
      expect(tasks.length).toBe(2);
      expect(tasks[0].status).toBe('Todo');
      expect(tasks[1].status).toBe('Doing');
    });

    const req = httpMock.expectOne(`${API}/leads/1/tasks`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('createTask deve fazer POST com status string', () => {
    const dto = { title: 'Nova Task', dueDate: null, status: 'Todo' as const };
    const mockResponse = {
      id: 10,
      leadId: 1,
      title: 'Nova Task',
      dueDate: null,
      status: 'Done',
      createdAt: '',
      updatedAt: '',
    };

    service.createTask(1, dto).subscribe((task) => {
      expect(task.status).toBe('Done');
      expect(task.title).toBe('Nova Task');
    });

    const req = httpMock.expectOne(`${API}/leads/1/tasks`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(dto);
    req.flush(mockResponse);
  });

  it('updateTask deve fazer PUT no endpoint correto', () => {
    const dto = {
      title: 'Task Atualizada',
      dueDate: null,
      status: 'Doing' as const,
    };
    const mockResponse = {
      id: 5,
      leadId: 2,
      title: 'Task Atualizada',
      dueDate: null,
      status: 'Doing',
      createdAt: '',
      updatedAt: '',
    };

    service.updateTask(2, 5, dto).subscribe((task) => {
      expect(task.status).toBe('Doing');
      expect(task.title).toBe('Task Atualizada');
    });

    const req = httpMock.expectOne(`${API}/leads/2/tasks/5`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(dto);
    req.flush(mockResponse);
  });

  it('deleteTask deve fazer DELETE no endpoint correto', () => {
    service.deleteTask(3, 7).subscribe();

    const req = httpMock.expectOne(`${API}/leads/3/tasks/7`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
