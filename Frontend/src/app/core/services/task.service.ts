import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TaskItem } from '../models/TaskItem/TaskItem.model';
import { TaskStatus } from '../models/TaskItem/TaskStatus.enum';
import { environment } from '../../../environments/environment';

export interface TaskCreateDto {
  title: string;
  dueDate?: string | null;
  status: TaskStatus;
}

export interface TaskUpdateDto {
  title: string;
  dueDate?: string | null;
  status: TaskStatus;
}

const API_BASE = environment.apiBase;

@Injectable({
  providedIn: 'root',
})
export class TaskService {
  constructor(private http: HttpClient) {}

  getTasksByLead(leadId: number): Observable<TaskItem[]> {
    return this.http.get<TaskItem[]>(`${API_BASE}/leads/${leadId}/tasks`);
  }

  getTaskById(leadId: number, taskId: number): Observable<TaskItem> {
    return this.http.get<TaskItem>(
      `${API_BASE}/leads/${leadId}/tasks/${taskId}`,
    );
  }

  createTask(leadId: number, dto: TaskCreateDto): Observable<TaskItem> {
    return this.http.post<TaskItem>(`${API_BASE}/leads/${leadId}/tasks`, dto);
  }

  updateTask(
    leadId: number,
    taskId: number,
    dto: TaskUpdateDto,
  ): Observable<TaskItem> {
    return this.http.put<TaskItem>(
      `${API_BASE}/leads/${leadId}/tasks/${taskId}`,
      dto,
    );
  }

  deleteTask(leadId: number, taskId: number): Observable<void> {
    return this.http.delete<void>(
      `${API_BASE}/leads/${leadId}/tasks/${taskId}`,
    );
  }
}
