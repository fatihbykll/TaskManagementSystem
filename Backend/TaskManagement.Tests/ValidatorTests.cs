using FluentAssertions;
using FluentValidation.TestHelper;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Validators;
namespace TaskManagement.Tests;
/// <summary>
/// CreateTaskDtoValidator kurallarını doğrulayan unit testler.
/// </summary>
public class ValidatorTests
{
    private readonly CreateTaskDtoValidator _validator = new();
    [Fact(DisplayName = "Validator: Geçerli DTO doğrulama hatası üretmemeli")]
    public void ValidDto_ShouldPass()
    {
        var dto = new CreateTaskDto
        {
            Title       = "Geçerli Görev",
            Description = "Kısa açıklama",
            DueDate     = DateTime.UtcNow.AddDays(5)
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }
    [Fact(DisplayName = "Validator: Boş Title hata üretmeli")]
    public void EmptyTitle_ShouldFail()
    {
        var dto = new CreateTaskDto { Title = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("Görev başlığı boş olamaz.");
    }
    [Fact(DisplayName = "Validator: 100 karakterden uzun Title hata üretmeli")]
    public void TooLongTitle_ShouldFail()
    {
        var dto = new CreateTaskDto { Title = new string('A', 101) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("Görev başlığı 100 karakteri geçemez.");
    }
    [Fact(DisplayName = "Validator: 500 karakterden uzun Description hata üretmeli")]
    public void TooLongDescription_ShouldFail()
    {
        var dto = new CreateTaskDto { Title = "Geçerli", Description = new string('X', 501) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Description)
              .WithErrorMessage("Açıklama 500 karakteri geçemez.");
    }
    [Fact(DisplayName = "Validator: Geçmiş DueDate hata üretmeli")]
    public void PastDueDate_ShouldFail()
    {
        var dto = new CreateTaskDto { Title = "Geçerli", DueDate = DateTime.UtcNow.AddDays(-5) };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.DueDate)
              .WithErrorMessage("Son teslim tarihi geçmiş bir tarih olamaz.");
    }
    [Fact(DisplayName = "Validator: null DueDate geçerli olmalı")]
    public void NullDueDate_ShouldPass()
    {
        var dto = new CreateTaskDto { Title = "Geçerli", DueDate = null };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }
}
