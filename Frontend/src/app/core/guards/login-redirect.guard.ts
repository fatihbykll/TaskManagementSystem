import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from '../services/token.service';
/**
 * Zaten oturum açmış kullanıcının /login veya /register'a erişimini önler.
 * Kullanıcı bu sayfalara gitmeye çalışırsa /tasks'a yönlendirilir.
 * authGuard'ın tam tersi mantıkla çalışır.
 */
export const loginRedirectGuard: CanActivateFn = () => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);
  const tokenService = inject(TokenService);
  if (!isPlatformBrowser(platformId)) return true;
  if (tokenService.isLoggedIn()) {
    router.navigate(['/tasks']);
    return false;
  }
  return true;
};
