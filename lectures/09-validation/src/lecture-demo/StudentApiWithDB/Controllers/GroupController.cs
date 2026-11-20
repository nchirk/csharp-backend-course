using Microsoft.AspNetCore.Mvc;
using StudentApi.Data.DTOs.Group;
using StudentApi.Data.Services;

namespace StudentApiWithDB.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupController : ControllerBase
{
    private readonly IGroupService groupService;
    
    public GroupController(IGroupService groupService)
    {
        this.groupService = groupService;
    }
    
    // 1. GET ALL / SEARCH (Async)
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search)
    {
        return Ok(await groupService.GetAllAsync(search));
    }
    
    // 2. GET BY ID (Async)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var group = await groupService.GetByIdAsync(id);
        if (group == null)
        {
            return NotFound($"Группа с ID {id} не найдена.");
        }
    
        return Ok(group);
    }
    
    // 3. POST (Async)
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] GroupCreateDto dto)  
    {        
        if (string.IsNullOrWhiteSpace(dto.Name))  
        {            
            return BadRequest("Имя группы обязательно.");  
        }
        var result =  await groupService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), result);
    }

    // 4. PUT  
    [HttpPut("{id:int}")]  
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] GroupUpdateDto dto)  
    {        
        if (string.IsNullOrWhiteSpace(dto.Name))  
        {            
            return BadRequest("Поле Name обязательно.");  
        }
        
        var success = await groupService.UpdateAsync(id, dto);
        if (success) { return NoContent(); }
        else { return NotFound($"Группа с ID {id} не найдена."); }
    }  

    // 5. DELETE
    [HttpDelete("{id:int}")]  
    public async Task<IActionResult> Delete([FromRoute] int id)  
    {        
        var success = await groupService.DeleteAsync(id);
        if (success) { return Ok($"Группа с ID {id} успешно удалена."); }
        else { return NotFound($"Группа с ID {id} не найдена."); }
    }
}