import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from '../services/token.service';
/**
 * JWT token varlığını ve geçerliliğini kontrol eder.
 * TokenService.isLoggedIn() hem token varlığını hem expiry'yi kontrol eder.
 * isPlatformBrowser: SSR sırasında localStorage'a erişilmesini önler.
 */
export const authGuard: CanActivateFn = () => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);
  const tokenService = inject(TokenService);
  if (!isPlatformBrowser(platformId)) return false;
  if (tokenService.isLoggedIn()) return true;
  router.navigate(['/login']);
  return false;
};
