using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskManagement.Tests.Infrastructure;
namespace TaskManagement.Tests.Tasks;
public class RecurringTaskTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    public RecurringTaskTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    private async Task<string> GetTokenAsync(string suffix = "")
    {
        var uid      = Guid.NewGuid().ToString("N")[..6];
        var email    = $"recurring{suffix}{uid}@test.com";
        var username = $"recuser{suffix}{uid}";
        await _client.PostAsJsonAsync("/api/Auth/register",
            new { username, email, password = "Test1234!", firstName = "Rec", lastName = "User" });
        var loginResp = await _client.PostAsJsonAsync("/api/Auth/login",
            new { email, password = "Test1234!" });
        var json = await loginResp.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("data").GetProperty("accessToken").GetString()!;
    }
    private void Authorize(string token) =>
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    [Fact(DisplayName = "RecurringTask Create: Daily frekansıyla → 201 ve nextRunDate dolu döner")]
    public async Task CreateTask_WithDailyFrequency_Returns201AndNextRunDate()
    {
        Authorize(await GetTokenAsync("r1"));
        var response = await _client.PostAsJsonAsync("/api/Tasks", new
        {
            title = "Günlük Tekrarlayan Görev",
            description = "Her gün tekrarlar",
            priority = 1,
            dueDate = (string?)null,
            categoryId = (string?)null,
            recurringFrequency = 1 // Daily
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("success").GetBoolean().Should().BeTrue();
        json.GetProperty("data").GetProperty("recurringFrequency").GetInt32().Should().Be(1);
    }
    [Fact(DisplayName = "RecurringTask Create: Weekly frekansıyla → 201 döner")]
    public async Task CreateTask_WithWeeklyFrequency_Returns201()
    {
        Authorize(await GetTokenAsync("r2"));
        var response = await _client.PostAsJsonAsync("/api/Tasks", new
        {
            title = "Haftalık Tekrarlayan Görev",
            description = "Her hafta tekrarlar",
            priority = 2,
            dueDate = (string?)null,
            categoryId = (string?)null,
            recurringFrequency = 2 // Weekly
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
    [Fact(DisplayName = "RecurringTask Create: None frekansıyla → 201 döner (normal görev)")]
    public async Task CreateTask_WithNoneFrequency_Returns201()
    {
        Authorize(await GetTokenAsync("r3"));
        var response = await _client.PostAsJsonAsync("/api/Tasks", new
        {
            title = "Normal Görev",
            description = "Tekrarlanmaz",
            priority = 1,
            dueDate = (string?)null,
            categoryId = (string?)null,
            recurringFrequency = 0 // None
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("data").GetProperty("recurringFrequency").GetInt32().Should().Be(0);
    }
    [Fact(DisplayName = "Productivity: Endpoint erişilebilir ve başarılı yanıt döner")]
    public async Task GetProductivity_AuthorizedUser_Returns200()
    {
        Authorize(await GetTokenAsync("r4"));
        var response = await _client.GetAsync("/api/Tasks/productivity");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("success").GetBoolean().Should().BeTrue();
    }
}
