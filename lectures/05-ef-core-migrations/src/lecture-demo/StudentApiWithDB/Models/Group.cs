namespace StudentApiWithDB.Models;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Навигационное свойство: у одной группы много студентов
    public ICollection<Student> Students { get; set; } = new List<Student>();
}