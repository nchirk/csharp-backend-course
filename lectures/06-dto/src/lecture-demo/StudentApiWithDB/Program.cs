using Microsoft.EntityFrameworkCore;
using StudentApi.Data.Data;
using StudentApi.Data.Services;

var builder = WebApplication.CreateBuilder(args);

// Регистрируем AppDbContext с использованием SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString)); // options.UseSqlite("Data Source=students.db"));

// var inMemoryConnection = builder.Configuration.GetConnectionString("InMemoryConnection");
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseInMemoryDatabase(inMemoryConnection));
builder.Services.AddScoped<IGroupService, GroupService>();

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapControllers();

app.Run();