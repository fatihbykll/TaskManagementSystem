using Microsoft.AspNetCore.ResponseCompression;
using TaskManagement.API.Middleware;
using Microsoft.AspNetCore.RateLimiting;
using Asp.Versioning;
using Hangfire;
using Hangfire.PostgreSql;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Services;
using TaskManagement.API.Services;
using TaskManagement.Infrastructure.Jobs;
using TaskManagement.API.Hubs;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Mappings;
using TaskManagement.Application.Services;
using TaskManagement.Application.Settings;
// AppSettings using mevcut
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Infrastructure.Services;
using TaskManagement.API.Services;
using TaskManagement.API.Middleware;
// ─── Serilog: İki aşamalı başlatma ──────────────────────────────────────────
// Bootstrap logger: configuration yüklenmeden önce oluşan hataları yakalar.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
    // Serilog'u ASP.NET Core host'a entegre et; appsettings.json'dan konfigürasyon okunur.
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());
    // ─── Veritabanı: Dual-Provider (Postgres / Oracle) ────────────────────────
    var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "Postgres";
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        switch (databaseProvider)
        {
            case "Oracle":
                options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection"),
                    o => o.MigrationsAssembly("TaskManagement.Infrastructure"));
                break;
            default:
                options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"),
                    o => o.MigrationsAssembly("TaskManagement.Infrastructure"));
                break;
        }
    });
    // ─── JWT: Strongly-typed config binding ───────────────────────────────────
    builder.Services.Configure<AppSettings>(o => o.AdminEmail = builder.Configuration["AdminEmail"] ?? string.Empty);
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
    // ─── Authentication & Authorization ───────────────────────────────────────
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            // ClockSkew sıfırlanır; token tam süresi dolduğunda geçersiz sayılır.
            ClockSkew = TimeSpan.Zero
        };
    });
    builder.Services.AddAuthorization();
    // ─── CORS: Angular geliştirme ve production origin'leri ───────────────────
    var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                         ?? new[] { "http://localhost:4200" };
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AngularPolicy", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  // Credential (cookie/auth header) aktarımına izin verir.
                  .AllowCredentials();
        });
    });
    // ─── Dosya yükleme: 10 MB limit ───────────────────────────────────────────
    builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 10 * 1024 * 1024);
    builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 10 * 1024 * 1024);
    // ─── Repository & Unit of Work ────────────────────────────────────────────
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    // ─── Application Services ─────────────────────────────────────────────────
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<ITaskService, TaskService>();
    builder.Services.AddScoped<IJwtService, JwtService>();
    builder.Services.AddScoped<ICommentService, CommentService>();
    builder.Services.AddScoped<IAttachmentService, AttachmentService>();
    builder.Services.AddScoped<IReportService, ReportService>();

    builder.Services.AddScoped<IEmailService, MockEmailService>();
    builder.Services.AddScoped<IInactiveUserReminderJob, InactiveUserReminderJob>();
    builder.Services.AddScoped<IRecurringTaskGeneratorJob, RecurringTaskGeneratorJob>();


    // ─── AutoMapper ───────────────────────────────────────────────────────────
    builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
    // ─── API Versioning ────────────────────────────────────────────────────────
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
    // ─── Health Checks ─────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>("database", HealthStatus.Unhealthy);
    // ─── Swagger: JWT Bearer desteği ile ─────────────────────────────────────
    // ─── Redis Distributed Cache ──────────────────────────────────────────────
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("RedisConnection") ?? "localhost:6379";
        options.InstanceName = "TaskManagement_";
    });

    // ─── Hangfire Background Jobs ─────────────────────────────────────────────
    var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
    builder.Services.AddHangfire(config =>
        config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
              .UseSimpleAssemblyNameTypeSerializer()
              .UseRecommendedSerializerSettings()
              .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));
    builder.Services.AddHangfireServer();

    // ─── SignalR ──────────────────────────────────────────────────────────────
    builder.Services.AddSignalR();
    builder.Services.AddScoped<INotificationService, SignalRNotificationService>();

    // ─── Rate Limiting (Brute-force koruması — Testing ortamında devre dışı) ──
    if (!builder.Environment.IsEnvironment("Testing"))
    builder.Services.AddRateLimiter(options =>
    {
        // "auth" policy: Login/Register endpoint'leri için — IP başına 10 istek/dakika
        options.AddFixedWindowLimiter("auth", limiterOptions =>
        {
            limiterOptions.PermitLimit = 10;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueLimit = 0;
        });
        // "api" policy: Genel API endpoint'leri için — IP başına 100 istek/dakika
        options.AddFixedWindowLimiter("api", limiterOptions =>
        {
            limiterOptions.PermitLimit = 100;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueLimit = 5;
        });
        options.OnRejected = async (ctx, ct) =>
        {
            ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await ctx.HttpContext.Response.WriteAsJsonAsync(
                new { success = false, message = "Çok fazla istek gönderildi. Lütfen bekleyin." }, ct);
        };
    });
    // ─── Response Compression (Gzip/Brotli) ─────────────────────────────────
    builder.Services.AddResponseCompression(opts =>
    {
        opts.EnableForHttps = true;
        opts.Providers.Add<GzipCompressionProvider>();
        opts.Providers.Add<BrotliCompressionProvider>();
        opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            new[] { "application/json", "text/json" });
    });
    builder.Services.Configure<GzipCompressionProviderOptions>(opts =>
        opts.Level = System.IO.Compression.CompressionLevel.Fastest);
    // ─── API Versioning ──────────────────────────────────────────────────────
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
    // ─── Fluent Validation ───────────────────────────────────────────────────
    builder.Services.AddFluentValidationAutoValidation()
                    .AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskDtoValidator>();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Task Management API",
            Version = "v1",
            Description = "Kişisel Görev Yönetim Sistemi REST API"
        });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT token girin. Örnek: Bearer eyJhbGci..."
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
        // XML Dokümantasyon — Controller /// summary'leri Swagger'da görünür
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (System.IO.File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    });
    var app = builder.Build();
    // wwwroot/uploads dizininin varlığını garanti eder.
    var uploadsDir = Path.Combine(
        app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
        "uploads", "attachments");
    Directory.CreateDirectory(uploadsDir);
    // Serilog request logging: her HTTP isteği otomatik loglanır.
    // ─── Correlation ID + Compression ────────────────────────────────────────
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseResponseCompression();
    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} → {StatusCode} | {Elapsed:0}ms";
    });
    // Global exception handler; tüm işlenmemiş exception'lar ApiResponse formatında döner.
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    // CORS; UseRouting'den sonra, UseAuthentication'dan önce gelmelidir.
    app.UseCors("AngularPolicy");
    // UseAuthentication, UseAuthorization'dan önce gelmek zorunda; middleware sırası kritiktir.
    app.UseAuthentication();
    app.UseAuthorization();
    // ─── Hangfire Dashboard ve Job Tanımlaması ────────────────────────────────
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter() }
    });

    RecurringJob.AddOrUpdate<IRecurringTaskGeneratorJob>(
        "recurring-task-generator",
        job => job.ExecuteAsync(CancellationToken.None),
        Cron.Daily);


    RecurringJob.AddOrUpdate<IInactiveUserReminderJob>(
        "inactive-user-reminder",
        job => job.ExecuteAsync(CancellationToken.None),
        Cron.Daily);

    if (!app.Environment.IsEnvironment("Testing")) app.UseRateLimiter();
    app.MapControllers();
    app.MapHub<NotificationHub>("/hubs/notifications");

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString(), description = e.Value.Description })
            });
            await context.Response.WriteAsync(result);
        }
    });
    app.Run();
}
catch (Exception ex)
{
    // Host başlatma hatası; bootstrap logger ile yakalanır.
    Log.Fatal(ex, "Uygulama başlatılamadı.");
}
finally
{
    // Buffer'daki tüm loglar flush edilir; uygulama kapanırken log kaybı önlenir.
    Log.CloseAndFlush();
}

// WebApplicationFactory erişimi için gerekli (Test projesi kullanır)
public partial class Program { }
