import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { BehaviorSubject } from 'rxjs';
/**
 * Dark/Light tema yönetimi.
 * Tercih localStorage'a kaydedilir; sayfa yenilemesinde korunur.
 * document.documentElement üzerinde .dark-theme sınıfı toggle edilir;
 * styles.scss'deki CSS değişkenleri bu sınıfa göre uygulanır.
 */
@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly THEME_KEY = 'app-theme';
  private readonly platformId = inject(PLATFORM_ID);
  private readonly isDark$ = new BehaviorSubject<boolean>(false);
  readonly isDarkMode$ = this.isDark$.asObservable();
  /** Uygulama başlangıcında AppComponent tarafından çağrılır. */
  initialize(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    const saved = localStorage.getItem(this.THEME_KEY);
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    this.applyTheme(saved ? saved === 'dark' : prefersDark);
  }
  toggle(): void {
    this.applyTheme(!this.isDark$.getValue());
  }
  private applyTheme(isDark: boolean): void {
    this.isDark$.next(isDark);
    document.documentElement.classList.toggle('dark-theme', isDark);
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.THEME_KEY, isDark ? 'dark' : 'light');
    }
  }
}
