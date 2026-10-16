using Microsoft.EntityFrameworkCore;
using StudentApiWithDB.Models;

namespace StudentApiWithDB.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Group> Groups => Set<Group>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Автоматически находит и применяет ВСЕ конфигурации (наследники IEntityTypeConfiguration), 
        // которые находятся в текущей сборке проекта.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}