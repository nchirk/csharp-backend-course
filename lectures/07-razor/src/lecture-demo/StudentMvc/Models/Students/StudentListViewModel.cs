namespace StudentMvc.Models.Students;

public class StudentListViewModel
{
    public IEnumerable<StudentItemViewModel> Students { get; set; } = [];
    public string TotalCountMessage => $"Всего студентов в базе: {Students.Count()}";
}