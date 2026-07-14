using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Mappings;
using TaskManagement.Application.Services;
using TaskManagement.Application.Settings;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;
using TaskManagement.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Veritabanı: Dual-Provider (Postgres / Oracle) ────────────────────────────
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "Postgres";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    switch (databaseProvider)
    {
        case "Oracle":
            options.UseOracle(
                builder.Configuration.GetConnectionString("OracleConnection"),
                o => o.MigrationsAssembly("TaskManagement.Infrastructure"));
            break;
        case "Postgres":
        default:
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("PostgresConnection"),
                o => o.MigrationsAssembly("TaskManagement.Infrastructure"));
            break;
    }
});

// ─── JWT: Strongly-typed config binding ───────────────────────────────────────
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

// ─── Authentication & Authorization ───────────────────────────────────────────
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

// ─── Repository & Unit of Work ────────────────────────────────────────────────
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ─── Application Services ─────────────────────────────────────────────────────
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// ─── AutoMapper ───────────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// ─── Swagger ──────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// UseAuthentication, UseAuthorization'dan önce gelmek zorunda; middleware sırası kritiktir.
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
