namespace StudentApiWithDB.Models;

public class Student
{
    // EF Core автоматически распознает свойство Id (или StudentId) как Primary Key (Первичный ключ)
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
}