import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivateFn, Router } from '@angular/router';

/**
 * JWT token varlığını kontrol eder.
 * isPlatformBrowser ile SSR ortamında localStorage'a erişim önlenir;
 * server-side render sırasında token yokmuş gibi davranılır.
 */
export const authGuard: CanActivateFn = () => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);

  if (!isPlatformBrowser(platformId)) return false;

  const token = localStorage.getItem('auth_token');
  if (token) return true;

  router.navigate(['/login']);
  return false;
};
