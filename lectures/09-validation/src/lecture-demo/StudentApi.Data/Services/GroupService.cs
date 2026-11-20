using Microsoft.EntityFrameworkCore;
using StudentApi.Data.Data;
using StudentApi.Data.DTOs;
using StudentApi.Data.DTOs.Group;
using StudentApi.Data.DTOs.Student;
using StudentApi.Data.Entities;

namespace StudentApi.Data.Services;

public class GroupService : IGroupService
{
    private readonly AppDbContext context;

    public GroupService(AppDbContext context)
    {
        this.context = context;
    }
    
    public async Task<IEnumerable<GroupGetDto>> GetAllAsync(string? search)
    {
        var groupsQuery = context.Groups
            .Include(g => g.Students)
            .AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            groupsQuery = groupsQuery.Where(s => s.Name.ToLower().Contains(search.ToLower()));
        }

        var groups = await groupsQuery.Select(g => new GroupGetDto()
        {
            Id = g.Id,
            Name = g.Name,
            Students = g.Students.Select(s => new StudentShortDto()
            {
                Id = s.Id,
                Name = s.Name
            }).ToList()
        }).ToListAsync();
        return groups;
    }

    public async Task<GroupGetDto?> GetByIdAsync(int id)
    {
        // EF Core сам скомпилирует это в SQL: SELECT ... WHERE Id = id LIMIT 1
        var group = await context.Groups.Select(g => new GroupGetDto()
        {
            Id = g.Id,
            Name = g.Name,
            Students = g.Students.Select(s => new StudentShortDto()
            {
                Id = s.Id,
                Name = s.Name
            }).ToList()
        }).FirstOrDefaultAsync(s => s.Id == id);

        return group;
    }

    public async Task<GroupGetDto> CreateAsync(GroupCreateDto dto)
    {
        // Маппинг DTO -> Entity
        var newGroup = new Group 
        {
            Name = dto.Name
        };

        await context.Groups.AddAsync(newGroup);  
        await context.SaveChangesAsync();  
        
        var resultDto = new GroupGetDto { Id = newGroup.Id, Name = newGroup.Name };

        return resultDto;
    }

    public async Task<bool> UpdateAsync(int id, GroupUpdateDto dto)
    {
        var group = await context.Groups.FirstOrDefaultAsync(s => s.Id == id);  
        if (group == null) { return false; }  
        
        group.Name = dto.Name;  
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var group = await context.Groups.FirstOrDefaultAsync(s => s.Id == id);  
        if (group == null) { return false; }  
        
        context.Groups.Remove(group);  
        await context.SaveChangesAsync();
        return true;
    }
}