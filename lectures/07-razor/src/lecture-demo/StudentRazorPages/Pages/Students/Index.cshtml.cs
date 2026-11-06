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

    // Свойства, в которые мы запишем данные. 
    // Страница .cshtml сможет обратиться к ним напрямую через @Model.Students и @Model.TotalCountMessage
    public IEnumerable<string> Students { get; set; } = [];
    public string TotalCountMessage => $"Всего студентов в базе: {Students.Count()}";

    // Обработчик HTTP GET запроса
    public async Task OnGetAsync(string? search)
    {
        // Напрямую берем данные из сервиса. 
        Students = (await _studentService.GetAllAsync(search)).Select(s => $"{s.Name} ({s.Age} лет)");
    }
}