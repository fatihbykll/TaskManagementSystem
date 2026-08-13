using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskManagement.Infrastructure.Data;
namespace TaskManagement.Tests.Infrastructure;
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public TestWebApplicationFactory()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ConnectionStrings__PostgresConnection")))
            Environment.SetEnvironmentVariable(
                "ConnectionStrings__PostgresConnection",
                "Host=localhost;Database=TaskManagementDbTest;Username=fatihbiyikli;Password=");
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JwtSettings__SecretKey")))
            Environment.SetEnvironmentVariable("JwtSettings__SecretKey", "TestSecretKeyForIntegrationTests_32charsX");
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
        // Test ortamında Redis bağlantısı yoktur; InMemory ile eziyoruz.
        Environment.SetEnvironmentVariable("ConnectionStrings__RedisConnection", "");
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Redis (IDistributedCache) kaydını kaldır, InMemory ile değiştir.
            var redisDescriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(IDistributedCache));
            if (redisDescriptor != null)
                services.Remove(redisDescriptor);
            services.AddDistributedMemoryCache();
        });
    }
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
        return host;
    }
}
