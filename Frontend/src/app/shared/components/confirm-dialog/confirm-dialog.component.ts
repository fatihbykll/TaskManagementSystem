import { Component, inject } from '@angular/core';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  /** 'warn' → kırmızı buton, 'primary' → mavi buton */
  confirmColor?: 'warn' | 'primary';
  icon?: string;
}
/**
 * Yeniden kullanılabilir onay diyaloğu.
 * MatDialog.open(ConfirmDialogComponent, { data: {...} }) ile açılır.
 * Kullanıcı onaylarsa true, iptal ederse false döner.
 * Uygulama genelinde delete, cancel ve kritik işlemler için kullanılır.
 */
@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="confirm-dialog">
      <div class="dialog-icon" [class]="data.confirmColor ?? 'warn'">
        <mat-icon>{{ data.icon ?? 'warning' }}</mat-icon>
      </div>
      <h2 mat-dialog-title>{{ data.title }}</h2>
      <mat-dialog-content>
        <p>{{ data.message }}</p>
      </mat-dialog-content>
      <mat-dialog-actions align="end">
        <button mat-stroked-button [mat-dialog-close]="false">
          {{ data.cancelText ?? 'Vazgeç' }}
        </button>
        <button mat-flat-button
                [color]="data.confirmColor ?? 'warn'"
                [mat-dialog-close]="true">
          {{ data.confirmText ?? 'Onayla' }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .confirm-dialog { padding: 0.5rem; max-width: 400px; }
    .dialog-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 56px;
      height: 56px;
      border-radius: 50%;
      margin: 0 auto 1rem;
      mat-icon { font-size: 28px; width: 28px; height: 28px; color: white; }
      &.warn    { background: linear-gradient(135deg, #f44336, #c62828); }
      &.primary { background: linear-gradient(135deg, #667eea, #764ba2); }
    }
    h2 { text-align: center; margin: 0 0 0.5rem; font-size: 1.2rem; }
    mat-dialog-content p {
      text-align: center;
      color: #666;
      margin: 0;
      line-height: 1.5;
    }
    mat-dialog-actions { gap: 0.5rem; padding: 1rem 0 0; }
  `]
})
export class ConfirmDialogComponent {
  readonly data: ConfirmDialogData = inject(MAT_DIALOG_DATA);
  readonly dialogRef = inject(MatDialogRef<ConfirmDialogComponent>);
}
