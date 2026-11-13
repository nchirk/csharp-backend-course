using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentApi.Data.DTOs.Student;
using StudentApi.Data.Services;

namespace StudentRazorPages.Pages.Students;

public class Update : PageModel  
{  
    private readonly IStudentService _studentService;

    public Update(IStudentService studentService)  
    {  
        _studentService = studentService;  
    }

    [BindProperty]  
    public StudentUpdateDto Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)  
    {  
        var student = await _studentService.GetByIdAsync(id);  
        if (student is null)  
        {  
            return NotFound();  
        }

// Заполняем форму текущими данными студента  
        Input = new StudentUpdateDto  
        {  
            Name = student.Name
        };

        return Page();  
    }

    public async Task<IActionResult> OnPostAsync(int id)  
    {
        var updated = await _studentService.UpdateAsync(id, Input);  
        if (!updated)  
        {  
            return NotFound();  
        }

        return RedirectToPage("./Index");  
    }  
} 