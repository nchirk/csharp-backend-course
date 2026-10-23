using StudentApi.Data.DTOs;
using StudentApi.Data.DTOs.Group;

namespace StudentApi.Data.Services;

public interface IGroupService
{
    Task<IEnumerable<GroupGetDto>> GetAllAsync(string? search);
    Task<GroupGetDto?> GetByIdAsync(int id);
    Task<GroupGetDto> CreateAsync(GroupCreateDto dto);
    Task<bool> UpdateAsync(int id, GroupUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}