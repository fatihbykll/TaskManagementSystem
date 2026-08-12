namespace TaskManagement.Application.Interfaces;
public interface IEmailService
{
    Task SendReminderAsync(string to, string subject, string body, CancellationToken ct = default);
}
