import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
@Injectable({ providedIn: 'root' })
export class ExportService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/Export`;

  // task-list component uyumluluğu için alias metodlar
  exportToPdf(_tasks?: any[]): void  { this.downloadPdf(); }
  exportToExcel(_tasks?: any[]): void { this.downloadExcel(); }
  downloadPdf(): void {
    this.http.get(`${this.base}/pdf`, { responseType: 'blob' }).subscribe(blob => {
      this.triggerDownload(blob, `gorevler_${this.today()}.pdf`, 'application/pdf');
    });
  }
  downloadExcel(): void {
    this.http.get(`${this.base}/excel`, { responseType: 'blob' }).subscribe(blob => {
      this.triggerDownload(blob, `gorevler_${this.today()}.xlsx`,
        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet');
    });
  }
  private triggerDownload(blob: Blob, filename: string, type: string): void {
    const url = URL.createObjectURL(new Blob([blob], { type }));
    const a   = document.createElement('a');
    a.href = url; a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
  }
  private today(): string {
    return new Date().toISOString().slice(0, 10).replace(/-/g, '');
  }
}
