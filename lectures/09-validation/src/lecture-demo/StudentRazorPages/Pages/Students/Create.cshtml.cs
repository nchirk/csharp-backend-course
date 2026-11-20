using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentApi.Data.DTOs.Student;
using StudentApi.Data.Services;

namespace StudentRazorPages.Pages.Students;

public class Create : PageModel
{
    private readonly IStudentService _studentService;
    private readonly IGroupService _groupService;

    public Create(IStudentService studentService, IGroupService groupService)
    {
        _studentService = studentService;
        _groupService = groupService;
    }

    [BindProperty]
    public StudentCreateDto Input { get; set; } = new();
    
    public async Task<IActionResult> OnGet()
    {
        await LoadGroupsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Подключаем серверную валидацию
        if (!ModelState.IsValid)
        {
            await LoadGroupsAsync();
            return Page();
        }
        await _studentService.CreateAsync(Input);
        return RedirectToPage("./Index"); // Шаг Redirect паттерна PRG
    }
    
    public IEnumerable<SelectListItem> GroupList { get; set; } = [];
    private async Task LoadGroupsAsync()
    {
        var groups = await _groupService.GetAllAsync(null);
        GroupList = groups.Select(x => new SelectListItem(x.Name, x.Id.ToString()));
    }
}