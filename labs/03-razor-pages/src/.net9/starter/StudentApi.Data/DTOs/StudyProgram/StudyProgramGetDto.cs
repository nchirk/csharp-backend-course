using StudentApi.Data.DTOs.Group;

namespace StudentApi.Data.DTOs.StudyProgram;

public class StudyProgramGetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<GroupShortDto> Groups { get; set; } = new List<GroupShortDto>();
}