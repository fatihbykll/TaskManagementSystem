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
          this.tokenService.setToken(response.data.token);
          this.currentUserSubject.next(response.data.user);
          // Token alındıktan sonra proaktif auto-logout zamanlayıcısını kur.
          this.sessionService.startSession(response.data.token);
        }
      })
    );
  }
  register(request: RegisterRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/register`, request).pipe(
      tap(response => {
        if (response.success) {
          this.tokenService.setToken(response.data.token);
          this.currentUserSubject.next(response.data.user);
          this.sessionService.startSession(response.data.token);
        }
      })
    );
  }
  logout(): void {
    this.tokenService.removeToken();
    this.currentUserSubject.next(null);
    // Zamanlayıcıyı temizle; süresi dolmadan çıkış yapılıyor.
    this.sessionService.endSession();
    this.router.navigate(['/login']);
  }
  getCurrentUser(): User | null {
    return this.currentUserSubject.getValue();
  }
  isLoggedIn(): boolean {
    return this.tokenService.isLoggedIn();
  }
}
