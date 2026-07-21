import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError } from 'rxjs';
import { ErrorHandlingService } from '../services/error-handling.service';
/**
 * Global HTTP hata yakalayıcısı.
 * Her HTTP isteğinin ardından hata çıkarsa ErrorHandlingService'e delege eder.
 * Böylece her servis veya component'te ayrı catchError yazmak gerekmez (DRY).
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const errorService = inject(ErrorHandlingService);
  return next(req).pipe(
    catchError(err => errorService.handleError(err))
  );
};
