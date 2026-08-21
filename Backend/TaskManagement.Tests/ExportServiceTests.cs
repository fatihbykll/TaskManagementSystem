using FluentAssertions;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Services;
namespace TaskManagement.Tests;
/// <summary>
/// ExportService — PDF ve Excel çıktılarının doğruluğunu doğrular.
/// Entegrasyon gerektirmez; saf unit test.
/// </summary>
public class ExportServiceTests
{
    private readonly ExportService _sut = new();
    private static List<TaskItemDto> SampleTasks() =>
    [
        new()
        {
            Id          = Guid.NewGuid(),
            Title       = "Test Görevi 1",
            Description = "Açıklama 1",
            Status      = TaskItemStatus.Pending,
            Priority    = Priority.Normal,
            DueDate     = DateTime.UtcNow.AddDays(3),
            CreatedAt   = DateTime.UtcNow,
            CategoryName = "İş"
        },
        new()
        {
            Id          = Guid.NewGuid(),
            Title       = "Test Görevi 2",
            Description = "Açıklama 2",
            Status      = TaskItemStatus.Completed,
            Priority    = Priority.High,
            DueDate     = null,
            CreatedAt   = DateTime.UtcNow,
            CategoryName = null
        }
    ];
    [Fact(DisplayName = "ExportTasksToPdf: Boş olmayan byte array döndürmeli")]
    public void ExportTasksToPdf_ReturnsNonEmptyByteArray()
    {
        var result = _sut.ExportTasksToPdf(SampleTasks(), "Test Raporu");
        result.Should().NotBeNullOrEmpty("PDF içeriği boş olmamalıdır.");
        result.Length.Should().BeGreaterThan(100, "PDF en az 100 byte olmalıdır.");
    }
    [Fact(DisplayName = "ExportTasksToPdf: PDF başlık magic bytes içermeli (%PDF-)")]
    public void ExportTasksToPdf_HasPdfMagicBytes()
    {
        var result = _sut.ExportTasksToPdf(SampleTasks());
        var header = System.Text.Encoding.ASCII.GetString(result[..5]);
        header.Should().Be("%PDF-", "çıktı geçerli bir PDF dosyası olmalıdır.");
    }
    [Fact(DisplayName = "ExportTasksToPdf: Boş liste ile hata fırlatmamalı")]
    public void ExportTasksToPdf_EmptyList_DoesNotThrow()
    {
        var act = () => _sut.ExportTasksToPdf([]);
        act.Should().NotThrow("Boş liste ile çalışıldığında istisna fırlatılmamalıdır.");
    }
    [Fact(DisplayName = "ExportTasksToExcel: Boş olmayan byte array döndürmeli")]
    public void ExportTasksToExcel_ReturnsNonEmptyByteArray()
    {
        var result = _sut.ExportTasksToExcel(SampleTasks());
        result.Should().NotBeNullOrEmpty("Excel içeriği boş olmamalıdır.");
        result.Length.Should().BeGreaterThan(100);
    }
    [Fact(DisplayName = "ExportTasksToExcel: Geçerli XLSX (ZIP) magic bytes içermeli")]
    public void ExportTasksToExcel_HasXlsxMagicBytes()
    {
        var result = _sut.ExportTasksToExcel(SampleTasks());
        // XLSX = ZIP format → PK magic
        result[0].Should().Be(0x50, "XLSX dosyası PK zip başlığıyla başlamalıdır.");
        result[1].Should().Be(0x4B);
    }
    [Fact(DisplayName = "ExportTasksToExcel: Boş liste ile hata fırlatmamalı")]
    public void ExportTasksToExcel_EmptyList_DoesNotThrow()
    {
        var act = () => _sut.ExportTasksToExcel([]);
        act.Should().NotThrow();
    }
}
