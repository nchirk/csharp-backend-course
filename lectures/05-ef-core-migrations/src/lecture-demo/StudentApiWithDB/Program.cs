using Microsoft.EntityFrameworkCore;
using StudentApiWithDB.Data;

var builder = WebApplication.CreateBuilder(args);

// Регистрируем AppDbContext с использованием SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString)); // options.UseSqlite("Data Source=students.db"));

// var inMemoryConnection = builder.Configuration.GetConnectionString("InMemoryConnection");
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseInMemoryDatabase(inMemoryConnection));

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapControllers();

app.Run();