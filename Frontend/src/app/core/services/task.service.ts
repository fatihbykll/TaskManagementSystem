import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedResponse } from '../../models/api-response.model';
import { TaskItem, CreateTaskRequest, UpdateTaskRequest, TaskFilter, TaskStatistics, TaskStatus } from '../../models/task.model';
@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Tasks`;
  getAll(filter: TaskFilter): Observable<ApiResponse<PagedResponse<TaskItem>>> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize',   filter.pageSize.toString());
    if (filter.searchTerm)    params = params.set('searchTerm',    filter.searchTerm);
    if (filter.status !== undefined)   params = params.set('status',   filter.status.toString());
    if (filter.priority !== undefined) params = params.set('priority', filter.priority.toString());
    if (filter.startDate)  params = params.set('startDate',  filter.startDate);
    if (filter.endDate)    params = params.set('endDate',    filter.endDate);
    if (filter.sortBy)     params = params.set('sortBy',     filter.sortBy);
    if (filter.sortDirection) params = params.set('sortDirection', filter.sortDirection);
    return this.http.get<ApiResponse<PagedResponse<TaskItem>>>(this.baseUrl, { params });
  }
  getById(id: string): Observable<ApiResponse<TaskItem>> {
    return this.http.get<ApiResponse<TaskItem>>(`${this.baseUrl}/${id}`);
  }
  create(req: CreateTaskRequest): Observable<ApiResponse<TaskItem>> {
    return this.http.post<ApiResponse<TaskItem>>(this.baseUrl, req);
  }
  update(id: string, req: UpdateTaskRequest): Observable<ApiResponse<TaskItem>> {
    return this.http.put<ApiResponse<TaskItem>>(`${this.baseUrl}/${id}`, req);
  }
  delete(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${id}`);
  }
  updateStatus(id: string, status: TaskStatus): Observable<ApiResponse<TaskItem>> {
    return this.http.patch<ApiResponse<TaskItem>>(`${this.baseUrl}/${id}/status`, { status });
  }
  getStatistics(): Observable<ApiResponse<TaskStatistics>> {
    return this.http.get<ApiResponse<TaskStatistics>>(`${this.baseUrl}/statistics`);
  }
  getOverdue(): Observable<ApiResponse<TaskItem[]>> {
    return this.http.get<ApiResponse<TaskItem[]>>(`${this.baseUrl}/overdue`);
  }
}
