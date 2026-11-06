using Microsoft.AspNetCore.Mvc;
using StudentApi.Data.DTOs.StudyProgram;
using StudentApi.Data.Services;

namespace StudentApiWithDB.Controllers;

[ApiController]
[Route("api/study-program")]
public class StudyProgramController : ControllerBase
{
    private readonly IStudyProgramService service;
    public StudyProgramController(IStudyProgramService service)
    {
        this.service = service;
    }
    
    // 1. GET ALL / SEARCH (Async)
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search)
    {
        return Ok(await service.GetAllAsync(search));
    }
    
    // 2. GET BY ID (Async)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var group = await service.GetByIdAsync(id);
        if (group == null)
        {
            return NotFound($"Группа с ID {id} не найдена.");
        }
    
        return Ok(group);
    }
    
    // 3. POST (Async)
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] StudyProgramCreateDto dto)  
    {        
        if (string.IsNullOrWhiteSpace(dto.Name))  
        {            
            return BadRequest("Имя группы обязательно.");  
        }
        var result =  await service.CreateAsync(dto);
        return NoContent();
    }
    
    // 4. PUT  
    [HttpPut("{id:int}")]  
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] StudyProgramUpdateDto dto)  
    {        
        if (string.IsNullOrWhiteSpace(dto.Name))  
        {            
            return BadRequest("Поле Name обязательно.");  
        }
        
        var success = await service.UpdateAsync(id, dto);
        if (success) { return NoContent(); }
        else { return NotFound($"Специальность с ID {id} не найдена."); }
    }  

    // 5. DELETE
    [HttpDelete("{id:int}")]  
    public async Task<IActionResult> Delete([FromRoute] int id)  
    {        
        var success = await service.DeleteAsync(id);
        if (success) { return Ok($"Специальность с ID {id} успешно удалена."); }
        else { return NotFound($"Специальность с ID {id} не найдена."); }
    }
}