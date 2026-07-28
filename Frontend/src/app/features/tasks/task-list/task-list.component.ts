import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { TaskService } from '../../../core/services/task.service';
import { ErrorHandlingService } from '../../../core/services/error-handling.service';
import { TaskCardComponent, TaskCardAction } from '../task-card/task-card.component';
import { TaskItem, TaskStatus, TaskPriority, TaskFilter } from '../../../models/task.model';
import { PagedResponse } from '../../../models/api-response.model';
@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    TaskCardComponent
  ],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.scss'
})
export class TaskListComponent implements OnInit, OnDestroy {
  private readonly taskService = inject(TaskService);
  private readonly notificationService = inject(ErrorHandlingService);
  private readonly destroy$ = new Subject<void>();
  tasks: TaskItem[] = [];
  pagedData: PagedResponse<TaskItem> | null = null;
  isLoading = false;
  readonly searchControl  = new FormControl('');
  readonly statusControl  = new FormControl<TaskStatus | ''>('' );
  readonly priorityControl = new FormControl<TaskPriority | ''>('');
  readonly TaskStatus   = TaskStatus;
  readonly TaskPriority = TaskPriority;
  readonly statusOptions = [
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
  pageSize = 9;
  ngOnInit(): void {
    // Arama kutusunda 400ms bekle; her tuş vuruşunda istek atma (debounce).
    this.searchControl.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
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
      pageSize: this.pageSize,
      searchTerm: this.searchControl.value || undefined,
      status: this.statusControl.value !== '' ? this.statusControl.value! : undefined,
      priority: this.priorityControl.value !== '' ? this.priorityControl.value! : undefined
    };
    this.taskService.getAll(filter).subscribe({
      next: res => {
        if (res.success) {
          this.tasks = res.data.items;
          this.pagedData = res.data;
        }
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }
  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadTasks();
  }
  onTaskAction(event: TaskCardAction): void {
    if (event.type === 'delete') {
      this.deleteTask(event.task.id);
    } else if (event.type === 'status' && event.newStatus !== undefined) {
      this.updateStatus(event.task.id, event.newStatus);
    }
  }
  private deleteTask(id: string): void {
    if (!confirm('Bu görevi silmek istediğinize emin misiniz?')) return;
    this.taskService.delete(id).subscribe({
      next: res => {
        if (res.success) {
          this.notificationService.showSuccess('Görev başarıyla silindi.');
          this.loadTasks();
        }
      }
    });
  }
  private updateStatus(id: string, status: TaskStatus): void {
    this.taskService.updateStatus(id, status).subscribe({
      next: res => {
        if (res.success) {
          this.notificationService.showSuccess('Görev durumu güncellendi.');
          this.loadTasks();
        }
      }
    });
  }
  clearFilters(): void {
    this.searchControl.reset('');
    this.statusControl.reset('');
    this.priorityControl.reset('');
  }
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
