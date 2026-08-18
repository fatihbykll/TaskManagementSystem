using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Enums;
namespace TaskManagement.Infrastructure.Services;
public class ExportService : IExportService
{
    static ExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }
    public byte[] ExportTasksToPdf(IEnumerable<TaskItemDto> tasks, string title = "Görev Listesi")
    {
        var taskList = tasks.ToList();
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Header().Column(col =>
                {
                    col.Item().Text(title).Bold().FontSize(18).FontColor(Color.FromHex("#1976D2"));
                    col.Item().Text($"Oluşturulma: {DateTime.Now:dd.MM.yyyy HH:mm}")
                        .FontSize(9).FontColor(Color.FromHex("#757575"));
                    col.Item().Height(8);
                });
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(2);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                    });
                    table.Header(header =>
                    {
                        var headerStyle = TextStyle.Default.Bold().FontColor(Colors.White);
                        var cellColor   = Colors.Blue.Medium;
                        header.Cell().Background(cellColor).Padding(6).Text("Başlık").Style(headerStyle);
                        header.Cell().Background(cellColor).Padding(6).Text("Kategori").Style(headerStyle);
                        header.Cell().Background(cellColor).Padding(6).Text("Durum").Style(headerStyle);
                        header.Cell().Background(cellColor).Padding(6).Text("Öncelik").Style(headerStyle);
                        header.Cell().Background(cellColor).Padding(6).Text("Son Tarih").Style(headerStyle);
                    });
                    foreach (var (task, i) in taskList.Select((t, i) => (t, i)))
                    {
                        var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                        table.Cell().Background(bg).Padding(5).Text(task.Title ?? "");
                        table.Cell().Background(bg).Padding(5).Text(task.CategoryName ?? "-");
                        table.Cell().Background(bg).Padding(5).Text(MapStatus((TaskItemStatus)task.Status));
                        table.Cell().Background(bg).Padding(5).Text(MapPriority((Priority)task.Priority));
                        table.Cell().Background(bg).Padding(5).Text(task.DueDate.HasValue ? task.DueDate.Value.ToString("dd.MM.yyyy") : "-");
                    }
                });
                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Sayfa ").FontSize(9);
                    txt.CurrentPageNumber().FontSize(9);
                    txt.Span(" / ").FontSize(9);
                    txt.TotalPages().FontSize(9);
                });
            });
        });
        return doc.GeneratePdf();
    }
    public byte[] ExportTasksToExcel(IEnumerable<TaskItemDto> tasks)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Görevler");
        var headers = new[] { "Başlık", "Açıklama", "Kategori", "Durum", "Öncelik", "Son Tarih", "Oluşturulma" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1976D2");
            cell.Style.Font.FontColor = XLColor.White;
        }
        int row = 2;
        foreach (var task in tasks)
        {
            ws.Cell(row, 1).Value = task.Title ?? "";
            ws.Cell(row, 2).Value = task.Description ?? "";
            ws.Cell(row, 3).Value = task.CategoryName ?? "";
            ws.Cell(row, 4).Value = MapStatus((TaskItemStatus)task.Status);
            ws.Cell(row, 5).Value = MapPriority((Priority)task.Priority);
            ws.Cell(row, 6).Value = task.DueDate.HasValue ? task.DueDate.Value.ToString("dd.MM.yyyy") : "";
            ws.Cell(row, 7).Value = task.CreatedAt.ToString("dd.MM.yyyy");
            if (row % 2 == 0)
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");
            row++;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
    private static string MapStatus(TaskItemStatus status) => status switch
    {
        TaskItemStatus.Pending    => "Beklemede",
        TaskItemStatus.InProgress => "Devam Ediyor",
        TaskItemStatus.Completed  => "Tamamlandı",
        TaskItemStatus.Cancelled  => "İptal",
        _                         => "Bilinmiyor"
    };
    private static string MapPriority(Priority priority) => priority switch
    {
        Priority.Low      => "Düşük",
        Priority.Normal   => "Normal",
        Priority.High     => "Yüksek",
        Priority.Urgent   => "Acil",
        Priority.Critical => "Kritik",
        _                 => "Bilinmiyor"
    };
}
