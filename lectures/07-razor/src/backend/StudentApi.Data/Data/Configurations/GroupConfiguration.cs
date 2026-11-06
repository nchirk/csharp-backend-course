using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentApi.Data.Entities;

namespace StudentApi.Data.Data.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
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
        
        // Настройка чистой связи "многие ко многим"
        builder.HasMany(g => g.Subjects)
            .WithMany(s => s.Groups);
        
        // Явное указание связи один-ко-многим (хотя EF Core понимает её и по конвенциям)
        builder.HasOne(s => s.StudyProgram)
            .WithMany(g => g.Groups)
            .HasForeignKey(s => s.StudyProgramId)
            .OnDelete(DeleteBehavior.SetNull); // При удалении группы студенты остаются, но их GroupId зануляется
    }
}