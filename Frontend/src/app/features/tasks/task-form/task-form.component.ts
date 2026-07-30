import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TaskService } from '../../../core/services/task.service';
import { CategoryService } from '../../../core/services/category.service';
import { ErrorHandlingService } from '../../../core/services/error-handling.service';
import { TaskItem, TaskPriority, TaskStatus, CreateTaskRequest, UpdateTaskRequest } from '../../../models/task.model';
import { Category } from '../../../models/category.model';
export interface TaskFormDialogData {
  task?: TaskItem; // Varsa düzenleme modu, yoksa ekleme modu
}
/**
 * Görev ekleme/düzenleme için Material Dialog formu.
 * MAT_DIALOG_DATA ile task bilgisi inject edilir:
 *  - task === undefined → Create modu
 *  - task !== undefined → Edit modu (form alanları doldurulur)
 *
 * Custom Validator: futureDateValidator — geçmiş tarih seçimini engeller.
 */
const futureDateValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  if (!control.value) return null;
  const selected = new Date(control.value);
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return selected < today ? { pastDate: true } : null;
};
@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './task-form.component.html',
  styleUrl: './task-form.component.scss'
})
export class TaskFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly taskService = inject(TaskService);
  private readonly categoryService = inject(CategoryService);
  private readonly notificationService = inject(ErrorHandlingService);
  readonly dialogRef = inject(MatDialogRef<TaskFormComponent>);
  readonly data: TaskFormDialogData = inject(MAT_DIALOG_DATA);
  isLoading = false;
  categories: Category[] = [];
  isEditMode = false;
  readonly TaskPriority = TaskPriority;
  readonly minDate = new Date();
  readonly priorityOptions = [
    { value: TaskPriority.Low,      label: 'Düşük' },
    { value: TaskPriority.Medium,   label: 'Orta' },
    { value: TaskPriority.High,     label: 'Yüksek' },
    { value: TaskPriority.Critical, label: 'Kritik' }
  ];
  taskForm = this.fb.group({
    title:       ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(500)]],
    priority:    [TaskPriority.Medium, Validators.required],
    dueDate:     [null as Date | null, futureDateValidator],
    categoryId:  [null as string | null]
  });
  get title()       { return this.taskForm.get('title'); }
  get description() { return this.taskForm.get('description'); }
  get dueDate()     { return this.taskForm.get('dueDate'); }
  ngOnInit(): void {
    this.isEditMode = !!this.data.task;
    this.loadCategories();
    if (this.data.task) {
      this.taskForm.patchValue({
        title:       this.data.task.title,
        description: this.data.task.description,
        priority:    this.data.task.priority,
        dueDate:     this.data.task.dueDate ? new Date(this.data.task.dueDate) : null,
        categoryId:  this.data.task.categoryId
      });
    }
  }
  private loadCategories(): void {
    this.categoryService.getAll().subscribe({
      next: res => { if (res.success) this.categories = res.data; }
    });
  }
  onSubmit(): void {
    if (this.taskForm.invalid) { this.taskForm.markAllAsTouched(); return; }
    this.isLoading = true;
    const v = this.taskForm.value;
    if (this.isEditMode && this.data.task) {
      const req: UpdateTaskRequest = {
        title:       v.title!,
        description: v.description ?? '',
        priority:    v.priority!,
        dueDate:     v.dueDate ? (v.dueDate as Date).toISOString() : null,
        categoryId:  v.categoryId ?? null
      };
      this.taskService.update(this.data.task.id, req).subscribe({
        next: res => {
          if (res.success) {
            this.notificationService.showSuccess('Görev başarıyla güncellendi.');
            this.dialogRef.close(true);
          }
          this.isLoading = false;
        },
        error: () => { this.isLoading = false; }
      });
    } else {
      const req: CreateTaskRequest = {
        title:       v.title!,
        description: v.description ?? '',
        priority:    v.priority!,
        dueDate:     v.dueDate ? (v.dueDate as Date).toISOString() : null,
        categoryId:  v.categoryId ?? null
      };
      this.taskService.create(req).subscribe({
        next: res => {
          if (res.success) {
            this.notificationService.showSuccess('Görev başarıyla oluşturuldu.');
            this.dialogRef.close(true);
          }
          this.isLoading = false;
        },
        error: () => { this.isLoading = false; }
      });
    }
  }
}
