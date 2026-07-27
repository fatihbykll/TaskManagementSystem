import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
/**
 * Global HTTP yükleme durumu yönetimi.
 * Counter tabanlıdır; eş zamanlı birden fazla istek olsa bile
 * hepsi tamamlanmadan spinner kapanmaz.
 */
@Injectable({ providedIn: 'root' })
export class LoadingService {
  private counter = 0;
  private readonly loading$ = new BehaviorSubject<boolean>(false);
  readonly isLoading$ = this.loading$.asObservable();
  show(): void {
    this.counter++;
    this.loading$.next(true);
  }
  hide(): void {
    this.counter = Math.max(0, this.counter - 1);
    if (this.counter === 0) this.loading$.next(false);
  }
}
