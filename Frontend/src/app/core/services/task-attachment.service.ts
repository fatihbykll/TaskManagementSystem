import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/api-response.model';
export interface TaskAttachment {
  id: string;
  taskId: string;
  fileName: string;
  fileSize: number;
  contentType: string;
  uploadedAt: string;
  downloadUrl: string;
}
/**
 * Görev eklenti (dosya) işlemlerini Backend ile konuşturan servis.
 * Route: api/tasks/{taskId}/attachments
 */
@Injectable({ providedIn: 'root' })
export class TaskAttachmentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/tasks`;
  getAttachments(taskId: string): Observable<ApiResponse<TaskAttachment[]>> {
    return this.http.get<ApiResponse<TaskAttachment[]>>(
      `${this.baseUrl}/${taskId}/attachments`
    );
  }
  /**
   * Dosyayı FormData olarak gönderir.
   * Backend: POST api/tasks/{taskId}/attachments — [FromForm] IFormFile file
   */
  uploadFile(taskId: string, file: File): Observable<ApiResponse<TaskAttachment>> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<ApiResponse<TaskAttachment>>(
      `${this.baseUrl}/${taskId}/attachments`,
      formData
    );
  }
  downloadFile(taskId: string, attachmentId: string): Observable<Blob> {
    return this.http.get(
      `${this.baseUrl}/${taskId}/attachments/${attachmentId}/download`,
      { responseType: 'blob' }
    );
  }
}
