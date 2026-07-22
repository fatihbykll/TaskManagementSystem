import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TokenService } from '../services/token.service';
/**
 * Fonksiyonel JWT interceptor (Angular 15+ stili; NgModule gerektirmez).
 *
 * Çalışma prensibi:
 *  1. TokenService'ten mevcut token okunur.
 *  2. Token varsa istek klonlanır ve Authorization header'ı eklenir.
 *  3. Auth endpoint'lerine (/Auth/login, /Auth/register) token eklenmez;
 *     bu istekler zaten token olmadan çalışmalıdır.
 */
export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const token = tokenService.getToken();
  // Login ve register istekleri header gerektirmez.
  const isAuthEndpoint =
    req.url.includes('/Auth/login') || req.url.includes('/Auth/register');
  if (token && !isAuthEndpoint) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }
  return next(req);
};
