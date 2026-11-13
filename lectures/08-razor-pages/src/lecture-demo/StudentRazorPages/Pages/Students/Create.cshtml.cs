using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentApi.Data.DTOs.Student;
using StudentApi.Data.Services;

namespace StudentRazorPages.Pages.Students;

public class Create : PageModel
{
    private readonly IStudentService _studentService;

    public Create(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [BindProperty]
    public StudentCreateDto Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _studentService.CreateAsync(Input);
        return RedirectToPage("./Index"); // Шаг Redirect паттерна PRG
    }
}