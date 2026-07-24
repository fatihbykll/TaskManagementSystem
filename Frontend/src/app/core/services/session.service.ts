import { Injectable, inject, OnDestroy, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { TokenService } from './token.service';
/**
 * Proaktif oturum yönetim servisi.
 *
 * ErrorInterceptor'daki 401 kontrolü reaktif çalışır: kullanıcı bir istek
 * attığında sunucu 401 dönerse çıkış yapılır. Ancak kullanıcı istekler
 * arasında pasif bekliyorsa token fark edilmeden süresi dolabilir.
 *
 * Bu servis token'ın exp claim'inden kalan süreyi hesaplayarak bir
 * setTimeout kurar; süre dolduğunda kullanıcıyı proaktif olarak
 * sisteme bildirmeden login sayfasına yönlendirir.
 */
@Injectable({ providedIn: 'root' })
export class SessionService implements OnDestroy {
  private readonly tokenService = inject(TokenService);
  private readonly router = inject(Router);
  private readonly platformId = inject(PLATFORM_ID);
  private logoutTimer: ReturnType<typeof setTimeout> | null = null;
  /**
   * Uygulama başlangıcında çağrılır (AppComponent.ngOnInit).
   * Mevcut token varsa zamanlayıcıyı kur; yoksa sessizce çık.
   */
  initialize(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    const token = this.tokenService.getToken();
    if (token && !this.tokenService.isLoggedIn()) {
      // Uygulama açıldığında token zaten süresi dolmuşsa anında çıkış yap.
      this.clearSession();
      return;
    }
    if (token) {
      this.scheduleAutoLogout(token);
    }
  }
  /**
   * Başarılı giriş sonrası çağrılır.
   * Önceki zamanlayıcı temizlenir, yeni token için yeniden kurulur.
   */
  startSession(token: string): void {
    this.clearTimer();
    this.scheduleAutoLogout(token);
  }
  /** Çıkış yapıldığında zamanlayıcıyı temizler. */
  endSession(): void {
    this.clearTimer();
  }
  /**
   * Token'ın exp alanından kalan milisaniyeyi hesaplar ve setTimeout kurar.
   * Maximum 2147483647ms (~24.8 gün) ile sınırlandırılmıştır;
   * setTimeout, bu değeri aşan sayıları yanlış işler.
   */
  private scheduleAutoLogout(token: string): void {
    const remainingMs = this.getRemainingMs(token);
    if (remainingMs <= 0) {
      this.clearSession();
      return;
    }
    const MAX_TIMEOUT = 2_147_483_647;
    const delay = Math.min(remainingMs, MAX_TIMEOUT);
    this.logoutTimer = setTimeout(() => {
      this.clearSession();
    }, delay);
  }
  /**
   * JWT payload'ındaki exp claim'den kalan süreyi milisaniye cinsinden döner.
   * Hatalı token formatında 0 döner; güvenli başarısızlık (fail-safe).
   */
  private getRemainingMs(token: string): number {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp * 1000 - Date.now();
    } catch {
      return 0;
    }
  }
  /** Token ve zamanlayıcıyı temizleyip login'e yönlendirir. */
  private clearSession(): void {
    this.clearTimer();
    this.tokenService.removeToken();
    this.router.navigate(['/login']);
  }
  private clearTimer(): void {
    if (this.logoutTimer !== null) {
      clearTimeout(this.logoutTimer);
      this.logoutTimer = null;
    }
  }
  ngOnDestroy(): void {
    this.clearTimer();
  }
}
