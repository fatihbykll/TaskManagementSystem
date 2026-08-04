import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SessionService } from './core/services/session.service';
import { ThemeService } from './core/services/theme.service';
import { NavbarComponent } from './layout/navbar/navbar.component';
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent],
  template: `
    <app-navbar>
      <router-outlet></router-outlet>
    </app-navbar>
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
