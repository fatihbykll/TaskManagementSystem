import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SessionService } from './core/services/session.service';
import { ThemeService } from './core/services/theme.service';
import { NavbarComponent } from './layout/navbar/navbar.component';
import { NotificationToastComponent } from './shared/components/notification-toast/notification-toast.component';
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent, NotificationToastComponent],
  template: `
    <app-navbar>
      <router-outlet></router-outlet>
    </app-navbar>
    <app-notification-toast></app-notification-toast>
  `
})
export class AppComponent implements OnInit {
  private readonly sessionService = inject(SessionService);
  private readonly themeService = inject(ThemeService);
  ngOnInit(): void {
    this.sessionService.initialize();
    this.themeService.initialize();
  }
}
