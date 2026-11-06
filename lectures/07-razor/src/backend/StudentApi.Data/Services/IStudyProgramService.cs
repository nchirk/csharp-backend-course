using StudentApi.Data.DTOs.StudyProgram;

namespace StudentApi.Data.Services;

public interface IStudyProgramService
{
    Task<IEnumerable<StudyProgramGetDto>> GetAllAsync(string? search);
    Task<StudyProgramGetDto?> GetByIdAsync(int id);
    Task<StudyProgramGetDto> CreateAsync(StudyProgramCreateDto dto);
    Task<bool> UpdateAsync(int id, StudyProgramUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}