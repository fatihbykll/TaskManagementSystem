namespace TaskManagement.Application.Interfaces;
public interface IInactiveUserReminderJob
{
    Task ExecuteAsync(CancellationToken ct = default);
}
