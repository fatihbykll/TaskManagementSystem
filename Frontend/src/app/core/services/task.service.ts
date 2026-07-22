import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedResponse } from '../../models/api-response.model';
import {
  TaskItem,
  CreateTaskRequest,
  UpdateTaskRequest,
  TaskFilter,
  TaskStatistics,
  TaskStatus
} from '../../models/task.model';
/**
 * Görev CRUD ve özel endpoint'ler için HTTP servisi.
 * HttpParams ile query string'ler tip güvenli oluşturulur;
 * string concatenation hatalarına karşı koruma sağlar.
 */
@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Tasks`;
  /**
   * Filtrelenmiş ve sayfalı görev listesi.
   * Tanımsız parametreler query string'e eklenmez (HttpParams null değerleri atlar).
   */
  getAll(filter: TaskFilter): Observable<ApiResponse<PagedResponse<TaskItem>>> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize', filter.pageSize.toString());
    if (filter.searchTerm) params = params.set('searchTerm', filter.searchTerm);
    if (filter.status !== undefined) params = params.set('status', filter.status.toString());
    if (filter.priority !== undefined) params = params.set('priority', filter.priority.toString());
    if (filter.startDate) params = params.set('startDate', filter.startDate);
    if (filter.endDate) params = params.set('endDate', filter.endDate);
    return this.http.get<ApiResponse<PagedResponse<TaskItem>>>(this.baseUrl, { params });
  }
  getById(id: string): Observable<ApiResponse<TaskItem>> {
    return this.http.get<ApiResponse<TaskItem>>(`${this.baseUrl}/${id}`);
  }
  create(request: CreateTaskRequest): Observable<ApiResponse<TaskItem>> {
    return this.http.post<ApiResponse<TaskItem>>(this.baseUrl, request);
  }
  update(id: string, request: UpdateTaskRequest): Observable<ApiResponse<TaskItem>> {
    return this.http.put<ApiResponse<TaskItem>>(`${this.baseUrl}/${id}`, request);
  }
  delete(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${id}`);
  }
  /** Yalnızca status alanını günceller; PATCH semantiği ile kısmi güncelleme yapılır. */
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
