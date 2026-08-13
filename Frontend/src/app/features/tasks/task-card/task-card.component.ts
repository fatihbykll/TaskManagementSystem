import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { TaskItem, TaskStatus, TaskPriority, RecurringFrequency } from '../../../models/task.model';
export interface TaskCardAction {
  type: 'edit' | 'delete' | 'status';
  task: TaskItem;
  newStatus?: TaskStatus;
}
const PRIORITY_META: Record<TaskPriority, { label: string; color: string; bg: string }> = {
  [TaskPriority.Low]:      { label: 'Düşük',  color: '#2e7d32', bg: 'rgba(46,125,50,0.1)' },
  [TaskPriority.Medium]:   { label: 'Orta',   color: '#e65100', bg: 'rgba(230,81,0,0.1)' },
  [TaskPriority.High]:     { label: 'Yüksek', color: '#c62828', bg: 'rgba(198,40,40,0.1)' },
  [TaskPriority.Critical]: { label: 'Kritik', color: '#6a1b9a', bg: 'rgba(106,27,154,0.1)' }
};
const STATUS_META: Record<TaskStatus, { label: string; color: string; icon: string }> = {
  [TaskStatus.Pending]:    { label: 'Beklemede',    color: '#757575', icon: 'schedule' },
  [TaskStatus.InProgress]: { label: 'Devam Ediyor', color: '#1565c0', icon: 'autorenew' },
  [TaskStatus.Completed]:  { label: 'Tamamlandı',   color: '#2e7d32', icon: 'check_circle' },
  [TaskStatus.Cancelled]:  { label: 'İptal',        color: '#b71c1c', icon: 'cancel' }
};
const RECURRING_LABELS: Record<RecurringFrequency, string> = {
  [RecurringFrequency.None]:    '',
  [RecurringFrequency.Daily]:   'Her Gün',
  [RecurringFrequency.Weekly]:  'Her Hafta',
  [RecurringFrequency.Monthly]: 'Her Ay'
};
@Component({
  selector: 'app-task-card',
  standalone: true,
  imports: [
    CommonModule, MatCardModule, MatChipsModule, MatIconModule,
    MatButtonModule, MatTooltipModule, MatMenuModule, MatDividerModule
  ],
  templateUrl: './task-card.component.html',
  styleUrl: './task-card.component.scss'
})
export class TaskCardComponent {
  @Input({ required: true }) task!: TaskItem;
  @Output() action = new EventEmitter<TaskCardAction>();
  readonly TaskStatus = TaskStatus;
  readonly RecurringFrequency = RecurringFrequency;
  get priority() { return PRIORITY_META[this.task.priority]; }
  get status()   { return STATUS_META[this.task.status]; }
  /** Tekrarlama etiketi: None ise null döner */
  get recurringLabel(): string | null {
    const f = this.task.recurringFrequency ?? RecurringFrequency.None;
    return f !== RecurringFrequency.None ? RECURRING_LABELS[f] : null;
  }
  /** ParentTaskId doluysa bu görev bir kopyadan oluşmuştur */
  get isClone(): boolean {
    return !!this.task.parentTaskId;
  }
  get dueDateInfo(): { label: string; color: string; icon: string; warning: boolean } | null {
    if (!this.task.dueDate) return null;
    const now = new Date(); now.setHours(0, 0, 0, 0);
    const due = new Date(this.task.dueDate); due.setHours(0, 0, 0, 0);
    const diffDays = Math.ceil((due.getTime() - now.getTime()) / 86400000);
    const done = this.task.status === TaskStatus.Completed || this.task.status === TaskStatus.Cancelled;
    if (!done && diffDays < 0)   return { label: `${Math.abs(diffDays)} gün gecikmiş`, color: '#c62828', icon: 'warning',        warning: true };
    if (!done && diffDays === 0) return { label: 'Bugün son gün!',                     color: '#e65100', icon: 'alarm',          warning: true };
    if (!done && diffDays <= 3)  return { label: `${diffDays} gün kaldı`,              color: '#f57c00', icon: 'alarm_on',       warning: true };
    return { label: due.toLocaleDateString('tr-TR'), color: '#757575', icon: 'calendar_today', warning: false };
  }
  get progressValue(): number {
    return { [TaskStatus.Pending]: 0, [TaskStatus.InProgress]: 50,
             [TaskStatus.Completed]: 100, [TaskStatus.Cancelled]: 100 }[this.task.status];
  }
  onEdit():   void { this.action.emit({ type: 'edit',   task: this.task }); }
  onDelete(): void { this.action.emit({ type: 'delete', task: this.task }); }
  onStatusChange(s: TaskStatus): void { this.action.emit({ type: 'status', task: this.task, newStatus: s }); }
}
