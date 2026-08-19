import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/api-response.model';
export interface AdminUser {
  id: string; username: string; email: string;
  firstName: string; lastName: string; role: string;
  isActive: boolean; createdAt: string; lastLoginAt?: string;
}
export interface AdminStats {
  totalUsers: number; totalTasks: number; activeUsers: number;
  completedTasksToday: number; pendingTasks: number;
}
export interface DailyReport {
  date: string; newTasks: number; completedTasks: number;
  activeUsers: number; overdueTaskCount: number;
}
@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/Admin`;
  getStatistics(): Observable<ApiResponse<AdminStats>> {
    return this.http.get<ApiResponse<AdminStats>>(`${this.base}/statistics`);
  }
  getUsers(): Observable<ApiResponse<AdminUser[]>> {
    return this.http.get<ApiResponse<AdminUser[]>>(`${this.base}/users`);
  }
  getDailyReport(): Observable<ApiResponse<DailyReport>> {
    return this.http.get<ApiResponse<DailyReport>>(`${this.base}/report/daily`);
  }
  deactivateUser(userId: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.base}/users/${userId}`);
  }
}
