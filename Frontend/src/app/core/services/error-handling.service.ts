import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { TokenService } from './token.service';
/**
 * Merkezi HTTP hata yönetim servisi.
 * 401 durumunda direkt localStorage'a dokunmak yerine TokenService
 * üzerinden removeToken() çağrılır — SSR uyumlu, DRY prensibine uygun.
 */
@Injectable({ providedIn: 'root' })
export class ErrorHandlingService {
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);
  private readonly tokenService = inject(TokenService); // ← Fix: localStorage bypass yerine
  private readonly SNACK_DURATION = 4000;
  handleError(error: HttpErrorResponse): Observable<never> {
    const message = this.extractMessage(error);
    this.showError(message);
    if (error.status === 401) {
      this.tokenService.removeToken(); // ← Fix: isPlatformBrowser'ı TokenService yönetir
      this.router.navigate(['/login']);
    }
    return throwError(() => error);
  }
  private extractMessage(error: HttpErrorResponse): string {
    if (error.error && typeof error.error === 'object') {
      if (Array.isArray(error.error.errors) && error.error.errors.length > 0) {
        return error.error.errors.join(' ');
      }
      if (error.error.message) {
        return error.error.message;
      }
    }
    return this.getDefaultMessage(error.status);
  }
  private getDefaultMessage(status: number): string {
    const messages: Record<number, string> = {
      0:   'Sunucuya ulaşılamıyor. Lütfen internet bağlantınızı kontrol edin.',
      400: 'Geçersiz istek. Lütfen girdiğiniz bilgileri kontrol edin.',
      401: 'Oturumunuz sona erdi. Lütfen tekrar giriş yapın.',
      403: 'Bu işlemi yapmaya yetkiniz bulunmuyor.',
      404: 'Aradığınız kaynak bulunamadı.',
      409: 'Bu kayıt zaten mevcut.',
      422: 'Girilen veriler işlenemiyor. Lütfen bilgileri kontrol edin.',
      500: 'Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyin.',
      503: 'Servis şu an kullanılamıyor. Lütfen daha sonra tekrar deneyin.'
    };
    return messages[status] ?? 'Beklenmedik bir hata oluştu.';
  }
  showError(message: string): void {
    this.snackBar.open(message, 'Kapat', {
      duration: this.SNACK_DURATION,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: ['error-snackbar']
    });
  }
  showSuccess(message: string): void {
    this.snackBar.open(message, 'Kapat', {
      duration: this.SNACK_DURATION,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: ['success-snackbar']
    });
  }
}
