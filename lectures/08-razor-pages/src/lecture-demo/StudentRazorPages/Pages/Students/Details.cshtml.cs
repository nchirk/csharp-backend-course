using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentApi.Data.DTOs.Student;
using StudentApi.Data.Services;

namespace StudentRazorPages.Pages.Students;

public class Details : PageModel
{
    private readonly IStudentService _studentService;

    public Details(IStudentService studentService)
    {
        _studentService = studentService;
    }

    public StudentGetDto Student { get; set; } = new StudentGetDto();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var student = await _studentService.GetByIdAsync(1);
        if (student is null)
        {
            return NotFound();
        }

        Student = student;
        return Page();
    }
}