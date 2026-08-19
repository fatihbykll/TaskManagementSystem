import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TokenService } from './token.service';
import { SessionService } from './session.service';
import { ApiResponse } from '../../models/api-response.model';
import { User, LoginRequest, RegisterRequest, AuthResponse } from '../../models/user.model';
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenService = inject(TokenService);
  private readonly sessionService = inject(SessionService);
  private readonly router = inject(Router);
  private readonly baseUrl = `${environment.apiUrl}/Auth`;
  private readonly currentUserSubject = new BehaviorSubject<User | null>(null);
  readonly currentUser$ = this.currentUserSubject.asObservable();
  login(request: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/login`, request).pipe(
      tap(response => {
        if (response.success) {
          this.tokenService.setToken(response.data.accessToken);
          this.currentUserSubject.next(this.parseUserFromToken(response.data.accessToken));
          this.sessionService.startSession(response.data.accessToken);
        }
      })
    );
  }
  register(request: RegisterRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/register`, request).pipe(
      tap(response => {
        if (response.success) {
          this.tokenService.setToken(response.data.accessToken);
          this.currentUserSubject.next(this.parseUserFromToken(response.data.accessToken));
          this.sessionService.startSession(response.data.accessToken);
        }
      })
    );
  }
  /**
   * Backend ayrı bir /me endpoint'i sunmadığından, kullanıcı bilgileri
   * JWT payload'ından decode edilir. İmza doğrulaması Backend'e aittir.
   */
  private parseUserFromToken(token: string): User | null {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return {
        id:        payload['sub']         ?? '',
        email:     payload['email']       ?? '',
        username:  payload['unique_name'] ?? '',
        firstName: payload['given_name']  ?? payload['unique_name'] ?? '',
        lastName:  payload['family_name'] ?? '',
        role:      payload['role'] ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? 'User'
      };
    } catch {
      return null;
    }
  }
  logout(): void {
    this.tokenService.removeToken();
    this.currentUserSubject.next(null);
    this.sessionService.endSession();
    this.router.navigate(['/login']);
  }
  getCurrentUser(): User | null { return this.currentUserSubject.getValue(); }
  isLoggedIn(): boolean         { return this.tokenService.isLoggedIn(); }
  isAdmin(): boolean            { return this.getCurrentUser()?.role === 'Admin'; }
}
