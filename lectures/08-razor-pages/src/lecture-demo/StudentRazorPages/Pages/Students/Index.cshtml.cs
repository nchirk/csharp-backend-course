using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentApi.Data.DTOs.Student;
using StudentApi.Data.Services;

namespace StudentRazorPages.Pages.Students;

public class Index : PageModel
{
    private readonly IStudentService _studentService;

    public Index(IStudentService studentService)
    {
        _studentService = studentService;
    }
    
    [BindProperty(SupportsGet = true)] // Разрешаем привязку из Query-строки (?SearchTerm=Mike)
    public string? SearchTerm { get; set; }

    // Свойства, в которые мы запишем данные. 
    // Страница .cshtml сможет обратиться к ним напрямую через @Model.Students и @Model.TotalCountMessage
    public IEnumerable<StudentGetDto> Students { get; set; } = [];
    public string TotalCountMessage => $"Всего студентов в базе: {Students.Count()}";

    // Обработчик HTTP GET запроса
    public async Task OnGetAsync()
    {
        // Напрямую берем данные из сервиса. 
        Students = await _studentService.GetAllAsync(SearchTerm);
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(int id) {
        await _studentService.DeleteAsync(id);
        return RedirectToPage(); // Перезагружаем текущую страницу через GET (PRG)
    }
}