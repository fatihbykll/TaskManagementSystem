import { Component, inject, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { AdminService, AdminUser, AdminStats, DailyReport } from './admin.service';
@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [
    CommonModule, DatePipe,
    MatCardModule, MatIconModule, MatTableModule, MatChipsModule,
    MatButtonModule, MatProgressSpinnerModule, MatTooltipModule, MatDividerModule
  ],
  template: `
    <div class="admin-panel">
      <div class="admin-header">
        <h1><mat-icon>admin_panel_settings</mat-icon> Admin Paneli</h1>
        <p class="subtitle">Sistem genelinde istatistikler ve kullanıcı yönetimi</p>
      </div>
      @if (isLoading) {
        <div class="loading-center"><mat-spinner diameter="48"></mat-spinner></div>
      } @else {
        <!-- İstatistik Kartları -->
        @if (stats) {
          <div class="stat-grid">
            <mat-card class="stat-card" style="background:linear-gradient(135deg,#667eea,#764ba2)">
              <mat-card-content><div class="stat-content">
                <div><span class="stat-label">Toplam Kullanıcı</span><span class="stat-value">{{ stats.totalUsers }}</span></div>
                <mat-icon class="stat-icon">people</mat-icon>
              </div></mat-card-content>
            </mat-card>
            <mat-card class="stat-card" style="background:linear-gradient(135deg,#4facfe,#00f2fe)">
              <mat-card-content><div class="stat-content">
                <div><span class="stat-label">Aktif Kullanıcı</span><span class="stat-value">{{ stats.activeUsers }}</span></div>
                <mat-icon class="stat-icon">person_check</mat-icon>
              </div></mat-card-content>
            </mat-card>
            <mat-card class="stat-card" style="background:linear-gradient(135deg,#43e97b,#38f9d7)">
              <mat-card-content><div class="stat-content">
                <div><span class="stat-label">Toplam Görev</span><span class="stat-value">{{ stats.totalTasks }}</span></div>
                <mat-icon class="stat-icon">task_alt</mat-icon>
              </div></mat-card-content>
            </mat-card>
            <mat-card class="stat-card" style="background:linear-gradient(135deg,#fa709a,#fee140)">
              <mat-card-content><div class="stat-content">
                <div><span class="stat-label">Bekleyen Görev</span><span class="stat-value">{{ stats.pendingTasks }}</span></div>
                <mat-icon class="stat-icon">pending_actions</mat-icon>
              </div></mat-card-content>
            </mat-card>
          </div>
        }
        <!-- Günlük Rapor -->
        @if (report) {
          <mat-card class="report-card">
            <mat-card-header>
              <mat-icon mat-card-avatar>bar_chart</mat-icon>
              <mat-card-title>Günlük Rapor</mat-card-title>
              <mat-card-subtitle>{{ report.date | date:'dd MMMM yyyy' : '' : 'tr' }}</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <div class="report-grid">
                <div class="report-item"><span class="report-value">{{ report.newTasks }}</span><span class="report-label">Yeni Görev</span></div>
                <div class="report-item"><span class="report-value">{{ report.completedTasks }}</span><span class="report-label">Tamamlanan</span></div>
                <div class="report-item"><span class="report-value">{{ report.activeUsers }}</span><span class="report-label">Aktif Kullanıcı</span></div>
                <div class="report-item"><span class="report-value" style="color:#e53935">{{ report.overdueTaskCount }}</span><span class="report-label">Geciken Görev</span></div>
              </div>
            </mat-card-content>
          </mat-card>
        }
        <!-- Kullanıcı Listesi -->
        <mat-card class="users-card">
          <mat-card-header>
            <mat-icon mat-card-avatar>group</mat-icon>
            <mat-card-title>Kullanıcılar</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <table mat-table [dataSource]="users" class="users-table">
              <ng-container matColumnDef="name">
                <th mat-header-cell *matHeaderCellDef>Ad Soyad</th>
                <td mat-cell *matCellDef="let u">{{ u.firstName }} {{ u.lastName }}</td>
              </ng-container>
              <ng-container matColumnDef="email">
                <th mat-header-cell *matHeaderCellDef>E-posta</th>
                <td mat-cell *matCellDef="let u">{{ u.email }}</td>
              </ng-container>
              <ng-container matColumnDef="role">
                <th mat-header-cell *matHeaderCellDef>Rol</th>
                <td mat-cell *matCellDef="let u">
                  <mat-chip [style.background]="u.role === 'Admin' ? '#7c4dff' : '#1976d2'" style="color:white">{{ u.role }}</mat-chip>
                </td>
              </ng-container>
              <ng-container matColumnDef="status">
                <th mat-header-cell *matHeaderCellDef>Durum</th>
                <td mat-cell *matCellDef="let u">
                  <mat-chip [style.background]="u.isActive ? '#2e7d32' : '#c62828'" style="color:white">
                    {{ u.isActive ? 'Aktif' : 'Pasif' }}
                  </mat-chip>
                </td>
              </ng-container>
              <ng-container matColumnDef="lastLogin">
                <th mat-header-cell *matHeaderCellDef>Son Giriş</th>
                <td mat-cell *matCellDef="let u">{{ u.lastLoginAt ? (u.lastLoginAt | date:'dd.MM.yyyy HH:mm') : '-' }}</td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
            </table>
          </mat-card-content>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .admin-panel { padding: 24px; max-width: 1200px; margin: 0 auto; }
    .admin-header h1 { display:flex; align-items:center; gap:8px; margin:0; font-size:28px; }
    .subtitle { color: var(--text-secondary, #666); margin: 4px 0 24px; }
    .loading-center { display:flex; justify-content:center; padding:60px; }
    .stat-grid { display:grid; grid-template-columns:repeat(auto-fit,minmax(200px,1fr)); gap:16px; margin-bottom:24px; }
    .stat-card { border-radius:16px; color:#fff; }
    .stat-content { display:flex; justify-content:space-between; align-items:center; }
    .stat-label { display:block; font-size:13px; opacity:.85; }
    .stat-value { display:block; font-size:32px; font-weight:700; }
    .stat-icon { font-size:40px; width:40px; height:40px; opacity:.7; }
    .report-card, .users-card { margin-bottom:24px; border-radius:16px; }
    .report-grid { display:grid; grid-template-columns:repeat(4,1fr); gap:16px; padding:8px 0; }
    .report-item { text-align:center; }
    .report-value { display:block; font-size:28px; font-weight:700; }
    .report-label { display:block; font-size:12px; color:#888; margin-top:4px; }
    .users-table { width:100%; }
  `]
})
export class AdminComponent implements OnInit {
  private readonly adminService = inject(AdminService);
  stats: AdminStats | null = null;
  users: AdminUser[] = [];
  report: DailyReport | null = null;
  isLoading = true;
  displayedColumns = ['name', 'email', 'role', 'status', 'lastLogin'];
  ngOnInit(): void {
    this.loadData();
  }
  private loadData(): void {
    this.adminService.getStatistics().subscribe({
      next: r => { if (r.success) this.stats = r.data; }
    });
    this.adminService.getDailyReport().subscribe({
      next: r => { if (r.success) this.report = r.data; }
    });
    this.adminService.getUsers().subscribe({
      next: r => {
        if (r.success) this.users = r.data;
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }
}
