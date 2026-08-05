using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskManagement.Tests.Infrastructure;
namespace TaskManagement.Tests.Auth;
public class AuthTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private static object RegisterDto(
        string username  = "testuser",
        string email     = "test@test.com",
        string password  = "Test1234!",
        string firstName = "Test",
        string lastName  = "User") =>
        new { username, email, password, firstName, lastName };
    private static object LoginDto(string email = "test@test.com", string password = "Test1234!") =>
        new { email, password };
    // ── REGISTER ─────────────────────────────────────────────────────────
    [Fact(DisplayName = "Register: Geçerli bilgilerle kayıt → 201 döner")]
    public async Task Register_ValidRequest_Returns201()
    {
        // Her çalıştırmada benzersiz kullanıcı: DB birikimi önlenir
        var uid = Guid.NewGuid().ToString("N")[..8];
        var response = await _client.PostAsJsonAsync("/api/Auth/register",
            RegisterDto(username: $"valid{uid}", email: $"valid{uid}@test.com"));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("success").GetBoolean().Should().BeTrue();
    }
    [Fact(DisplayName = "Register: Aynı e-posta ikinci kez → 201 değil döner")]
    public async Task Register_DuplicateEmail_ReturnsError()
    {
        var uid = Guid.NewGuid().ToString("N")[..8];
        await _client.PostAsJsonAsync("/api/Auth/register",
            RegisterDto(username: $"dup1{uid}", email: $"dup{uid}@test.com"));
        var response = await _client.PostAsJsonAsync("/api/Auth/register",
            RegisterDto(username: $"dup2{uid}", email: $"dup{uid}@test.com"));
        response.StatusCode.Should().NotBe(HttpStatusCode.Created);
    }
    [Fact(DisplayName = "Register: Aynı kullanıcı adı ikinci kez → 201 değil döner")]
    public async Task Register_DuplicateUsername_ReturnsError()
    {
        var uid = Guid.NewGuid().ToString("N")[..8];
        await _client.PostAsJsonAsync("/api/Auth/register",
            RegisterDto(username: $"same{uid}", email: $"same1{uid}@test.com"));
        var response = await _client.PostAsJsonAsync("/api/Auth/register",
            RegisterDto(username: $"same{uid}", email: $"same2{uid}@test.com"));
        response.StatusCode.Should().NotBe(HttpStatusCode.Created);
    }
    [Fact(DisplayName = "Register: Zayıf şifre (sadece harf) → 400 döner")]
    public async Task Register_WeakPassword_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/register",
            RegisterDto(username: "weakpass", email: "weak@test.com", password: "onlyletters"));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact(DisplayName = "Register: Geçersiz e-posta formatı → 400 döner")]
    public async Task Register_InvalidEmail_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/register",
            RegisterDto(username: "invalidemail", email: "not-an-email"));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact(DisplayName = "Register: Kullanıcı adı 2 karakter (min 3) → 400 döner")]
    public async Task Register_ShortUsername_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/register",
            RegisterDto(username: "ab", email: "short@test.com"));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    // ── LOGIN ─────────────────────────────────────────────────────────────
    [Fact(DisplayName = "Login: Doğru bilgilerle giriş → 200 ve accessToken döner")]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        var uid = Guid.NewGuid().ToString("N")[..8];
        await _client.PostAsJsonAsync("/api/Auth/register",
            RegisterDto(username: $"login{uid}", email: $"login{uid}@test.com"));
        var response = await _client.PostAsJsonAsync("/api/Auth/login",
            LoginDto(email: $"login{uid}@test.com"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json  = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = json.GetProperty("data").GetProperty("accessToken").GetString();
        token.Should().NotBeNullOrEmpty();
    }
    [Fact(DisplayName = "Login: Yanlış şifre → 401 döner")]
    public async Task Login_WrongPassword_Returns401()
    {
        var uid = Guid.NewGuid().ToString("N")[..8];
        await _client.PostAsJsonAsync("/api/Auth/register",
            RegisterDto(username: $"wrong{uid}", email: $"wrong{uid}@test.com"));
        var response = await _client.PostAsJsonAsync("/api/Auth/login",
            LoginDto(email: $"wrong{uid}@test.com", password: "WrongPass99!"));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    [Fact(DisplayName = "Login: Kayıtlı olmayan e-posta → 401 döner")]
    public async Task Login_NonExistentUser_Returns401()
    {
        var uid = Guid.NewGuid().ToString("N")[..8];
        var response = await _client.PostAsJsonAsync("/api/Auth/login",
            LoginDto(email: $"ghost{uid}@test.com"));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    [Fact(DisplayName = "Korumalı endpoint: Token olmadan → 401 döner")]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/Tasks");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
