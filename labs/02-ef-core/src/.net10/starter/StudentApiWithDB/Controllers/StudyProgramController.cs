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
    
    // GET ALL / SEARCH (Async)
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search)
    {
        return Ok(await service.GetAllAsync(search));
    }
    
    // POST (Async)
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
}
