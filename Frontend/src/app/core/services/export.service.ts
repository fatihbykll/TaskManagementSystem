import { Injectable } from '@angular/core';
import * as XLSX from 'xlsx';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
export interface ExportTask {
  title:       string;
  description: string;
  status:      string;
  priority:    string;
  categoryName?: string;
  dueDate?:    string | null;
  createdAt?:  string | null;
}
@Injectable({ providedIn: 'root' })
export class ExportService {
  /** Görev listesini Excel (.xlsx) olarak indirir */
  exportToExcel(tasks: ExportTask[], fileName = 'gorevler'): void {
    const rows = tasks.map((t, i) => ({
      '#':          i + 1,
      'Başlık':     t.title,
      'Açıklama':   t.description ?? '',
      'Durum':      this.translateStatus(t.status),
      'Öncelik':    this.translatePriority(t.priority),
      'Kategori':   t.categoryName ?? '-',
      'Bitiş Tarihi': t.dueDate ? new Date(t.dueDate).toLocaleDateString('tr-TR') : '-',
      'Oluşturulma': t.createdAt ? new Date(t.createdAt).toLocaleDateString('tr-TR') : '-',
    }));
    const worksheet = XLSX.utils.json_to_sheet(rows);
    const workbook  = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Görevler');
    // Kolon genişlikleri
    worksheet['!cols'] = [
      { wch: 4 }, { wch: 30 }, { wch: 40 },
      { wch: 15 }, { wch: 12 }, { wch: 18 },
      { wch: 15 }, { wch: 15 },
    ];
    XLSX.writeFile(workbook, `${fileName}_${this.dateStamp()}.xlsx`);
  }
  /** Görev listesini PDF olarak indirir */
  exportToPdf(tasks: ExportTask[], fileName = 'gorevler'): void {
    const doc = new jsPDF({ orientation: 'landscape' });
    // Başlık
    doc.setFontSize(16);
    doc.setTextColor(102, 126, 234);
    doc.text('Görev Listesi', 14, 16);
    doc.setFontSize(9);
    doc.setTextColor(130, 130, 130);
    doc.text(`Oluşturulma: ${new Date().toLocaleDateString('tr-TR')}  |  Toplam: ${tasks.length} görev`, 14, 22);
    autoTable(doc, {
      startY: 28,
      head: [['#', 'Başlık', 'Durum', 'Öncelik', 'Kategori', 'Bitiş Tarihi']],
      body: tasks.map((t, i) => [
        i + 1,
        t.title,
        this.translateStatus(t.status),
        this.translatePriority(t.priority),
        t.categoryName ?? '-',
        t.dueDate ? new Date(t.dueDate).toLocaleDateString('tr-TR') : '-',
      ]),
      headStyles: { fillColor: [102, 126, 234], textColor: 255, fontStyle: 'bold' },
      alternateRowStyles: { fillColor: [245, 245, 255] },
      styles: { fontSize: 9, cellPadding: 3 },
    });
    doc.save(`${fileName}_${this.dateStamp()}.pdf`);
  }
  private translateStatus(s: string): string {
    const map: Record<string, string> = {
      'Pending': 'Bekliyor', 'InProgress': 'Devam Ediyor',
      'Completed': 'Tamamlandı', 'Cancelled': 'İptal'
    };
    return map[s] ?? s;
  }
  private translatePriority(p: string): string {
    const map: Record<string, string> = {
      'Low': 'Düşük', 'Medium': 'Orta', 'High': 'Yüksek', 'Critical': 'Kritik'
    };
    return map[p] ?? p;
  }
  private dateStamp(): string {
    return new Date().toISOString().slice(0, 10);
  }
}
