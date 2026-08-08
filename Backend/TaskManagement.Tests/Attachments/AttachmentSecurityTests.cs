using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskManagement.Tests.Infrastructure;
namespace TaskManagement.Tests.Attachments;
public class AttachmentSecurityTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    public AttachmentSecurityTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();
    private async Task<string> GetTokenAsync()
    {
        var uid = Guid.NewGuid().ToString("N")[..6];
        var email = $"att{uid}@test.com";
        await _client.PostAsJsonAsync("/api/Auth/register",
            new { username = $"att{uid}", email, password = "Test1234!", firstName = "A", lastName = "B" });
        var r = await _client.PostAsJsonAsync("/api/Auth/login", new { email, password = "Test1234!" });
        var j = await r.Content.ReadFromJsonAsync<JsonElement>();
        return j.GetProperty("data").GetProperty("accessToken").GetString()!;
    }
    private async Task<Guid> CreateTaskAsync()
    {
        var r = await _client.PostAsJsonAsync("/api/Tasks",
            new { title = "Attach Testi", description = "d", priority = 1,
                  dueDate = (string?)null, categoryId = (string?)null });
        var j = await r.Content.ReadFromJsonAsync<JsonElement>();
        return Guid.Parse(j.GetProperty("data").GetProperty("id").GetString()!);
    }
    private static MultipartFormDataContent BuildFile(string name, string mime, byte[]? data = null)
    {
        var form = new MultipartFormDataContent();
        var fc = new ByteArrayContent(data ?? new byte[] { 0x00, 0x01, 0x02, 0x03 });
        fc.Headers.ContentType = MediaTypeHeaderValue.Parse(mime);
        form.Add(fc, "file", name);
        return form;
    }
    [Fact(DisplayName = "Upload: .jpg gecerli resim → 201 döner")]
    public async Task Upload_ValidJpeg_Returns201()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync());
        var id = await CreateTaskAsync();
        var r = await _client.PostAsync($"/api/tasks/{id}/attachments",
            BuildFile("foto.jpg", "image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }));
        r.StatusCode.Should().Be(HttpStatusCode.Created);
    }
    [Fact(DisplayName = "Upload: .pdf gecerli belge → 201 döner")]
    public async Task Upload_ValidPdf_Returns201()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync());
        var id = await CreateTaskAsync();
        var r = await _client.PostAsync($"/api/tasks/{id}/attachments",
            BuildFile("rapor.pdf", "application/pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 }));
        r.StatusCode.Should().Be(HttpStatusCode.Created);
    }
    [Fact(DisplayName = "Upload: .txt gecerli metin → 201 döner")]
    public async Task Upload_ValidTxt_Returns201()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync());
        var id = await CreateTaskAsync();
        var r = await _client.PostAsync($"/api/tasks/{id}/attachments",
            BuildFile("not.txt", "text/plain", System.Text.Encoding.UTF8.GetBytes("test")));
        r.StatusCode.Should().Be(HttpStatusCode.Created);
    }
    [Fact(DisplayName = "Upload: .exe uzantisi (calistirilabilir) → 400 döner")]
    public async Task Upload_ExeExtension_Returns400()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync());
        var id = await CreateTaskAsync();
        var r = await _client.PostAsync($"/api/tasks/{id}/attachments",
            BuildFile("mal.exe", "application/octet-stream", new byte[] { 0x4D, 0x5A }));
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await r.Content.ReadAsStringAsync();
        body.Should().Contain("Desteklenmeyen dosya türü");
    }
    [Fact(DisplayName = "Upload: .zip uzantisi (arsiv) → 400 döner")]
    public async Task Upload_ZipExtension_Returns400()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync());
        var id = await CreateTaskAsync();
        var r = await _client.PostAsync($"/api/tasks/{id}/attachments",
            BuildFile("arc.zip", "application/zip", new byte[] { 0x50, 0x4B, 0x03, 0x04 }));
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact(DisplayName = "Upload: .sh uzantisi (shell script) → 400 döner")]
    public async Task Upload_ShellScript_Returns400()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync());
        var id = await CreateTaskAsync();
        var r = await _client.PostAsync($"/api/tasks/{id}/attachments",
            BuildFile("hack.sh", "text/plain", System.Text.Encoding.UTF8.GetBytes("#!/bin/bash")));
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact(DisplayName = "Upload: .jpg + yanlis MIME (MIME sahtekarlik) → 400 döner")]
    public async Task Upload_ExtensionMimeMismatch_Returns400()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync());
        var id = await CreateTaskAsync();
        var r = await _client.PostAsync($"/api/tasks/{id}/attachments",
            BuildFile("sahte.jpg", "application/octet-stream"));
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await r.Content.ReadAsStringAsync();
        body.Should().Contain("MIME");
    }
    [Fact(DisplayName = "Upload: 10MB+1 byte asimi → 400 veya 413 döner")]
    public async Task Upload_ExceedsMaxSize_ReturnsError()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync());
        var id = await CreateTaskAsync();
        var r = await _client.PostAsync($"/api/tasks/{id}/attachments",
            BuildFile("buyuk.pdf", "application/pdf", new byte[10 * 1024 * 1024 + 1]));
        ((int)r.StatusCode).Should().BeGreaterThanOrEqualTo(400);
    }
}
