import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
/**
 * Merkezi HTTP hata yönetim servisi.
 *
 * Sorumlulukları:
 *  1. Backend'den dönen ApiResponse<T>.errors dizisini parse eder.
 *  2. HTTP durum koduna göre kullanıcıya anlamlı Türkçe mesaj gösterir.
 *  3. 401 durumunda token temizleyip /login'e yönlendirir (Auto-logout).
 *  4. Tüm hata akışlarını Observable<never> olarak zincire geri döner;
 *     her serviste ayrı try/catch yazmak zorunda kalınmaz (DRY prensibi).
 */
@Injectable({ providedIn: 'root' })
export class ErrorHandlingService {
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);
  /** Snackbar gösterim süresi (ms) */
  private readonly SNACK_DURATION = 4000;
  /**
   * HTTP hatasını işler ve Observable<never> döner.
   * Kullanım: catchError(err => this.errorService.handleError(err))
   */
  handleError(error: HttpErrorResponse): Observable<never> {
    const message = this.extractMessage(error);
    this.showError(message);
    if (error.status === 401) {
      localStorage.removeItem('auth_token');
      this.router.navigate(['/login']);
    }
    // Hata zincirini kesmeden devam ettirmek için throwError kullanılır.
    return throwError(() => error);
  }
  /**
   * Backend ApiResponse<T> formatını veya HTTP durum kodunu okuyarak
   * kullanıcıya gösterilecek Türkçe mesajı belirler.
   */
  private extractMessage(error: HttpErrorResponse): string {
    // Backend'den yapılandırılmış ApiResponse<T> döndüyse parse edilir.
    if (error.error && typeof error.error === 'object') {
      // Validation hatalar listesi varsa hepsini birleştir.
      if (Array.isArray(error.error.errors) && error.error.errors.length > 0) {
        return error.error.errors.join(' ');
      }
      // Tek mesaj varsa doğrudan kullan.
      if (error.error.message) {
        return error.error.message;
      }
    }
    // HTTP durum koduna göre varsayılan mesajlar.
    return this.getDefaultMessage(error.status);
  }
  /** HTTP durum koduna karşılık gelen kullanıcı dostu Türkçe mesajlar. */
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
  /** Angular Material Snackbar ile hata mesajı gösterir. */
  private showError(message: string): void {
    this.snackBar.open(message, 'Kapat', {
      duration: this.SNACK_DURATION,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: ['error-snackbar']
    });
  }
  /** Başarı mesajı göstermek için yardımcı metot (formlar sonrası kullanılır). */
  showSuccess(message: string): void {
    this.snackBar.open(message, 'Kapat', {
      duration: this.SNACK_DURATION,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: ['success-snackbar']
    });
  }
}
