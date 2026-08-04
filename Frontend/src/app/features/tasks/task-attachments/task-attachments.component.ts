import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
@Component({
  selector: 'app-task-attachments',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatProgressBarModule],
  template: `
    <div class="attachments-section">
      <h3><mat-icon>attach_file</mat-icon> Dosya Ekleri</h3>
      
      <div class="upload-area" (click)="fileInput.click()">
        <input type="file" #fileInput (change)="onFileSelected($event)" style="display:none" multiple>
        <mat-icon>cloud_upload</mat-icon>
        <p>Dosyaları yüklemek için tıklayın veya sürükleyin</p>
      </div>
      @if (isUploading) {
        <mat-progress-bar mode="indeterminate"></mat-progress-bar>
      }
      <div class="file-list">
        <!-- Backend entegrasyonu simülasyonu -->
        @for (file of files; track file.name) {
          <div class="file-item">
            <mat-icon>insert_drive_file</mat-icon>
            <span class="file-name">{{ file.name }}</span>
            <span class="file-size">{{ (file.size / 1024).toFixed(1) }} KB</span>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .attachments-section { margin-top: 1.5rem; }
    h3 { display: flex; align-items: center; gap: 0.5rem; font-size: 1.1rem; color: #555; }
    .upload-area { border: 2px dashed #ccc; border-radius: 8px; padding: 2rem; text-align: center; cursor: pointer; transition: background 0.2s; margin-bottom: 1rem; }
    .upload-area:hover { background: #f5f5f5; border-color: #667eea; }
    .upload-area mat-icon { font-size: 3rem; width: 3rem; height: 3rem; color: #888; }
    .upload-area p { color: #666; margin-top: 0.5rem; }
    .file-list { display: flex; flex-direction: column; gap: 0.5rem; margin-top: 1rem; }
    .file-item { display: flex; align-items: center; gap: 0.75rem; background: #e3f2fd; padding: 0.5rem 1rem; border-radius: 4px; }
    .file-name { flex: 1; font-weight: 500; color: #1565c0; }
    .file-size { font-size: 0.8rem; color: #666; }
  `]
})
export class TaskAttachmentsComponent {
  @Input({ required: true }) taskId!: string;
  files: File[] = [];
  isUploading = false;
  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.isUploading = true;
      // Gerçek senaryoda burada FormData oluşturulup Backend'e POST isteği atılır.
      setTimeout(() => {
        this.files.push(...Array.from(input.files!));
        this.isUploading = false;
      }, 1000); // 1 sn simülasyon
    }
  }
}
