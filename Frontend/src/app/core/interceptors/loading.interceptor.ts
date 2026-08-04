import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { LoadingService } from '../services/loading.service';
/**
 * Her HTTP isteği başladığında LoadingService.show(),
 * tamamlandığında (başarı veya hata) LoadingService.hide() çağrılır.
 * finalize() operatörü hem success hem error akışında tetiklenir.
 */
export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loadingService = inject(LoadingService);
  loadingService.show();
  return next(req).pipe(finalize(() => loadingService.hide()));
};
