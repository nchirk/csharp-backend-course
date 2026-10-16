namespace StudentApiWithDB.Models;

public class Student
{
    // EF Core автоматически распознает свойство Id (или StudentId) как Primary Key (Первичный ключ)
    public int Id { get; set; }
    public int Age { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Связь 1:N (Студент принадлежит группе)
    public int? GroupId { get; set; }
    
    // Навигационное свойство
    public Group? Group { get; set; }  
}