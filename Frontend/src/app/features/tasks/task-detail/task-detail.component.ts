import { Component, inject, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TaskService } from '../../../core/services/task.service';
import { ErrorHandlingService } from '../../../core/services/error-handling.service';
import { TaskFormComponent, TaskFormDialogData } from '../task-form/task-form.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { TaskItem, TaskStatus, TaskPriority } from '../../../models/task.model';
import { TaskCommentsComponent } from '../task-comments/task-comments.component';
import { TaskAttachmentsComponent } from '../task-attachments/task-attachments.component';
const PRIORITY_META: Record<TaskPriority, { label: string; color: string }> = {
  [TaskPriority.Low]:      { label: 'Düşük',  color: '#2e7d32' },
  [TaskPriority.Medium]:   { label: 'Orta',   color: '#e65100' },
  [TaskPriority.High]:     { label: 'Yüksek', color: '#c62828' },
  [TaskPriority.Critical]: { label: 'Kritik', color: '#6a1b9a' }
};
const STATUS_META: Record<TaskStatus, { label: string; color: string; icon: string }> = {
  [TaskStatus.Pending]:    { label: 'Beklemede',    color: '#757575', icon: 'schedule' },
  [TaskStatus.InProgress]: { label: 'Devam Ediyor', color: '#1565c0', icon: 'autorenew' },
  [TaskStatus.Completed]:  { label: 'Tamamlandı',   color: '#2e7d32', icon: 'check_circle' },
  [TaskStatus.Cancelled]:  { label: 'İptal',        color: '#b71c1c', icon: 'cancel' }
};
@Component({
  selector: 'app-task-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    TaskCommentsComponent,
    TaskAttachmentsComponent
  ],
  templateUrl: './task-detail.component.html',
  styleUrl: './task-detail.component.scss'
})
export class TaskDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly taskService = inject(TaskService);
  private readonly dialog = inject(MatDialog);
  private readonly notificationService = inject(ErrorHandlingService);
  private readonly destroyRef = inject(DestroyRef); // Memory leak koruması için Angular 16+ DestroyRef
  task: TaskItem | null = null;
  isLoading = true;
  get priority() { return this.task ? PRIORITY_META[this.task.priority] : null; }
  get status()   { return this.task ? STATUS_META[this.task.status]   : null; }
  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) { this.router.navigate(['/tasks']); return; }
    this.taskService.getById(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          if (res.success) this.task = res.data;
          else this.router.navigate(['/tasks']);
          this.isLoading = false;
        },
        error: () => { this.isLoading = false; this.router.navigate(['/tasks']); }
      });
  }
  openEditDialog(): void {
    const ref = this.dialog.open(TaskFormComponent, {
      data: { task: this.task } as TaskFormDialogData,
      width: '560px',
      disableClose: true
    });
    
    ref.afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(updated => {
        if (updated && this.task) this.ngOnInit(); // Sayfayı yenile
      });
  }
  openDeleteDialog(): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Görevi Sil',
        message: `"${this.task?.title}" görevini kalıcı olarak silmek istediğinize emin misiniz?`,
        confirmText: 'Sil',
        cancelText: 'Vazgeç',
        confirmColor: 'warn',
        icon: 'delete_forever'
      } as ConfirmDialogData,
      width: '420px'
    });
    
    ref.afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(confirmed => {
        if (confirmed && this.task) {
          this.taskService.delete(this.task.id)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
              next: res => {
                if (res.success) {
                  this.notificationService.showSuccess('Görev silindi.');
                  this.router.navigate(['/tasks']);
                }
              }
            });
        }
      });
  }
  goBack(): void { this.router.navigate(['/tasks']); }
}
