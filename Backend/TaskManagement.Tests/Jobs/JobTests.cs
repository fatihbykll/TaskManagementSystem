using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.Interfaces;
using TaskManagement.Tests.Infrastructure;
namespace TaskManagement.Tests.Jobs;
public class JobTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;
    public JobTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    private async Task<string> GetTokenAsync(string suffix = "")
    {
        var uid      = Guid.NewGuid().ToString("N")[..6];
        var email    = $"jobtest{suffix}{uid}@test.com";
        var username = $"jobuser{suffix}{uid}";
        await _client.PostAsJsonAsync("/api/Auth/register",
            new { username, email, password = "Test1234!", firstName = "Job", lastName = "Test" });
        var loginResp = await _client.PostAsJsonAsync("/api/Auth/login",
            new { email, password = "Test1234!" });
        var json = await loginResp.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("data").GetProperty("accessToken").GetString()!;
    }
    [Fact(DisplayName = "InactiveUserReminderJob: Servis DI container'da kayıtlı olmalı")]
    public void InactiveUserReminderJob_IsRegisteredInDI()
    {
        using var scope = _factory.Services.CreateScope();
        var job = scope.ServiceProvider.GetService<IInactiveUserReminderJob>();
        job.Should().NotBeNull("InactiveUserReminderJob DI container'a kayıtlı olmalıdır.");
    }
    [Fact(DisplayName = "RecurringTaskGeneratorJob: Servis DI container'da kayıtlı olmalı")]
    public void RecurringTaskGeneratorJob_IsRegisteredInDI()
    {
        using var scope = _factory.Services.CreateScope();
        var job = scope.ServiceProvider.GetService<IRecurringTaskGeneratorJob>();
        job.Should().NotBeNull("RecurringTaskGeneratorJob DI container'a kayıtlı olmalıdır.");
    }
    [Fact(DisplayName = "InactiveUserReminderJob: ExecuteAsync hiç exception fırlatmamalı")]
    public async Task InactiveUserReminderJob_ExecuteAsync_DoesNotThrow()
    {
        using var scope = _factory.Services.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<IInactiveUserReminderJob>();
        var act = async () => await job.ExecuteAsync();
        await act.Should().NotThrowAsync("Job çalışırken beklenmedik exception oluşmamalıdır.");
    }
    [Fact(DisplayName = "RecurringTaskGeneratorJob: ExecuteAsync hiç exception fırlatmamalı")]
    public async Task RecurringTaskGeneratorJob_ExecuteAsync_DoesNotThrow()
    {
        using var scope = _factory.Services.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<IRecurringTaskGeneratorJob>();
        var act = async () => await job.ExecuteAsync();
        await act.Should().NotThrowAsync("Job çalışırken beklenmedik exception oluşmamalıdır.");
    }
}
