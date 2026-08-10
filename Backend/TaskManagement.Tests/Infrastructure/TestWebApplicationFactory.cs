using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
namespace TaskManagement.Tests.Infrastructure;
/// <summary>
/// Environment variable tabanlı WebApplicationFactory.
/// CI ortamında env var'lar GitHub Actions tarafından set edilir.
/// Local ortamda env var set edilmemişse yerel varsayılan değerler kullanılır.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public TestWebApplicationFactory()
    {
        // CI env var varsa onu kullan (GitHub Actions), yoksa local fallback
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
        {
            Environment.SetEnvironmentVariable("JwtSettings__Issuer", "TaskManagementAPI");
        }

        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JwtSettings__Audience")))
        {
            Environment.SetEnvironmentVariable("JwtSettings__Audience", "TaskManagementClient");
        }

        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JwtSettings__ExpiryInMinutes")))
        {
            Environment.SetEnvironmentVariable("JwtSettings__ExpiryInMinutes", "60");
        }

        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DatabaseProvider")))
        {
            Environment.SetEnvironmentVariable("DatabaseProvider", "Postgres");
        }

        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AdminEmail")))
        {
            Environment.SetEnvironmentVariable("AdminEmail", "admin@test.com");
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}
