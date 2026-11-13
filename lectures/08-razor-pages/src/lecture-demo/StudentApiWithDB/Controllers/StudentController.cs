using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentApi.Data.Data;
using StudentApi.Data.DTOs.Student;
using StudentApi.Data.Entities;
using StudentApi.Data.Services;

namespace StudentApiWithDB.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly IStudentService studentService;

    public StudentController(IStudentService studentService)
    {
        this.studentService = studentService;
    }
    
    // 1. GET ALL / SEARCH (Async)
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search)
    {
        return Ok(await studentService.GetAllAsync(search));
    }
    
    // 2. GET BY ID (Async)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var student = await studentService.GetByIdAsync(id);
        if (student == null)
        {
            return NotFound($"Группа с ID {id} не найдена.");
        }
    
        return Ok(student);
    }
    
    // 3. POST (Async)
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] StudentCreateDto dto)  
    {        
        if (string.IsNullOrWhiteSpace(dto.Name))  
        {            
            return BadRequest("Имя студента обязательно.");  
        }
        var result =  await studentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), result);
    }

    // 4. PUT  
    [HttpPut("{id:int}")]  
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] StudentUpdateDto dto)  
    {        
        if (string.IsNullOrWhiteSpace(dto.Name))  
        {            
            return BadRequest("Поле Name обязательно.");  
        }
        
        var success = await studentService.UpdateAsync(id, dto);
        if (success) { return NoContent(); }
        else { return NotFound($"Студент с ID {id} не найден."); }
    }  
    
    // 5. PATCH (Async)
    [HttpPatch("{id:int}/group")]
    public async Task<IActionResult> UpdateGroup([FromRoute] int id, [FromBody] int newGroupId)
    {
        var success = await studentService.UpdateGroupAsync(id, newGroupId);
        if (success) { return NoContent(); }
        else { return NotFound($"Студент с ID {id} не найден."); }
    }

    // 5. DELETE
    [HttpDelete("{id:int}")]  
    public async Task<IActionResult> Delete([FromRoute] int id)  
    {        
        var success = await studentService.DeleteAsync(id);
        if (success) { return Ok($"Группа с ID {id} успешно удалена."); }
        else { return NotFound($"Группа с ID {id} не найдена."); }
    }
}