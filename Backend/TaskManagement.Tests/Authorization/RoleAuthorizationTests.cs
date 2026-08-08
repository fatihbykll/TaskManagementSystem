using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskManagement.Tests.Infrastructure;
namespace TaskManagement.Tests.Authorization;
public class RoleAuthorizationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    public RoleAuthorizationTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();
    private async Task<string> GetTokenAsync(string email, string username)
    {
        await _client.PostAsJsonAsync("/api/Auth/register",
            new { username, email, password = "Test1234!", firstName = "T", lastName = "U" });
        var r = await _client.PostAsJsonAsync("/api/Auth/login", new { email, password = "Test1234!" });
        var j = await r.Content.ReadFromJsonAsync<JsonElement>();
        return j.GetProperty("data").GetProperty("accessToken").GetString()!;
    }
    [Fact(DisplayName = "Admin: /api/Admin/statistics → 200 döner")]
    public async Task Admin_Statistics_Returns200()
    {
        var uid = Guid.NewGuid().ToString("N")[..5];
        var token = await GetTokenAsync("admin@milsoft.com", $"adm{uid}");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var r = await _client.GetAsync("/api/Admin/statistics");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        var j = await r.Content.ReadFromJsonAsync<JsonElement>();
        j.GetProperty("success").GetBoolean().Should().BeTrue();
        j.GetProperty("data").GetProperty("totalUsers").GetInt32().Should().BeGreaterThan(0);
    }
    [Fact(DisplayName = "Admin: /api/Admin/users → 200 ve liste döner")]
    public async Task Admin_Users_Returns200WithList()
    {
        var uid = Guid.NewGuid().ToString("N")[..5];
        var token = await GetTokenAsync("admin@milsoft.com", $"adm2{uid}");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var r = await _client.GetAsync("/api/Admin/users");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        var j = await r.Content.ReadFromJsonAsync<JsonElement>();
        j.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }
    [Fact(DisplayName = "RegularUser: /api/Admin/statistics → 403 Forbidden döner")]
    public async Task RegularUser_Statistics_Returns403()
    {
        var uid = Guid.NewGuid().ToString("N")[..6];
        var token = await GetTokenAsync($"usr{uid}@test.com", $"usr{uid}");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var r = await _client.GetAsync("/api/Admin/statistics");
        r.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    [Fact(DisplayName = "RegularUser: /api/Admin/users → 403 Forbidden döner")]
    public async Task RegularUser_Users_Returns403()
    {
        var uid = Guid.NewGuid().ToString("N")[..6];
        var token = await GetTokenAsync($"ru{uid}@test.com", $"ru{uid}");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var r = await _client.GetAsync("/api/Admin/users");
        r.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    [Fact(DisplayName = "NoToken: /api/Admin/statistics → 401 Unauthorized döner")]
    public async Task NoToken_AdminEndpoint_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var r = await _client.GetAsync("/api/Admin/statistics");
        r.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
