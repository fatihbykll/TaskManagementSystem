import { Component, inject, ViewChild } from '@angular/core';
import { AsyncPipe, CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatSidenavModule, MatSidenav } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDividerModule } from '@angular/material/divider';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Observable, map, shareReplay } from 'rxjs';
import { NotificationService, AppNotification } from '../../core/services/notification.service';
import { ThemeService } from '../../core/services/theme.service';
import { LoadingService } from '../../core/services/loading.service';
import { AuthService } from '../../core/services/auth.service';
interface NavItem { label: string; icon: string; route: string; }
@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    CommonModule, AsyncPipe, RouterLink, RouterLinkActive,
    MatSidenavModule, MatToolbarModule, MatButtonModule, MatIconModule,
    MatListModule, MatMenuModule, MatBadgeModule, MatTooltipModule, MatProgressBarModule,
    MatDividerModule
  ],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss'
})
export class NavbarComponent {
  @ViewChild('sidenav') sidenav!: MatSidenav;
  private readonly breakpointObserver = inject(BreakpointObserver);
  readonly themeService = inject(ThemeService);
  readonly notificationService = inject(NotificationService);
  readonly notifications$ = this.notificationService.notifications$;
  readonly loadingService = inject(LoadingService);
  private readonly authService = inject(AuthService);
  readonly isDarkMode$ = this.themeService.isDarkMode$;
  readonly isLoading$ = this.loadingService.isLoading$;
  readonly isMobile$: Observable<boolean> = this.breakpointObserver
    .observe([Breakpoints.Handset, Breakpoints.TabletPortrait])
    .pipe(map(r => r.matches), shareReplay(1));
  readonly navItems: NavItem[] = [
    { label: 'Dashboard',   icon: 'dashboard', route: '/dashboard' },
    { label: 'Görevler',    icon: 'task_alt',  route: '/tasks' },
    { label: 'Kategoriler', icon: 'category',  route: '/categories' }
  ];
  get isLoggedIn(): boolean { return this.authService.isLoggedIn(); }
  get currentUser()         { return this.authService.getCurrentUser(); }
  toggleTheme(): void { this.themeService.toggle(); }
  logout(): void      { this.authService.logout(); }
  /**
   * (click) event'inde async pipe kullanılamaz.
   * Mobil ekranda nav öğesine tıklanınca sidenav'ı kapat.
   */
  onNavItemClick(): void {
    this.breakpointObserver
      .observe([Breakpoints.Handset, Breakpoints.TabletPortrait])
      .pipe(map(r => r.matches))
      .subscribe(isMobile => {
        if (isMobile) this.sidenav.close();
      })
      .unsubscribe();
  }
}
