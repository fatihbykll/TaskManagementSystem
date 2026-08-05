using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
namespace TaskManagement.Tests.Infrastructure;
/// <summary>
/// Environment variable tabanlı WebApplicationFactory.
/// Environment variables: User Secrets ve appsettings.json'dan daha yüksek önceliğe sahiptir.
/// UseEnvironment("Testing"): User Secrets yalnızca "Development"'ta yüklenir; böylece
/// production DB'ye yanlışlıkla bağlanma riski ortadan kalkar.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestConnStr =
        "Host=localhost;Database=TaskManagementDbTest;Username=fatihbiyikli;Password=";
    public TestWebApplicationFactory()
    {
        // Environment variables: User Secrets'tan önce gelir
        // ASP.NET Core nested key separator: __ (çift alt çizgi)
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__PostgresConnection", TestConnStr);
        Environment.SetEnvironmentVariable(
            "JwtSettings__SecretKey", "TestSecretKeyForIntegrationTests_32charsX");
        Environment.SetEnvironmentVariable(
            "JwtSettings__Issuer", "TaskManagementAPI");
        Environment.SetEnvironmentVariable(
            "JwtSettings__Audience", "TaskManagementClient");
        Environment.SetEnvironmentVariable(
            "JwtSettings__ExpiryInMinutes", "60");
        Environment.SetEnvironmentVariable(
            "DatabaseProvider", "Postgres");
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing": User Secrets yalnızca "Development"'ta yüklenir
        // Bu sayede local user-secrets bizi override edemez
        builder.UseEnvironment("Testing");
    }
}
