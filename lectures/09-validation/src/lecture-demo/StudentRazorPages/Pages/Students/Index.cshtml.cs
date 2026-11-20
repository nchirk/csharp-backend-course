using System.ComponentModel.DataAnnotations;
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
    [StringLength(200, ErrorMessage = "Поисковый запрос не должен превышать 200 символов")]
    public string? SearchTerm { get; set; }

    [TempData] public string? DeleteErrorMessage { get; set; }
    
    // Свойства, в которые мы запишем данные. 
    // Страница .cshtml сможет обратиться к ним напрямую через @Model.Students и @Model.TotalCountMessage
    public IEnumerable<StudentGetDto> Students { get; set; } = [];
    public string TotalCountMessage => $"Всего студентов в базе: {Students.Count()}";

    // Обработчик HTTP GET запроса
    public async Task<IActionResult> OnGetAsync()
    {
        if (!ModelState.IsValid)
        {
            Students = [];
            return Page();
        }
        // Напрямую берем данные из сервиса. 
        Students = await _studentService.GetAllAsync(SearchTerm);
        return Page();
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(int id) {
        var success = await _studentService.DeleteAsync(id);
        if (!success)
        {
            DeleteErrorMessage = "Не удалось удалить студента, возможно он был уже удален.";
        }
        return RedirectToPage(); // Перезагружаем текущую страницу через GET (PRG)
    }
}