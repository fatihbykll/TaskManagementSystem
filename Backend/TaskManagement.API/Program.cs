using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Mappings;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Repositories;
var builder = WebApplication.CreateBuilder(args);
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
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
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
app.UseAuthorization();
app.MapControllers();
app.Run();
