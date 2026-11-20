using StudentApi.Data.DTOs.Student;

namespace StudentApi.Data.DTOs.Group;

public class GroupGetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<StudentShortDto> Students { get; set; } = new List<StudentShortDto>();
}