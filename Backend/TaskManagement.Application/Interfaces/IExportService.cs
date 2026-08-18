using TaskManagement.Application.DTOs;
namespace TaskManagement.Application.Interfaces;
/// <summary>
/// Görev verilerini PDF ve Excel formatında dışa aktarma sözleşmesi.
/// </summary>
public interface IExportService
{
    byte[] ExportTasksToPdf(IEnumerable<TaskItemDto> tasks, string title = "Görev Listesi");
    byte[] ExportTasksToExcel(IEnumerable<TaskItemDto> tasks);
}
