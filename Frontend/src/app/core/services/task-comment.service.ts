import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/api-response.model';
export interface TaskComment { id: string; taskId: string; text: string; createdAt: string; authorName: string; }
@Injectable({ providedIn: 'root' })
export class TaskCommentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Tasks`;
  getComments(taskId: string): Observable<ApiResponse<TaskComment[]>> {
    return this.http.get<ApiResponse<TaskComment[]>>(`${this.baseUrl}/${taskId}/comments`);
  }
  addComment(taskId: string, text: string): Observable<ApiResponse<TaskComment>> {
    return this.http.post<ApiResponse<TaskComment>>(`${this.baseUrl}/${taskId}/comments`, { text });
  }
}
