using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Database Provider Configuration ---
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "Postgres";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    switch (databaseProvider)
    {
        case "Oracle":
            options.UseOracle(
                builder.Configuration.GetConnectionString("OracleConnection"),
                oracleOptions => oracleOptions.MigrationsAssembly("TaskManagement.Infrastructure"));
            break;
        case "Postgres":
        default:
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("PostgresConnection"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly("TaskManagement.Infrastructure"));
            break;
    }
});

// --- AutoMapper ---
builder.Services.AddAutoMapper(typeof(TaskManagement.Application.Mappings.MappingProfile).Assembly);

// --- Controllers ---
builder.Services.AddControllers();

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
