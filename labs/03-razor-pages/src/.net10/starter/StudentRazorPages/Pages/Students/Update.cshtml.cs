using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentApi.Data.DTOs.Student;
using StudentApi.Data.Services;

namespace StudentRazorPages.Pages.Students;

public class Update : PageModel  
{  
    private readonly IStudentService _studentService;
    private readonly IGroupService _groupService;

    public Update(IStudentService studentService, IGroupService groupService)
    {
        _studentService = studentService;
        _groupService = groupService;
    }

    [BindProperty]  
    public StudentUpdateDto Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)  
    {  
        var student = await _studentService.GetByIdForUpdateAsync(id);  
        if (student is null)  
        {  
            return NotFound();  
        }

        await LoadGroupsAsync();
        Input = student;

        return Page();  
    }

    public async Task<IActionResult> OnPostAsync(int id)  
    {
        if (!ModelState.IsValid)
        {
            return await PageWithGroupsAsync();
        }
        var updated = await _studentService.UpdateAsync(id, Input);  
        if (!updated)  
        {  
            ModelState.AddModelError("", "Не удалось обновить студента, возможно он был удален.");
            return await PageWithGroupsAsync();
        }

        return RedirectToPage("./Index");  
    }

    private async Task<IActionResult> PageWithGroupsAsync()
    {
        await LoadGroupsAsync();
        return Page();
    }

    public IEnumerable<SelectListItem> GroupList { get; set; } = [];
    private async Task LoadGroupsAsync()
    {
        var groups = await _groupService.GetAllAsync(null);
        GroupList = groups.Select(x => new SelectListItem(x.Name, x.Id.ToString()));
    }
} 