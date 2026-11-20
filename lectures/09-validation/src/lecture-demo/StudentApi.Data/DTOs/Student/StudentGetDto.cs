namespace StudentApi.Data.DTOs.Student;

public class StudentGetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? GroupName { get; set; }
}