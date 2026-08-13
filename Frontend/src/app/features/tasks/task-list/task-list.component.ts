import { ExportService } from '../../../core/services/export.service';
import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { TaskService } from '../../../core/services/task.service';
import { ErrorHandlingService } from '../../../core/services/error-handling.service';
import { TaskCardComponent, TaskCardAction } from '../task-card/task-card.component';
import { TaskBoardComponent } from '../task-board/task-board.component';
import { TaskFormComponent, TaskFormDialogData } from '../task-form/task-form.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { TaskItem, TaskStatus, TaskPriority, TaskFilter, RecurringFrequency } from '../../../models/task.model';
import { PagedResponse } from '../../../models/api-response.model';
type SortField = 'title' | 'dueDate' | 'priority' | 'createdAt';
type ViewMode  = 'grid' | 'board';
@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatFormFieldModule, MatInputModule, MatSelectModule,
    MatButtonModule, MatButtonToggleModule, MatIconModule,
    MatPaginatorModule, MatProgressSpinnerModule,
    MatDialogModule, MatTooltipModule,
    TaskCardComponent, TaskBoardComponent
  ],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.scss'
})
export class TaskListComponent implements OnInit, OnDestroy {
  private readonly taskService  = inject(TaskService);
  private readonly notification = inject(ErrorHandlingService);
  private readonly dialog       = inject(MatDialog);
  private readonly router       = inject(Router);
  private readonly exportService = inject(ExportService);
  private readonly destroy$     = new Subject<void>();
  tasks: TaskItem[] = [];
  allTasks: TaskItem[] = [];   // Board view için tüm görevler
  pagedData: PagedResponse<TaskItem> | null = null;
  isLoading = false;
  viewMode: ViewMode = 'grid';
  readonly searchControl   = new FormControl('');
  readonly statusControl   = new FormControl<TaskStatus | ''>('');
  readonly priorityControl = new FormControl<TaskPriority | ''>('');
  readonly recurringOnlyControl = new FormControl<boolean>(false);
  readonly RecurringFrequency = RecurringFrequency;
  sortBy: SortField       = 'createdAt';
  sortDirection: 'asc' | 'desc' = 'desc';
  currentPage = 0;
  pageSize    = 9;
  readonly TaskStatus = TaskStatus; readonly TaskPriority = TaskPriority;
  readonly statusOptions   = [
    { value: TaskStatus.Pending,    label: 'Beklemede' },
    { value: TaskStatus.InProgress, label: 'Devam Ediyor' },
    { value: TaskStatus.Completed,  label: 'Tamamlandı' },
    { value: TaskStatus.Cancelled,  label: 'İptal' }
  ];
  readonly priorityOptions = [
    { value: TaskPriority.Low, label: 'Düşük' }, { value: TaskPriority.Medium, label: 'Orta' },
    { value: TaskPriority.High, label: 'Yüksek' }, { value: TaskPriority.Critical, label: 'Kritik' }
  ];
  readonly sortOptions: { value: SortField; label: string }[] = [
    { value: 'createdAt', label: 'Oluşturma Tarihi' },
    { value: 'dueDate',   label: 'Bitiş Tarihi' },
    { value: 'priority',  label: 'Öncelik' },
    { value: 'title',     label: 'Başlık' }
  ];
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
    if (this.viewMode === 'board') {
      // Board modda tüm görevleri çek (max 200), sayfalama yok
      const filter: TaskFilter = { pageNumber: 1, pageSize: 200 };
      this.taskService.getAll(filter).subscribe({
        next: res => { if (res.success) this.allTasks = res.data.items; this.isLoading = false; },
        error: () => { this.isLoading = false; }
      });
    } else {
      const filter: TaskFilter = {
        pageNumber: this.currentPage + 1, pageSize: this.pageSize,
        searchTerm: this.searchControl.value || undefined,
        status:   this.statusControl.value   !== '' ? this.statusControl.value!   : undefined,
        priority: this.priorityControl.value !== '' ? this.priorityControl.value! : undefined,
        sortBy: this.sortBy, sortDirection: this.sortDirection
      };
      this.taskService.getAll(filter).subscribe({
        next: res => {
          if (res.success) { this.tasks = res.data.items; this.pagedData = res.data; }
          this.isLoading = false;
        },
        error: () => { this.isLoading = false; }
      });
    }
  }
  setView(mode: ViewMode): void {
    this.viewMode = mode;
    this.loadTasks();
  }
  toggleSort(field: SortField): void {
    if (this.sortBy === field) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = field; this.sortDirection = 'asc';
    }
    this.currentPage = 0;
    this.loadTasks();
  }
  onBoardStatusChanged(event: { taskId: string; newStatus: TaskStatus }): void {
    this.taskService.updateStatus(event.taskId, event.newStatus).subscribe({
      next: res => { if (res.success) this.notification.showSuccess('Görev taşındı.'); },
      error: () => { this.loadTasks(); } // Hata durumunda geri al
    });
  }
  openCreateDialog(): void {
    const ref = this.dialog.open(TaskFormComponent, {
      data: {} as TaskFormDialogData, width: '560px', disableClose: true
    });
    ref.afterClosed().subscribe(c => { if (c) this.loadTasks(); });
  }
  onTaskAction(event: TaskCardAction): void {
    if (event.type === 'edit') {
      this.dialog.open(TaskFormComponent, {
        data: { task: event.task } as TaskFormDialogData, width: '560px', disableClose: true
      }).afterClosed().subscribe(u => { if (u) this.loadTasks(); });
    } else if (event.type === 'delete') {
      this.dialog.open(ConfirmDialogComponent, {
        data: { title: 'Görevi Sil', message: `"${event.task.title}" silinsin mi?`,
                confirmText: 'Sil', confirmColor: 'warn', icon: 'delete_forever' } as ConfirmDialogData,
        width: '420px'
      }).afterClosed().subscribe(c => {
        if (c) this.taskService.delete(event.task.id).subscribe({
          next: r => { if (r.success) { this.notification.showSuccess('Görev silindi.'); this.loadTasks(); } }
        });
      });
    } else if (event.type === 'status' && event.newStatus !== undefined) {
      this.taskService.updateStatus(event.task.id, event.newStatus).subscribe({
        next: r => { if (r.success) { this.notification.showSuccess('Durum güncellendi.'); this.loadTasks(); } }
      });
    }
  }
  onPageChange(e: PageEvent): void { this.currentPage = e.pageIndex; this.pageSize = e.pageSize; this.loadTasks(); }
  clearFilters(): void { this.searchControl.reset(''); this.statusControl.reset(''); this.priorityControl.reset(''); }
  exportExcel(): void {
    this.exportService.exportToExcel(this.tasks.map(t => ({
      title:       t.title,
      description: t.description ?? '',
      status:      t.status?.toString() ?? '',
      priority:    t.priority?.toString() ?? '',
      categoryName: t.categoryId ?? undefined,
      dueDate:     t.dueDate,
      createdAt:   t.createdAt,
    })));
  }
  exportPdf(): void {
    this.exportService.exportToPdf(this.tasks.map(t => ({
      title:       t.title,
      description: t.description ?? '',
      status:      t.status?.toString() ?? '',
      priority:    t.priority?.toString() ?? '',
      categoryName: t.categoryId ?? undefined,
      dueDate:     t.dueDate,
      createdAt:   t.createdAt,
    })));
  }
  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
