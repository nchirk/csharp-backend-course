namespace StudentApi.Data.DTOs.Student;

public class StudentCreateDto
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public int? GroupId { get; set; }
}