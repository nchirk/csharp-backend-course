using StudentApi.Data.DTOs.Student;

namespace StudentApi.Data.Services;

public interface IStudentService
{
    Task<IEnumerable<StudentGetDto>> GetAllAsync(string? search);
    Task<StudentGetDto?> GetByIdAsync(int id);
    Task<StudentGetDto> CreateAsync(StudentCreateDto dto);
    Task<bool> UpdateAsync(int id, StudentUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateGroupAsync(int id, int newGroupId);
}