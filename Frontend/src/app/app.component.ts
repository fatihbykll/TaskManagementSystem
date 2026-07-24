import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SessionService } from './core/services/session.service';
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  private readonly sessionService = inject(SessionService);
  /**
   * Uygulama başlangıcında SessionService initialize edilir.
   * Sayfa yenilendiğinde mevcut token kontrol edilip zamanlayıcı yeniden kurulur.
   * Bu olmadan tarayıcıyı yenileme auto-logout zamanlayıcısını sıfırlardı.
   */
  ngOnInit(): void {
    this.sessionService.initialize();
  }
}
