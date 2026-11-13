using Microsoft.EntityFrameworkCore;
using StudentApi.Data.Data;
using StudentApi.Data.DTOs.Student;
using StudentApi.Data.Entities;

namespace StudentApi.Data.Services;

public class StudentService : IStudentService
{
    private readonly AppDbContext context;

    public StudentService(AppDbContext context)
    {
        this.context = context;
    }
    
    public async Task<IEnumerable<StudentGetDto>> GetAllAsync(string? search)
    {
        var studentsQuery = context.Students
            .Include(s => s.Group)
            .AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            studentsQuery = studentsQuery.Where(s => s.Name.ToLower().Contains(search.ToLower()));
        }

        var students = await studentsQuery.Select(s => new StudentGetDto()
        {
            Id = s.Id,
            Name = s.Name,
            Age = s.Age,
            GroupName = s.Group == null? null : s.Group.Name
        }).ToListAsync();
        return students;
    }

    public async Task<StudentGetDto?> GetByIdAsync(int id)
    {
        // EF Core сам скомпилирует это в SQL: SELECT ... WHERE Id = id LIMIT 1
        var student = await context.Students
            .Include(s => s.Group)
            .Select(s => new StudentGetDto(){
                Id = s.Id,
                Name = s.Name,
                Age = s.Age,
                GroupName = s.Group == null? null : s.Group.Name
            }).FirstOrDefaultAsync(s => s.Id == id);

        return student;
    }

    public async Task<StudentGetDto> CreateAsync(StudentCreateDto dto)
    {
        // Маппинг DTO -> Entity
        var newStudent = new Student 
        {
            Name = dto.Name,
            Age = dto.Age,
            GroupId = dto.GroupId
        };

        await context.Students.AddAsync(newStudent);  
        await context.SaveChangesAsync();

        var result = await GetByIdAsync(newStudent.Id);
        return result!; 
    }

    public async Task<bool> UpdateAsync(int id, StudentUpdateDto dto)
    {
        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == id);  
        if (student == null) { return false; }  
        
        student.Name = dto.Name;  
        student.Age = dto.Age;
        student.GroupId = dto.GroupId;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateGroupAsync(int id, int newGroupId)
    {
        bool groupExists = await context.Groups.AnyAsync(g => g.Id == newGroupId);
        if  (!groupExists) { return false; }
        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == id);  
        if (student == null) { return false; }  
        
        student.GroupId = newGroupId;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == id);  
        if (student == null) { return false; }  
        
        context.Students.Remove(student);  
        await context.SaveChangesAsync();
        return true;
    }
}