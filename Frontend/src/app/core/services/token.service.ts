import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
/**
 * JWT token'ının LocalStorage üzerindeki tüm CRUD işlemlerini tek bir sınıfta toplar.
 * SSR uyumludur; sunucu ortamında localStorage'a erişilmez.
 * Servislerin doğrudan localStorage ile konuşması DRY prensibini ihlal ederdi.
 */
@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly TOKEN_KEY = 'auth_token';
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));
  getToken(): string | null {
    if (!this.isBrowser) return null;
    return localStorage.getItem(this.TOKEN_KEY);
  }
  setToken(token: string): void {
    if (!this.isBrowser) return;
    localStorage.setItem(this.TOKEN_KEY, token);
  }
  removeToken(): void {
    if (!this.isBrowser) return;
    localStorage.removeItem(this.TOKEN_KEY);
  }
  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;
    return !this.isTokenExpired(token);
  }
  /**
   * JWT payload'ını Base64 decode ederek exp claim'ini kontrol eder.
   * İmzayı doğrulamaz; bu Backend'in (JWT middleware) sorumluluğudur.
   */
  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      // exp Unix timestamp (saniye); Date.now() milisaniye döner.
      return payload.exp * 1000 < Date.now();
    } catch {
      return true;
    }
  }
}
