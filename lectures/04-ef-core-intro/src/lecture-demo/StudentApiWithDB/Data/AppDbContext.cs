using Microsoft.EntityFrameworkCore;
using StudentApiWithDB.Models;

namespace StudentApiWithDB.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    // Таблица студентов
    public DbSet<Student> Students => Set<Student>();
}