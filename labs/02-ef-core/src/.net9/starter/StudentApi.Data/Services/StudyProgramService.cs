using StudentApi.Data.DTOs.StudyProgram;

namespace StudentApi.Data.Services;

public class StudyProgramService : IStudyProgramService
{
    public async Task<IEnumerable<StudyProgramGetDto>> GetAllAsync(string? search)
    {
        throw new NotImplementedException();
    }

    public async Task<StudyProgramGetDto> CreateAsync(StudyProgramCreateDto dto)
    {
        throw new NotImplementedException();
    }
}