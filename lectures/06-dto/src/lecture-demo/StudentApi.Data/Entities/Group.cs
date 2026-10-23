namespace StudentApi.Data.Entities;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Навигационное свойство 1:N: у одной группы много студентов
    public ICollection<Student> Students { get; set; } = new List<Student>();
    
    //Навигационное свойство N:M: много групп изучают много предметов
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}