namespace TaskManagement.Application.Interfaces;
public interface IRecurringTaskGeneratorJob
{
    Task ExecuteAsync(CancellationToken ct = default);
}
