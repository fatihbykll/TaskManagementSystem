using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Tests.Infrastructure;

/// <summary>
/// Environment variable tabanlı WebApplicationFactory.
/// CI ortaminda env var GitHub Actions tarafından set edilir.
/// Local ortamda set edilmemisse yerel varsayilan degerler kullanilir.
/// CreateHost override ile migration otomatik uygulanir.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public TestWebApplicationFactory()
    {
        // CI env var varsa onu kullan, yoksa local fallback
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ConnectionStrings__PostgresConnection")))
        {
            Environment.SetEnvironmentVariable(
                "ConnectionStrings__PostgresConnection",
                "Host=localhost;Database=TaskManagementDbTest;Username=fatihbiyikli;Password=");
        }

        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JwtSettings__SecretKey")))
        {
            Environment.SetEnvironmentVariable(
                "JwtSettings__SecretKey", "TestSecretKeyForIntegrationTests_32charsX");
        }

        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JwtSettings__Issuer")))
            Environment.SetEnvironmentVariable("JwtSettings__Issuer", "TaskManagementAPI");

        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JwtSettings__Audience")))
            Environment.SetEnvironmentVariable("JwtSettings__Audience", "TaskManagementClient");

        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JwtSettings__ExpiryInMinutes")))
            Environment.SetEnvironmentVariable("JwtSettings__ExpiryInMinutes", "60");

        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DatabaseProvider")))
            Environment.SetEnvironmentVariable("DatabaseProvider", "Postgres");

        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AdminEmail")))
            Environment.SetEnvironmentVariable("AdminEmail", "admin@test.com");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing": User Secrets yalnizca "Development" ortaminda yuklenir
        builder.UseEnvironment("Testing");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Migration'i host basladiktan sonra uygula
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();

        return host;
    }
}
