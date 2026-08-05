using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskManagement.Tests.Infrastructure;
namespace TaskManagement.Tests.Tasks;
public class TaskCrudTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    public TaskCrudTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    private async Task<string> GetTokenAsync(string suffix = "")
    {
        var uid      = Guid.NewGuid().ToString("N")[..6];
        var email    = $"tasktest{suffix}{uid}@test.com";
        var username = $"taskuser{suffix}{uid}";
        await _client.PostAsJsonAsync("/api/Auth/register",
            new { username, email, password = "Test1234!", firstName = "Test", lastName = "User" });
        var loginResp = await _client.PostAsJsonAsync("/api/Auth/login",
            new { email, password = "Test1234!" });
        var json = await loginResp.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("data").GetProperty("accessToken").GetString()!;
    }
    private void Authorize(string token) =>
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    private static object TaskDto(string title = "Test Görevi", int priority = 1) =>
        new { title, description = "Açıklama", priority, dueDate = (string?)null, categoryId = (string?)null };
    private async Task<string> CreateTaskAndGetId(string title = "Test Görevi")
    {
        var resp = await _client.PostAsJsonAsync("/api/Tasks", TaskDto(title));
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("data").GetProperty("id").GetString()!;
    }
    // ── CRUD ──────────────────────────────────────────────────────────────
    [Fact(DisplayName = "Task Create: Geçerli verilerle → 201 döner")]
    public async Task CreateTask_ValidRequest_Returns201()
    {
        Authorize(await GetTokenAsync("c1"));
        var response = await _client.PostAsJsonAsync("/api/Tasks", TaskDto());
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("success").GetBoolean().Should().BeTrue();
        json.GetProperty("data").GetProperty("title").GetString().Should().Be("Test Görevi");
    }
    [Fact(DisplayName = "Task Create: Başlık boş → 400 döner")]
    public async Task CreateTask_EmptyTitle_Returns400()
    {
        Authorize(await GetTokenAsync("c2"));
        var response = await _client.PostAsJsonAsync("/api/Tasks", TaskDto(title: ""));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact(DisplayName = "Task GetById: Var olan görev → 200 ve doğru başlık döner")]
    public async Task GetTaskById_ExistingTask_Returns200()
    {
        Authorize(await GetTokenAsync("g1"));
        var id       = await CreateTaskAndGetId("Detay Testi");
        var response = await _client.GetAsync($"/api/Tasks/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("data").GetProperty("title").GetString().Should().Be("Detay Testi");
    }
    [Fact(DisplayName = "Task GetById: Olmayan GUID → 404 döner")]
    public async Task GetTaskById_NotFound_Returns404()
    {
        Authorize(await GetTokenAsync("g2"));
        var response = await _client.GetAsync($"/api/Tasks/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    [Fact(DisplayName = "Task Update: Başlık güncelleme → 200 ve yeni başlık döner")]
    public async Task UpdateTask_ValidRequest_Returns200()
    {
        Authorize(await GetTokenAsync("u1"));
        var id         = await CreateTaskAndGetId("Eski Başlık");
        var updateResp = await _client.PutAsJsonAsync($"/api/Tasks/{id}", TaskDto(title: "Yeni Başlık"));
        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await updateResp.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("data").GetProperty("title").GetString().Should().Be("Yeni Başlık");
    }
    [Fact(DisplayName = "Task Delete: Silme → 200, sonrasında GetById → 404")]
    public async Task DeleteTask_ThenGet_Returns404()
    {
        Authorize(await GetTokenAsync("d1"));
        var id         = await CreateTaskAndGetId("Silinecek");
        var deleteResp = await _client.DeleteAsync($"/api/Tasks/{id}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var getResp = await _client.GetAsync($"/api/Tasks/{id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    [Fact(DisplayName = "Task StatusPatch: InProgress'e geçiş → 200 döner")]
    public async Task PatchStatus_ToInProgress_Returns200()
    {
        Authorize(await GetTokenAsync("s1"));
        var id       = await CreateTaskAndGetId();
        var response = await _client.PatchAsJsonAsync($"/api/Tasks/{id}/status", new { status = 1 });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    // ── FİLTRELEME / ARAMA (IQueryable) ──────────────────────────────────
    [Fact(DisplayName = "Filter: searchTerm → sadece eşleşen görevler gelir")]
    public async Task Filter_BySearchTerm_ReturnsOnlyMatching()
    {
        Authorize(await GetTokenAsync("f1"));
        // Her çalıştırmada benzersiz prefix: önceki test verisiyle karışmaz
        var p = Guid.NewGuid().ToString("N")[..8];
        await _client.PostAsJsonAsync("/api/Tasks", TaskDto($"{p} Angular Kanban"));
        await _client.PostAsJsonAsync("/api/Tasks", TaskDto($"{p} Backend API"));
        await _client.PostAsJsonAsync("/api/Tasks", TaskDto($"{p} Angular Unit"));
        var response = await _client.GetAsync(
            $"/api/Tasks?searchTerm={p}%20Angular&pageNumber=1&pageSize=50");
        var json  = await response.Content.ReadFromJsonAsync<JsonElement>();
        var items = json.GetProperty("data").GetProperty("data");
        items.GetArrayLength().Should().Be(2);
    }
    [Fact(DisplayName = "Filter: priority=2 → yüksek öncelikli görev listede")]
    public async Task Filter_ByPriority_ReturnsOnlyMatching()
    {
        Authorize(await GetTokenAsync("f2"));
        var uid = Guid.NewGuid().ToString("N")[..8];
        await _client.PostAsJsonAsync("/api/Tasks", TaskDto($"Düşük{uid}",  priority: 0));
        await _client.PostAsJsonAsync("/api/Tasks", TaskDto($"Yüksek{uid}", priority: 2));
        await _client.PostAsJsonAsync("/api/Tasks", TaskDto($"Kritik{uid}", priority: 3));
        // Tüm priority=2 görevleri al (önceki çalıştırma datası da gelebilir)
        var response = await _client.GetAsync("/api/Tasks?priority=2&pageNumber=1&pageSize=100");
        var json     = await response.Content.ReadFromJsonAsync<JsonElement>();
        var items    = json.GetProperty("data").GetProperty("data");
        // Bu çalıştırmada eklediğimiz Yüksek{uid} mutlaka listede olmalı
        var titles = Enumerable.Range(0, items.GetArrayLength())
            .Select(i => items[i].GetProperty("title").GetString())
            .ToList();
        titles.Should().Contain($"Yüksek{uid}");
        // Dönen tüm görevler priority=2 olmalı (IQueryable filtre doğrulaması)
        for (int i = 0; i < items.GetArrayLength(); i++)
            items[i].GetProperty("priority").GetInt32().Should().Be(2);
    }
    [Fact(DisplayName = "Filter: pageSize=2 ile ilk sayfada 2 görev döner")]
    public async Task Filter_Pagination_CorrectPageSize()
    {
        Authorize(await GetTokenAsync("f3"));
        var uid = Guid.NewGuid().ToString("N")[..8];
        for (int i = 1; i <= 3; i++)
            await _client.PostAsJsonAsync("/api/Tasks", TaskDto($"Sayfalama{uid} {i}"));
        var response  = await _client.GetAsync("/api/Tasks?pageNumber=1&pageSize=2");
        var json      = await response.Content.ReadFromJsonAsync<JsonElement>();
        var data      = json.GetProperty("data");
        // pageSize=2 ile ilk sayfada en fazla 2 kayıt gelmeli
        data.GetProperty("data").GetArrayLength().Should().BeLessThanOrEqualTo(2);
        data.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
        // 3+ görev var, hasNextPage bekleniyor
        data.GetProperty("hasNextPage").GetBoolean().Should().BeTrue();
    }
}
