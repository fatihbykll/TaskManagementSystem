import { Component, Input, OnInit, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TaskAttachmentService, TaskAttachment } from '../../../core/services/task-attachment.service';
import { ErrorHandlingService } from '../../../core/services/error-handling.service';
@Component({
  selector: 'app-task-attachments',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatProgressBarModule, MatTooltipModule],
  template: `
    <div class="attachments-section">
      <h3><mat-icon>attach_file</mat-icon> Dosya Ekleri</h3>
      <!-- Yükleme alanı -->
      <div class="upload-area"
           (click)="fileInput.click()"
           (dragover)="$event.preventDefault()"
           (drop)="onDrop($event)">
        <input type="file" #fileInput (change)="onFileSelected($event)"
               style="display:none" multiple accept="*/*">
        <mat-icon>cloud_upload</mat-icon>
        <p>Dosyaları sürükleyin veya <strong>tıklayın</strong></p>
        <span class="upload-hint">Maksimum 10 MB</span>
      </div>
      @if (isUploading) {
        <mat-progress-bar mode="indeterminate" class="upload-bar"></mat-progress-bar>
      }
      <!-- Dosya listesi -->
      <div class="file-list">
        @for (att of attachments; track att.id) {
          <div class="file-item">
            <mat-icon class="file-icon">insert_drive_file</mat-icon>
            <div class="file-info">
              <span class="file-name">{{ att.fileName }}</span>
              <span class="file-size">{{ formatSize(att.fileSize) }}</span>
            </div>
            <button mat-icon-button
                    matTooltip="İndir"
                    (click)="download(att)">
              <mat-icon>download</mat-icon>
            </button>
          </div>
        }
        @if (!isLoading && attachments.length === 0) {
          <p class="no-files">Henüz dosya eklenmemiş.</p>
        }
      </div>
    </div>
  `,
  styles: [`
    .attachments-section { }
    h3 { display:flex; align-items:center; gap:.5rem; font-size:1.1rem; color:#555; margin-top:0; }
    .upload-area {
      border: 2px dashed #ccc; border-radius: 8px; padding: 1.5rem;
      text-align: center; cursor: pointer; transition: all .2s; margin-bottom: .75rem;
      mat-icon { font-size: 2.5rem; width: 2.5rem; height: 2.5rem; color: #999; }
      p { margin: .5rem 0 0; color: #666; }
      .upload-hint { font-size: .75rem; color: #aaa; }
      &:hover { background: #f5f5f5; border-color: #667eea; }
    }
    .upload-bar { margin-bottom: .75rem; border-radius: 4px; }
    .file-list { display: flex; flex-direction: column; gap: .5rem; }
    .file-item {
      display: flex; align-items: center; gap: .75rem;
      background: #e3f2fd; padding: .5rem 1rem; border-radius: 6px;
      .file-icon { color: #1565c0; }
    }
    .file-info { flex: 1; display: flex; flex-direction: column; }
    .file-name { font-weight: 500; color: #1565c0; font-size: .9rem; }
    .file-size { font-size: .75rem; color: #666; }
    .no-files { color: #aaa; font-style: italic; font-size: .9rem; }
  `]
})
export class TaskAttachmentsComponent implements OnInit {
  @Input({ required: true }) taskId!: string;
  private readonly attachmentService = inject(TaskAttachmentService);
  private readonly notificationService = inject(ErrorHandlingService);
  private readonly destroyRef = inject(DestroyRef);
  attachments: TaskAttachment[] = [];
  isLoading = false;
  isUploading = false;
  ngOnInit(): void { this.loadAttachments(); }
  loadAttachments(): void {
    this.isLoading = true;
    this.attachmentService.getAttachments(this.taskId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => { if (res.success) this.attachments = res.data; this.isLoading = false; },
        error: () => { this.isLoading = false; }
      });
  }
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) this.uploadFiles(Array.from(input.files));
  }
  onDrop(event: DragEvent): void {
    event.preventDefault();
    const files = Array.from(event.dataTransfer?.files ?? []);
    if (files.length) this.uploadFiles(files);
  }
  private uploadFiles(files: File[]): void {
    this.isUploading = true;
    let completed = 0;
    files.forEach(file => {
      this.attachmentService.uploadFile(this.taskId, file)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: res => {
            if (res.success) {
              this.attachments.push(res.data);
              this.notificationService.showSuccess(`"${file.name}" yüklendi.`);
            }
            if (++completed === files.length) this.isUploading = false;
          },
          error: () => { if (++completed === files.length) this.isUploading = false; }
        });
    });
  }
  download(att: TaskAttachment): void {
    this.attachmentService.downloadFile(this.taskId, att.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(blob => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url; a.download = att.fileName; a.click();
        URL.revokeObjectURL(url);
      });
  }
  formatSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }
}
