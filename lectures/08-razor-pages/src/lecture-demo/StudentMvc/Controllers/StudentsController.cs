using Microsoft.AspNetCore.Mvc;
using StudentApi.Data.Services;
using StudentMvc.Models.Students;

namespace StudentMvc.Controllers;

// Наследуемся от Controller (для страниц), а не ControllerBase (как в API)
public class StudentsController : Controller
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    // Маршрут по умолчанию: /Student или /Student/Index
    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        // 1. Запрашиваем чистые DTO из нашей бизнес-логики
        var studentsDto = await _studentService.GetAllAsync(search);

        // 2. Превращаем DTO во ViewModel, адаптируя под экран
        var viewModel = new StudentListViewModel
        {
            Students = studentsDto.Select(s => new StudentItemViewModel
            {
                Id = s.Id,
                DisplayName = $"{s.Name} ({s.Age} лет)"
            })
        };

        // 3. Передаем модель в шаблон. Метод View() автоматически найдет нужный .cshtml файл
        return View(viewModel);
    }
}
