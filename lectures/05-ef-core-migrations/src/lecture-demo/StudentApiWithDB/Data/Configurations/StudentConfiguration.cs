namespace StudentApiWithDB.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentApiWithDB.Models;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        // Установка первичного ключа (необязательно, если имя Id, но полезно для явности)
        builder.HasKey(s => s.Id);

        // Установка обязательных полей и ограничений длины строки
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(50);

        // Явное указание связи один-ко-многим (хотя EF Core понимает её и по конвенциям)
        builder.HasOne(s => s.Group)
            .WithMany(g => g.Students)
            .HasForeignKey(s => s.GroupId)
            .OnDelete(DeleteBehavior.SetNull); // При удалении группы студенты остаются, но их GroupId зануляется
    }
}