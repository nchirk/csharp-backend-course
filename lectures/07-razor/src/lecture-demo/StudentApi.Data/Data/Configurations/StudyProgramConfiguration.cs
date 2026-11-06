using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentApi.Data.Entities;

namespace StudentApi.Data.Data.Configurations;

public class StudyProgramConfiguration : IEntityTypeConfiguration<StudyProgram>
{
    public void Configure(EntityTypeBuilder<StudyProgram> builder)
    {
        // Установка первичного ключа
        builder.HasKey(g => g.Id);

        // Имя группы обязательно для заполнения и не должно превышать 20 символов
        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(20);

        // Создание уникального индекса: в базе данных не может быть двух групп с одинаковым именем
        builder.HasIndex(g => g.Name)
            .IsUnique();
    }
}