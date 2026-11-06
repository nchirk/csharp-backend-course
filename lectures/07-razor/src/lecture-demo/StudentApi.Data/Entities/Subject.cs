namespace StudentApi.Data.Entities;

public class Subject
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;

    // Навигационное свойство N:M: много групп изучают много предметов
    public ICollection<Group> Groups { get; set; } = new List<Group>();
}