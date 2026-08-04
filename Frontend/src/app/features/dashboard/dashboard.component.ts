import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';
import { TaskService } from '../../core/services/task.service';
import { TaskStatistics, TaskItem } from '../../models/task.model';
interface StatCard {
  label: string;
  value: number;
  icon: string;
  color: string;
  gradient: string;
}
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    RouterLink
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly taskService = inject(TaskService);
  stats: TaskStatistics | null = null;
  overdueTasks: TaskItem[] = [];
  isLoading = true;
  statCards: StatCard[] = [];
  ngOnInit(): void {
    this.loadDashboardData();
  }
  private loadDashboardData(): void {
    this.taskService.getStatistics().subscribe({
      next: (res) => {
        if (res.success) {
          this.stats = res.data;
          this.buildStatCards(res.data);
        }
      },
      error: () => { this.isLoading = false; }
    });
    this.taskService.getOverdue().subscribe({
      next: (res) => {
        if (res.success) this.overdueTasks = res.data;
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }
  private buildStatCards(stats: TaskStatistics): StatCard[] {
    this.statCards = [
      { label: 'Toplam Görev', value: stats.totalTasks, icon: 'assignment', color: '#fff', gradient: 'linear-gradient(135deg,#667eea,#764ba2)' },
      { label: 'Beklemede', value: stats.pendingCount, icon: 'pending_actions', color: '#fff', gradient: 'linear-gradient(135deg,#f093fb,#f5576c)' },
      { label: 'Devam Eden', value: stats.inProgressCount, icon: 'autorenew', color: '#fff', gradient: 'linear-gradient(135deg,#4facfe,#00f2fe)' },
      { label: 'Tamamlanan', value: stats.completedCount, icon: 'check_circle', color: '#fff', gradient: 'linear-gradient(135deg,#43e97b,#38f9d7)' },
      { label: 'Vadesi Geçmiş', value: stats.overdueCount, icon: 'warning', color: '#fff', gradient: 'linear-gradient(135deg,#fa709a,#fee140)' },
      { label: 'Bu Ay Tamamlanan', value: stats.completedThisMonth, icon: 'calendar_month', color: '#fff', gradient: 'linear-gradient(135deg,#a18cd1,#fbc2eb)' },
    ];
    return this.statCards;
  }
}
