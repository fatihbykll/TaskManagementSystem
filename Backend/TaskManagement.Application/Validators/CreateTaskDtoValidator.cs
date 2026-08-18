using FluentValidation;
using TaskManagement.Application.DTOs;
namespace TaskManagement.Application.Validators;
public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Görev başlığı boş olamaz.")
            .MaximumLength(100).WithMessage("Görev başlığı 100 karakteri geçemez.");
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama 500 karakteri geçemez.");
        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow.AddDays(-1)).WithMessage("Son teslim tarihi geçmiş bir tarih olamaz.");
    }
}
