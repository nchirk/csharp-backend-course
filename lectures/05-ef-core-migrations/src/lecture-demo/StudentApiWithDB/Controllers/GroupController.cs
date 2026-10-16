using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentApiWithDB.Data;
using StudentApiWithDB.Models;

namespace GroupApiWithDB.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupController : ControllerBase
{
    private readonly AppDbContext context;
    
    public GroupController(AppDbContext dbContext)
    {
        context = dbContext;
    }
    
    // 1. GET ALL / SEARCH (Async)
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            // Используем асинхронную версию ToListAsync()
            return Ok(await context.Groups.ToListAsync());
        }
        
        var filtered = await context.Groups
            .Where(s => s.Name.ToLower().Contains(search.ToLower()))
            .ToListAsync();
    
        return Ok(filtered);
    }
    
    // 2. GET BY ID (Async)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        // EF Core сам скомпилирует это в SQL: SELECT ... WHERE Id = id LIMIT 1
        var group = await context.Groups.FirstOrDefaultAsync(s => s.Id == id);
        
        if (group == null)
        {
            return NotFound($"Группа с ID {id} не найдена.");
        }
    
        return Ok(group);
    }
    
    // 3. POST (Async)
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Group newGroup)
    {
        if (string.IsNullOrWhiteSpace(newGroup.Name))
        {
            return BadRequest("Имя группы обязательно.");
        }
        
        // ВАЖНО: Мы НЕ высчитываем ID вручную! 
        // БД SQLite сама сгенерирует автоинкрементный ID при сохранении.
        await context.Groups.AddAsync(newGroup);
        
        // Фиксируем изменения в БД физически
        await context.SaveChangesAsync();
    
        return CreatedAtAction(nameof(GetById), new { id = newGroup.Id }, newGroup);
    }
    
    // 4. PUT (Async)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] Group updatedGroup)
    {
        if (string.IsNullOrWhiteSpace(updatedGroup.Name) )
        {
            return BadRequest("Поле Name обязательно.");
        }
        
        var group = await context.Groups.FirstOrDefaultAsync(s => s.Id == id);

        if (group == null)
        {
            return NotFound($"Студент с ID {id} не найден.");
        }

        // Обновляем свойства найденного объекта, EF Core отследит эти изменения
        group.Name = updatedGroup.Name;

        await context.SaveChangesAsync();
        return NoContent();
    }

    // 6. DELETE (Async)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var group = await context.Groups.FirstOrDefaultAsync(s => s.Id == id);

        if (group == null)
        {
            return NotFound($"Студент с ID {id} не найден.");
        }

        // Удаляем объект из DbSet
        context.Groups.Remove(group);
        
        await context.SaveChangesAsync();
        return Ok($"Студент с ID {id} успешно удален.");
    }
}