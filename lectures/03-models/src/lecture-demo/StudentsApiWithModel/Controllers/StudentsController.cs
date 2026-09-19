using Microsoft.AspNetCore.Mvc;
using StudentsApiWithModel.Models;

namespace StudentsApiWithModel.Controllers;

[ApiController]
[Route("api/[controller]")] // Автоматически преобразуется в api/students
public class StudentsController : ControllerBase
{
    // Статический список в оперативной памяти — временная имитация базы данных.
    // Инициализируем начальными данными с уникальными ID.
    private static readonly List<Student> Students = new()
    {
        new Student { Id = 1, Name = "Иван Иванов", Group = "ИП-21" },
        new Student { Id = 2, Name = "Мария Сидорова", Group = "ИП-22" },
        new Student { Id = 3, Name = "Алексей Петров", Group = "ИП-21" },
        new Student { Id = 4, Name = "Ирина Козлова", Group = "ИП-23" }
    };

    // 1. GET: Получение всех студентов или фильтрация по строке поиска (Query-параметр)
    // Пример: GET api/students?search=иван
    [HttpGet]
    public IActionResult Get([FromQuery] string? search)
    {
        // Если параметр пустой или отсутствовал в URL — отдаем полный список
        if (string.IsNullOrWhiteSpace(search))
        {
            return Ok(Students); // Статус 200 OK
        }

        // Фильтруем коллекцию по имени на совпадение подстроки (без учета регистра)
        var filtered = Students
            .Where(s => s.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Ok(filtered); // Статус 200 OK с отфильтрованными данными
    }

    // 2. GET: Точечное получение студента по ID (Route-параметр с ограничением типа)
    // Пример: GET api/students/2
    [HttpGet("{id:int}")]
    public IActionResult GetById([FromRoute] int id)
    {
        var student = Students.FirstOrDefault(s => s.Id == id);
        
        // Если элемент не найден — возвращаем ошибку 404 Not Found
        if (student == null)
        {
            return NotFound($"Студент с ID {id} не найден.");
        }

        return Ok(student); // Статус 200 OK
    }

    // 3. POST: Добавление нового студента (Данные считываются из JSON-тела запроса)
    // Пример: POST api/students (в Body передается объект Student без ID)
    [HttpPost]
    public IActionResult Add([FromBody] Student newStudent)
    {
        // Валидация входных данных
        if (string.IsNullOrWhiteSpace(newStudent.Name))
        {
            return BadRequest("Имя студента не может быть пустым."); // Статус 400 Bad Request
        }

        if (string.IsNullOrWhiteSpace(newStudent.Group))
        {
            return BadRequest("Группа студента должна быть указана.");
        }

        // Имитация автоинкремента ID (выбираем максимальный ID и прибавляем 1)
        newStudent.Id = Students.Count > 0 ? Students.Max(s => s.Id) + 1 : 1;
        
        Students.Add(newStudent);

        // Возвращаем статус 201 Created, заголовок Location со ссылкой на GetById и сам объект
        return CreatedAtAction(nameof(GetById), new { id = newStudent.Id }, newStudent);
    }

    // 4. PUT: Полное обновление данных студента (ID из маршрута, данные из тела)
    // Пример: PUT api/students/1
    [HttpPut("{id:int}")]
    public IActionResult Update([FromRoute] int id, [FromBody] Student updatedStudent)
    {
        var index = Students.FindIndex(s => s.Id == id);
        
        if (index == -1)
        {
            return NotFound($"Студент с ID {id} не найден."); // Статус 404 Not Found
        }

        // Дополнительная валидация
        if (string.IsNullOrWhiteSpace(updatedStudent.Name) || string.IsNullOrWhiteSpace(updatedStudent.Group))
        {
            return BadRequest("Поля Name и Group обязательны для заполнения.");
        }

        // Гарантируем, что ID объекта останется неизменным (берется из маршрута)
        updatedStudent.Id = id;
        Students[index] = updatedStudent;

        return NoContent(); // Статус 204 No Content (изменения успешны, тело ответа пустое)
    }

    // 5. PATCH: Частичное обновление — только смена учебной группы (ID из маршрута, строка из тела)
    // Пример: PATCH api/students/1/group
    [HttpPatch("{id:int}/group")]
    public IActionResult UpdateGroup([FromRoute] int id, [FromBody] string newGroup)
    {
        var student = Students.FirstOrDefault(s => s.Id == id);
        
        if (student == null)
        {
            return NotFound($"Студент с ID {id} не найден.");
        }

        if (string.IsNullOrWhiteSpace(newGroup))
        {
            return BadRequest("Название группы не может быть пустым.");
        }

        student.Group = newGroup.Trim();
        return Ok(student); // Статус 200 OK с обновленным объектом
    }

    // 6. DELETE: Удаление студента по ID (ID из маршрута)
    // Пример: DELETE api/students/1
    [HttpDelete("{id:int}")]
    public IActionResult Delete([FromRoute] int id)
    {
        var student = Students.FirstOrDefault(s => s.Id == id);
        
        if (student == null)
        {
            return NotFound($"Студент с ID {id} не найден."); // Статус 404 Not Found
        }

        Students.Remove(student);
        return Ok($"Студент с ID {id} ({student.Name}) успешно удален."); // Статус 200 OK
    }
}
