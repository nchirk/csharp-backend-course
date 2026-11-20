namespace StudentApi.Data.Entities;

public class StudyProgram
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    
    // Навигационное свойство 1:N: у одной специальности много групп
    public ICollection<Group> Groups { get; set; } = new List<Group>();
}