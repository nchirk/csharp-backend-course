using System.ComponentModel.DataAnnotations;

namespace StudentApi.Data.DTOs.Student;

public class StudentCreateDto
{
    [Required(ErrorMessage = "Имя студента обязательно")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Для имени от 1 до 50 символов")]
    public string Name { get; set; } = string.Empty;
    
    [Range(1, 150, ErrorMessage = "Задайте возраст в диапазоне от 1 до 150")]
    public int Age { get; set; }
    public int? GroupId { get; set; }
}