import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Lead } from '../models/Lead/Lead.model';
import { LeadStatus } from '../models/Lead/LeadStatus.enum';
import { TaskItem } from '../models/TaskItem/TaskItem.model';
import { environment } from '../../../environments/environment';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface LeadDetail extends Lead {
  tasks: TaskItem[];
}

export interface LeadCreateDto {
  name: string;
  email: string;
  status: LeadStatus;
}

export interface LeadUpdateDto {
  name: string;
  email: string;
  status: LeadStatus;
}

const API_BASE = environment.apiBase;

@Injectable({
  providedIn: 'root',
})
export class LeadService {
  constructor(private http: HttpClient) {}

  getLeads(
    search?: string,
    status?: LeadStatus | string,
    page = 1,
    pageSize = 10,
  ): Observable<PagedResult<Lead>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search) {
      params = params.set('search', search);
    }
    if (status) {
      params = params.set('status', status);
    }

    return this.http.get<PagedResult<Lead>>(`${API_BASE}/leads`, { params });
  }

  getLeadById(id: number): Observable<LeadDetail> {
    return this.http.get<LeadDetail>(`${API_BASE}/leads/${id}`);
  }

  createLead(dto: LeadCreateDto): Observable<Lead> {
    return this.http.post<Lead>(`${API_BASE}/leads`, dto);
  }

  updateLead(id: number, dto: LeadUpdateDto): Observable<Lead> {
    return this.http.put<Lead>(`${API_BASE}/leads/${id}`, dto);
  }

  deleteLead(id: number): Observable<void> {
    return this.http.delete<void>(`${API_BASE}/leads/${id}`);
  }
}
