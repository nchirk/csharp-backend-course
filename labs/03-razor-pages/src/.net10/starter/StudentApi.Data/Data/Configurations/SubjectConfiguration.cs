using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentApi.Data.Entities;

namespace StudentApi.Data.Data.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        // Первичный ключ
        builder.HasKey(s => s.Id);

        // Свойства
        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(150);
            
        // Связь с StudentGroup здесь повторно не настраиваем, 
        // EF Core уже знает о ней из конфигурации группы.
    }
}