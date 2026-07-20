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
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Infrastructure.Services;
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
    // ─── AutoMapper ───────────────────────────────────────────────────────────
    builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
    // ─── Swagger: JWT Bearer desteği ile ─────────────────────────────────────
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
    });
    var app = builder.Build();
    // wwwroot/uploads dizininin varlığını garanti eder.
    var uploadsDir = Path.Combine(
        app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
        "uploads", "attachments");
    Directory.CreateDirectory(uploadsDir);
    // Serilog request logging: her HTTP isteği otomatik loglanır.
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
    app.MapControllers();
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
