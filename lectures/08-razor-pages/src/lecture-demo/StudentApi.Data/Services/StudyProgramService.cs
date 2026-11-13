using Microsoft.EntityFrameworkCore;
using StudentApi.Data.Data;
using StudentApi.Data.DTOs.Group;
using StudentApi.Data.DTOs.StudyProgram;
using StudentApi.Data.Entities;

namespace StudentApi.Data.Services;

public class StudyProgramService : IStudyProgramService
{
    private readonly AppDbContext context;

    public StudyProgramService(AppDbContext context)
    {
        this.context = context;
    }
    
    public async Task<IEnumerable<StudyProgramGetDto>> GetAllAsync(string? search)
    {
        var studyProgramsQuery = context.StudyPrograms
            .Include(sp => sp.Groups)
            .AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            studyProgramsQuery = studyProgramsQuery.Where(s => s.Name.ToLower().Contains(search.ToLower()));
        }

        var studyPrograms = await studyProgramsQuery.Select(sp => new StudyProgramGetDto()
        {
            Id = sp.Id,
            Name = sp.Name,
            Groups = sp.Groups.Select(s => new GroupShortDto()
            {
                Id = s.Id,
                Name = s.Name
            }).ToList()
        }).ToListAsync();
        return studyPrograms;
    }

    public async Task<StudyProgramGetDto?> GetByIdAsync(int id)
    {
        // EF Core сам скомпилирует это в SQL: SELECT ... WHERE Id = id LIMIT 1
        var group = await context.StudyPrograms.Select(sp => new StudyProgramGetDto()
        {
            Id = sp.Id,
            Name = sp.Name,
            Groups = sp.Groups.Select(s => new GroupShortDto()
            {
                Id = s.Id,
                Name = s.Name
            }).ToList()
        }).FirstOrDefaultAsync(s => s.Id == id);

        return group;
    }

    public async Task<StudyProgramGetDto> CreateAsync(StudyProgramCreateDto dto)
    {
        // Маппинг DTO -> Entity
        var newStudyProgram = new StudyProgram 
        {
            Name = dto.Name
        };

        await context.StudyPrograms.AddAsync(newStudyProgram);  
        await context.SaveChangesAsync();  
        
        var resultDto = new StudyProgramGetDto { Id = newStudyProgram.Id, Name = newStudyProgram.Name };

        return resultDto;
    }

    public async Task<bool> UpdateAsync(int id, StudyProgramUpdateDto dto)
    {
        var group = await context.StudyPrograms.FirstOrDefaultAsync(s => s.Id == id);  
        if (group == null) { return false; }  
        
        group.Name = dto.Name;  
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var group = await context.StudyPrograms.FirstOrDefaultAsync(s => s.Id == id);  
        if (group == null) { return false; }  
        
        context.StudyPrograms.Remove(group);  
        await context.SaveChangesAsync();
        return true;
    }
}