import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { TaskService } from '../../../core/services/task.service';
import { ErrorHandlingService } from '../../../core/services/error-handling.service';
import { TaskCardComponent, TaskCardAction } from '../task-card/task-card.component';
import { TaskFormComponent, TaskFormDialogData } from '../task-form/task-form.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { TaskItem, TaskStatus, TaskPriority, TaskFilter } from '../../../models/task.model';
import { PagedResponse } from '../../../models/api-response.model';
@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatFormFieldModule, MatInputModule, MatSelectModule,
    MatButtonModule, MatIconModule, MatPaginatorModule,
    MatProgressSpinnerModule, MatDialogModule, TaskCardComponent
  ],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.scss'
})
export class TaskListComponent implements OnInit, OnDestroy {
  private readonly taskService  = inject(TaskService);
  private readonly notification = inject(ErrorHandlingService);
  private readonly dialog       = inject(MatDialog);
  private readonly router       = inject(Router);
  private readonly destroy$     = new Subject<void>();
  tasks: TaskItem[] = [];
  pagedData: PagedResponse<TaskItem> | null = null;
  isLoading = false;
  readonly searchControl   = new FormControl('');
  readonly statusControl   = new FormControl<TaskStatus | ''>('');
  readonly priorityControl = new FormControl<TaskPriority | ''>('');
  readonly TaskStatus   = TaskStatus;
  readonly TaskPriority = TaskPriority;
  readonly statusOptions   = [
    { value: TaskStatus.Pending,    label: 'Beklemede' },
    { value: TaskStatus.InProgress, label: 'Devam Ediyor' },
    { value: TaskStatus.Completed,  label: 'Tamamlandı' },
    { value: TaskStatus.Cancelled,  label: 'İptal' }
  ];
  readonly priorityOptions = [
    { value: TaskPriority.Low,      label: 'Düşük' },
    { value: TaskPriority.Medium,   label: 'Orta' },
    { value: TaskPriority.High,     label: 'Yüksek' },
    { value: TaskPriority.Critical, label: 'Kritik' }
  ];
  currentPage = 0;
  pageSize    = 9;
  ngOnInit(): void {
    this.searchControl.valueChanges.pipe(
      debounceTime(400), distinctUntilChanged(), takeUntil(this.destroy$)
    ).subscribe(() => { this.currentPage = 0; this.loadTasks(); });
    this.statusControl.valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe(() => { this.currentPage = 0; this.loadTasks(); });
    this.priorityControl.valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe(() => { this.currentPage = 0; this.loadTasks(); });
    this.loadTasks();
  }
  loadTasks(): void {
    this.isLoading = true;
    const filter: TaskFilter = {
      pageNumber: this.currentPage + 1,
      pageSize:   this.pageSize,
      searchTerm: this.searchControl.value || undefined,
      status:     this.statusControl.value   !== '' ? this.statusControl.value!   : undefined,
      priority:   this.priorityControl.value !== '' ? this.priorityControl.value! : undefined
    };
    this.taskService.getAll(filter).subscribe({
      next: res => {
        if (res.success) { this.tasks = res.data.items; this.pagedData = res.data; }
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }
  openCreateDialog(): void {
    const ref = this.dialog.open(TaskFormComponent, {
      data: {} as TaskFormDialogData, width: '560px', disableClose: true
    });
    ref.afterClosed().subscribe(created => { if (created) this.loadTasks(); });
  }
  onTaskAction(event: TaskCardAction): void {
    if (event.type === 'edit') {
      const ref = this.dialog.open(TaskFormComponent, {
        data: { task: event.task } as TaskFormDialogData, width: '560px', disableClose: true
      });
      ref.afterClosed().subscribe(updated => { if (updated) this.loadTasks(); });
    } else if (event.type === 'delete') {
      const ref = this.dialog.open(ConfirmDialogComponent, {
        data: {
          title: 'Görevi Sil',
          message: `"${event.task.title}" görevini silmek istediğinize emin misiniz?`,
          confirmText: 'Sil', confirmColor: 'warn', icon: 'delete_forever'
        } as ConfirmDialogData, width: '420px'
      });
      ref.afterClosed().subscribe(confirmed => {
        if (confirmed) {
          this.taskService.delete(event.task.id).subscribe({
            next: res => { if (res.success) { this.notification.showSuccess('Görev silindi.'); this.loadTasks(); } }
          });
        }
      });
    } else if (event.type === 'status' && event.newStatus !== undefined) {
      this.taskService.updateStatus(event.task.id, event.newStatus).subscribe({
        next: res => { if (res.success) { this.notification.showSuccess('Durum güncellendi.'); this.loadTasks(); } }
      });
    }
  }
  navigateToDetail(taskId: string): void {
    this.router.navigate(['/tasks', taskId]);
  }
  onPageChange(e: PageEvent): void {
    this.currentPage = e.pageIndex;
    this.pageSize    = e.pageSize;
    this.loadTasks();
  }
  clearFilters(): void {
    this.searchControl.reset('');
    this.statusControl.reset('');
    this.priorityControl.reset('');
  }
  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
