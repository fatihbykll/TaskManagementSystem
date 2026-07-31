import { Component, Input, Output, EventEmitter, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import {
  CdkDragDrop, DragDropModule,
  moveItemInArray, transferArrayItem
} from '@angular/cdk/drag-drop';
import { TaskItem, TaskStatus, TaskPriority } from '../../../models/task.model';

interface KanbanColumn {
  status: TaskStatus;
  label: string;
  gradient: string;
  icon: string;
  tasks: TaskItem[];
}

@Component({
  selector: 'app-task-board',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatTooltipModule, DragDropModule],
  templateUrl: './task-board.component.html',
  styleUrl: './task-board.component.scss'
})
export class TaskBoardComponent implements OnChanges {
  @Input() tasks: TaskItem[] = [];
  @Output() statusChanged = new EventEmitter<{ taskId: string; newStatus: TaskStatus }>();

  readonly pColor: Record<number, string> = {
    0: '#2e7d32', 1: '#e65100', 2: '#c62828', 3: '#6a1b9a'
  };
  readonly pLabel: Record<number, string> = {
    0: 'Düşük', 1: 'Orta', 2: 'Yüksek', 3: 'Kritik'
  };

  columns: KanbanColumn[] = [
    { status: TaskStatus.Pending,    label: 'Beklemede',    gradient: 'linear-gradient(135deg,#757575,#9e9e9e)', icon: 'schedule',     tasks: [] },
    { status: TaskStatus.InProgress, label: 'Devam Ediyor', gradient: 'linear-gradient(135deg,#1565c0,#1976d2)', icon: 'autorenew',    tasks: [] },
    { status: TaskStatus.Completed,  label: 'Tamamlandı',   gradient: 'linear-gradient(135deg,#2e7d32,#388e3c)', icon: 'check_circle', tasks: [] },
    { status: TaskStatus.Cancelled,  label: 'İptal',        gradient: 'linear-gradient(135deg,#b71c1c,#c62828)', icon: 'cancel',       tasks: [] }
  ];

  get dropListIds(): string[] {
    return this.columns.map(c => 'col-' + c.status);
  }

  ngOnChanges(): void {
    this.columns.forEach(col => col.tasks = []);
    this.tasks.forEach(task => {
      const col = this.columns.find(c => c.status === task.status);
      if (col) col.tasks.push(task);
    });
  }

  onDrop(event: CdkDragDrop<TaskItem[]>, target: KanbanColumn): void {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
      const moved = event.container.data[event.currentIndex];
      this.statusChanged.emit({ taskId: moved.id, newStatus: target.status });
    }
  }

  dueDateWarn(task: TaskItem): { text: string; color: string } | null {
    if (!task.dueDate) return null;
    const d = Math.ceil((new Date(task.dueDate).getTime() - Date.now()) / 86400000);
    if (d < 0)   return { text: Math.abs(d) + 'g gecikti', color: '#c62828' };
    if (d === 0) return { text: 'Bugün!',                   color: '#e65100' };
    if (d <= 3)  return { text: d + 'g kaldı',              color: '#f57c00' };
    return null;
  }
}
