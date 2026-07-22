import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TokenService } from './token.service';
import { ApiResponse } from '../../models/api-response.model';
import { User, LoginRequest, RegisterRequest, AuthResponse } from '../../models/user.model';
/**
 * Kimlik doğrulama servisi.
 *
 * currentUser$ (BehaviorSubject): Giriş yapan kullanıcı bilgisi reaktif olarak tutulur.
 * Tüm bileşenler bu stream'e subscribe olarak anlık kullanıcı durumunu alır.
 * BehaviorSubject seçilme sebebi: yeni subscribe olan bileşene anında son değeri iletir.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenService = inject(TokenService);
  private readonly router = inject(Router);
  private readonly baseUrl = `${environment.apiUrl}/Auth`;
  // null → giriş yapılmamış, User → giriş yapılmış
  private readonly currentUserSubject = new BehaviorSubject<User | null>(null);
  readonly currentUser$ = this.currentUserSubject.asObservable();
  /**
   * Kullanıcı adı ve şifre ile giriş yapar.
   * tap(): yan etki olarak token kaydedilir ve currentUser güncellenir;
   *        veri dönüşümü yapılmaz, zincir bozulmaz.
   */
  login(request: LoginRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/login`, request).pipe(
      tap(response => {
        if (response.success) {
          this.tokenService.setToken(response.data.token);
          this.currentUserSubject.next(response.data.user);
        }
      })
    );
  }
  /**
   * Yeni kullanıcı kaydı oluşturur.
   * Başarılı kayıt sonrası otomatik giriş için token set edilir.
   */
  register(request: RegisterRequest): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/register`, request).pipe(
      tap(response => {
        if (response.success) {
          this.tokenService.setToken(response.data.token);
          this.currentUserSubject.next(response.data.user);
        }
      })
    );
  }
  /** Token'ı temizler, kullanıcı state'ini sıfırlar ve login'e yönlendirir. */
  logout(): void {
    this.tokenService.removeToken();
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }
  getCurrentUser(): User | null {
    return this.currentUserSubject.getValue();
  }
  isLoggedIn(): boolean {
    return this.tokenService.isLoggedIn();
  }
}
