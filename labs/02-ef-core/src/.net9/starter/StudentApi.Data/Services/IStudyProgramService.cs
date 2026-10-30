using StudentApi.Data.DTOs.StudyProgram;

namespace StudentApi.Data.Services;

public interface IStudyProgramService
{
    Task<IEnumerable<StudyProgramGetDto>> GetAllAsync(string? search);
    Task<StudyProgramGetDto> CreateAsync(StudyProgramCreateDto dto);
}