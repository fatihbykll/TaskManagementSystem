import { Component, inject, OnInit, OnDestroy, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';
import { Chart, ArcElement, DoughnutController, BarController, CategoryScale, LinearScale, BarElement, Tooltip, Legend, Title } from 'chart.js';
import { TaskService } from '../../core/services/task.service';
import { TaskStatistics, TaskItem } from '../../models/task.model';
Chart.register(ArcElement, DoughnutController, BarController, CategoryScale, LinearScale, BarElement, Tooltip, Legend, Title);
interface StatCard {
  label: string; value: number; icon: string; color: string; gradient: string;
}
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule, MatButtonModule, MatProgressSpinnerModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit, OnDestroy, AfterViewInit {
  private readonly taskService = inject(TaskService);
  @ViewChild('statusChart') statusChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('priorityChart') priorityChartRef!: ElementRef<HTMLCanvasElement>;
  stats: TaskStatistics | null = null;
  overdueTasks: TaskItem[] = [];
  isLoading = true;
  statCards: StatCard[] = [];
  chartsReady = false;
  private statusChart: Chart | null = null;
  private priorityChart: Chart | null = null;
  ngOnInit(): void { this.loadDashboardData(); }
  ngAfterViewInit(): void { this.chartsReady = true; }
  ngOnDestroy(): void {
    this.statusChart?.destroy();
    this.priorityChart?.destroy();
  }
  private loadDashboardData(): void {
    this.taskService.getStatistics().subscribe({
      next: (res) => {
        if (res.success) {
          this.stats = res.data;
          this.buildStatCards(res.data);
          // View init'i bekle
          setTimeout(() => this.buildCharts(res.data), 100);
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
  private buildStatCards(stats: TaskStatistics): void {
    this.statCards = [
      { label: 'Toplam Görev',       value: stats.totalTasks,          icon: 'assignment',      color: '#fff', gradient: 'linear-gradient(135deg,#667eea,#764ba2)' },
      { label: 'Beklemede',          value: stats.pendingCount,         icon: 'pending_actions', color: '#fff', gradient: 'linear-gradient(135deg,#f093fb,#f5576c)' },
      { label: 'Devam Eden',         value: stats.inProgressCount,      icon: 'autorenew',       color: '#fff', gradient: 'linear-gradient(135deg,#4facfe,#00f2fe)' },
      { label: 'Tamamlanan',         value: stats.completedCount,       icon: 'check_circle',    color: '#fff', gradient: 'linear-gradient(135deg,#43e97b,#38f9d7)' },
      { label: 'Vadesi Geçmiş',      value: stats.overdueCount,         icon: 'warning',         color: '#fff', gradient: 'linear-gradient(135deg,#fa709a,#fee140)' },
      { label: 'Bu Ay Tamamlanan',   value: stats.completedThisMonth,   icon: 'calendar_month',  color: '#fff', gradient: 'linear-gradient(135deg,#a18cd1,#fbc2eb)' },
    ];
  }
  private buildCharts(stats: TaskStatistics): void {
    // Donut: Durum dağılımı
    const statusCtx = this.statusChartRef?.nativeElement?.getContext('2d');
    if (statusCtx) {
      this.statusChart?.destroy();
      this.statusChart = new Chart(statusCtx, {
        type: 'doughnut',
        data: {
          labels: ['Beklemede', 'Devam Eden', 'Tamamlandı', 'İptal'],
          datasets: [{
            data: [stats.pendingCount, stats.inProgressCount, stats.completedCount, stats.cancelledCount],
            backgroundColor: ['#9e9e9e', '#1976d2', '#388e3c', '#c62828'],
            borderWidth: 3,
            borderColor: 'transparent',
            hoverBorderColor: '#fff'
          }]
        },
        options: {
          responsive: true, maintainAspectRatio: false,
          plugins: {
            legend: { position: 'bottom', labels: { padding: 16, font: { size: 13 } } },
            title: { display: true, text: 'Görev Durum Dağılımı', font: { size: 15, weight: 'bold' } }
          },
          cutout: '65%'
        }
      });
    }
    // Bar: Öncelik dağılımı
    const priorityCtx = this.priorityChartRef?.nativeElement?.getContext('2d');
    if (priorityCtx) {
      this.priorityChart?.destroy();
      this.priorityChart = new Chart(priorityCtx, {
        type: 'bar',
        data: {
          labels: ['Tamamlandı', 'Beklemede', 'Devam Eden', 'İptal'],
          datasets: [{
            label: 'Görev Sayısı',
            data: [
              stats.completedCount,
              stats.pendingCount,
              stats.inProgressCount,
              stats.cancelledCount
            ],
            backgroundColor: ['#2e7d32aa', '#e65100aa', '#c62828aa', '#6a1b9aaa'],
            borderRadius: 8,
            borderSkipped: false
          }]
        },
        options: {
          responsive: true, maintainAspectRatio: false,
          plugins: {
            legend: { display: false },
            title: { display: true, text: 'Görev Dağılımı (Bar)', font: { size: 15, weight: 'bold' } }
          },
          scales: {
            y: { beginAtZero: true, ticks: { stepSize: 1 }, grid: { color: '#e0e0e055' } },
            x: { grid: { display: false } }
          }
        }
      });
    }
  }
}
