using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentApiWithDB.Data;
using StudentApiWithDB.Models;

namespace StudentApiWithDB.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly AppDbContext context;
    
    public StudentController(AppDbContext context)
    {
        this.context = context;
    }
    
    // 1. GET ALL / SEARCH (Async)
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            // Используем асинхронную версию ToListAsync()
            return Ok(await context.Students.ToListAsync());
        }
        
        var filtered = await context.Students
            .Where(s => s.Name.ToLower().Contains(search.ToLower()))
            .ToListAsync();

        return Ok(filtered);
    }

    // 2. GET BY ID (Async)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        // EF Core сам скомпилирует это в SQL: SELECT ... WHERE Id = id LIMIT 1
        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == id);
        
        if (student == null)
        {
            return NotFound($"Студент с ID {id} не найден.");
        }

        return Ok(student);
    }

    // 3. POST (Async)
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Student newStudent)
    {
        if (string.IsNullOrWhiteSpace(newStudent.Name))
        {
            return BadRequest("Имя обязательно.");
        }
        
        // ВАЖНО: Мы НЕ высчитываем ID вручную! 
        // БД SQLite сама сгенерирует автоинкрементный ID при сохранении.
        await context.Students.AddAsync(newStudent);
        
        // Фиксируем изменения в БД физически
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = newStudent.Id }, newStudent);
    }

    // 4. PUT (Async)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] Student updatedStudent)
    {
        if (string.IsNullOrWhiteSpace(updatedStudent.Name) )
        {
            return BadRequest("Поле Name обязательно.");
        }
        
        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
        {
            return NotFound($"Студент с ID {id} не найден.");
        }

        // Обновляем свойства найденного объекта, EF Core отследит эти изменения
        student.Name = updatedStudent.Name;
        student.Group = updatedStudent.Group;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // 5. PATCH (Async)
    [HttpPatch("{id:int}/group")]
    public async Task<IActionResult> UpdateGroup([FromRoute] int id, [FromBody] int newGroupId)
    {
        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
        {
            return NotFound($"Студент с ID {id} не найден.");
        }
        
        student.GroupId = newGroupId;
        
        await context.SaveChangesAsync();
        return Ok(student);
    }

    // 6. DELETE (Async)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var student = await context.Students.FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
        {
            return NotFound($"Студент с ID {id} не найден.");
        }

        // Удаляем объект из DbSet
        context.Students.Remove(student);
        
        await context.SaveChangesAsync();
        return Ok($"Студент с ID {id} успешно удален.");
    }
}