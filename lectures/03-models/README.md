
# Лекция 3. Модели и привязка данных, полный CRUD, настройка маршрутизации

## Перед стартом: Новая модель данных

Прежде чем перейти к лекции, заменяем простую строку на полноценный объект. Создаем файл `Models/Student.cs`:

```csharp
namespace StudentsApi.Models;

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
}
```

---

## 1. Модели и привязка данных (Model Binding)

Model Binding (привязка данных) — это механизм в ASP.NET Core, который автоматически берет данные из HTTP-запроса (из URL, строки запроса, тела или заголовков), преобразует их в типы C# и передает в параметры метода контроллера.

Атрибуты источника данных (Source Attributes):

- `[FromBody]` — извлекает данные из тела запроса (обычно JSON). Идеально для создания/обновления объектов (`POST`, `PUT`).
- `[FromRoute]` — извлекает данные прямо из пути URL (например, `/api/students/5`, где `5` — это ID).
- `[FromQuery]` — извлекает данные из строки запроса после знака вопроса (например, `?search=Иван&page=2`).
- `[FromForm]` — используется для классических HTML-форм или отправки файлов (`multipart/form-data`).
- `[FromHeader]` — извлекает значения из HTTP-заголовков (токенов авторизации, метаданных).

Код контроллера (Обновление GET и POST):  
Переводим коллекцию на модель `Student` и обновляем методы:

```csharp
using Microsoft.AspNetCore.Mvc;
using StudentsApi.Models;

namespace StudentsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    // Временная база данных: теперь со студентами как с объектами
    private static readonly List<Student> Students = new()
    {
        new Student { Id = 1, Name = "Иван Иванов", Group = "ИП-21" },
        new Student { Id = 2, Name = "Мария Сидорова", Group = "ИП-22" },
        new Student { Id = 3, Name = "Алексей Петров", Group = "ИП-21" }
    };

    // GET: api/students?search=иван
    [HttpGet]
    public IActionResult Get([FromQuery] string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return Ok(Students);
        }

        var filtered = Students
            .Where(s => s.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Ok(filtered);
    }

    // POST: api/students
    [HttpPost]
    public IActionResult Add([FromBody] Student newStudent)
    {
        if (string.IsNullOrWhiteSpace(newStudent.Name))
        {
            return BadRequest("Имя студента не может быть пустым.");
        }

        // Автогенерация ID для симуляции БД
        newStudent.Id = Students.Count > 0 ? Students.Max(s => s.Id) + 1 : 1;
        Students.Add(newStudent);

        // Возвращаем статус 201 и ссылку на получение объекта по ID
        return CreatedAtAction(nameof(GetById), new { id = newStudent.Id }, newStudent);
    }

    // GET: api/students/3
    [HttpGet("{id:int}")]
    public IActionResult GetById([FromRoute] int id)
    {
        var student = Students.FirstOrDefault(s => s.Id == id);
        return student == null ? NotFound() : Ok(student);
    }
}
```

Тестирование в `.http` файле:

```http
### Получить всех студентов
GET http://localhost:5000/api/students

### Поиск студента
GET http://localhost:5000/api/students?search=Мария

### Добавить студента
POST http://localhost:5000/api/students
Content-Type: application/json

{
  "name": "Ирина Козлова",
  "group": "ИП-23"
}
```

## ❓ Быстрые вопросы для проверки (5 минут)

1. Откуда атрибут `[FromQuery]` пытается прочитать данные? _(Ответ: Из строки запроса в URL после знака вопроса, например `?name=Ivan`)._
2. Какой атрибут нужно использовать, если клиент отправляет данные в формате JSON в теле запроса? _(Ответ: `[FromBody]`)._
3. Что произойдет, если в метод с `[FromRoute] int id` передать в URL строку "abc"? _(Ответ: Маршрутизатор выдаст ошибку 400 Bad Request или 404 Not Found, так как тип данных не сможет привязаться к int)._

## 2. Полный CRUD: GET, POST, DELETE, PUT, PATCH

CRUD — акроним для четырех базовых функций управления данными: Create (Создание -> `POST`), Read (Чтение -> `GET`), Update (Обновление -> `PUT`/`PATCH`), Delete (Удаление -> `DELETE`). Мы уже работали с `GET`, `POST`, `DELETE` запросами, добавим `PUT` и `PATCH` и получим полноценный CRUD для списка студентов.

_Разница между PUT и PATCH:_

- PUT полностью заменяет существующий ресурс новым объектом. Если какие-то поля не переданы, они затрутся значениями по умолчанию.
- PATCH выполняет частичное обновление (изменение только конкретных полей).

Код контроллера (Добавление PUT, PATCH, DELETE):  
Дописываем методы в `StudentsController`:

```csharp
    // PUT: api/students/1 (Полное обновление)
    [HttpPut("{id:int}")]
    public IActionResult Update([FromRoute] int id, [FromBody] Student updatedStudent)
    {
        var index = Students.FindIndex(s => s.Id == id);
        if (index == -1) return NotFound($"Студент с ID {id} не найден.");

        // Полностью заменяем данные, сохраняя ID
        updatedStudent.Id = id;
        Students[index] = updatedStudent;

        return NoContent(); // Статус 204 (Успешно, контента в ответе нет)
    }

    // PATCH: api/students/1/group (Частичное обновление - только группа)
    [HttpPatch("{id:int}/group")]
    public IActionResult UpdateGroup([FromRoute] int id, [FromBody] string newGroup)
    {
        var student = Students.FirstOrDefault(s => s.Id == id);
        if (student == null) return NotFound($"Студент с ID {id} не найден.");

        if (string.IsNullOrWhiteSpace(newGroup)) return BadRequest("Группа не может быть пустой.");

        student.Group = newGroup;
        return Ok(student);
    }

    // DELETE: api/students/1
    [HttpDelete("{id:int}")]
    public IActionResult Delete([FromRoute] int id)
    {
        var student = Students.FirstOrDefault(s => s.Id == id);
        if (student == null) return NotFound($"Студент с ID {id} не найден.");

        Students.Remove(student);
        return Ok($"Студент с ID {id} успешно удален.");
    }
```

Тестирование в `.http` файле:

```http
### Полное обновление студента (PUT)
PUT http://localhost:5000/api/students/1
Content-Type: application/json

{
  "name": "Иван Обновленный",
  "group": "ИП-25"
}

### Частичное обновление группы (PATCH)
PATCH http://localhost:5000/api/students/1/group
Content-Type: application/json

"ИП-99"

### Удаление студента (DELETE)
DELETE http://localhost:5000/api/students/1
```

## ❓ Быстрые вопросы для проверки (5 минут)

1. В чем разница между PUT и PATCH запросами? _(Ответ: PUT обновляет объект целиком, заменяя его, а PATCH вносит точечные, частичные изменения)._
2. Какой статус ответа сервера возвращает метод `NoContent()`? _(Ответ: HTTP Status 204 — запрос обработан успешно, но серверу не нужно возвращать никакого контента в теле)._
3. Почему удалять ресурс через `GET /api/students/delete?id=1` — это плохая практика? _(Ответ: Нарушается семантика протокола HTTP. GET-запросы должны быть безопасными и только читать данные, а для удаления зарезервирован метод DELETE)._


## 3. Передача параметров

Для проектирования понятного и предсказуемого API крайне важно правильно выбирать способ передачи параметров для каждого HTTP-метода.

### 3.1 Передача параметров в GET-запросах

| Способ передачи                       | Как выглядит в URL          | Когда использовать (Best Practice)                                                                         |
| ------------------------------------- | --------------------------- | ---------------------------------------------------------------------------------------------------------- |
| Параметры маршрута (Route parameters) | `/api/students/4`           | Для точечной идентификации конкретного ресурса (например, по ID). Без этого параметра запрос теряет смысл. |
| Строка запроса (Query parameters)     | `/api/students?search=иван` | Для фильтрации, поиска, сортировки или пагинации. Эти параметры почти всегда являются необязательными.     |

Пример реализации в коде и HTTP:

```csharp
// 1. Поиск/Фильтрация (Query-параметр)
// GET api/students?search=иван
[HttpGet]
public IActionResult Get([FromQuery] string? search)
{
    if (string.IsNullOrWhiteSpace(search)) return Ok(Students);
    var filtered = Students.Where(s => s.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
    return Ok(filtered);
}

// 2. Получение по ID (Route-параметр)
// GET api/students/2
[HttpGet("{id:int}")]
public IActionResult GetById([FromRoute] int id)
{
    var student = Students.FirstOrDefault(s => s.Id == id);
    return student == null ? NotFound() : Ok(student);
}
```

```http
### Запрос 1: Фильтрация по строке поиска (Query)
GET http://localhost:5000/api/students?search=Мария

### Запрос 2: Точечный поиск по ID (Route)
GET http://localhost:5000/api/students/2
```

---

### 3.2 Передача параметров в POST-запросах

| Способ передачи                       | Как выглядит в URL / Запросе                          | Когда использовать (Best Practice)                                                                                                                                                                                                                    |
| ------------------------------------- | ----------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Тело запроса (Request Body)           | Скрыто из URL (передается внутри Body в формате JSON) | Основной способ. Для передачи всей структуры данных создаваемого объекта (например, ФИО и группа нового студента).                                                                                                                                    |
| Параметры маршрута (Route parameters) | `/api/students/2/labs`                              | Для указания целевого или родительского ресурса, внутри которого создается новый объект (в примере: добавление лабороаторной работы именно для студента с ID 2).<br><br>*Примечание: пример предполагает в коде две отдельные модели Students и Labs. Дан обзорно.* |
| Строка запроса (Query parameters)     | `/api/students?archive=false`                         | Используется редко. Подходит для флагов-модификаторов или настроек самого процесса создания (например, не отправлять приветственное письмо).<br><br>*Примечание: примера в коде нет, дан обзорно.*                                                    |

Пример реализации в коде и HTTP:

```csharp
// Добавление нового студента (Тело запроса)
// POST api/students
[HttpPost]
public IActionResult Add([FromBody] Student newStudent)
{
    if (string.IsNullOrWhiteSpace(newStudent.Name)) return BadRequest("Имя не может быть пустым.");
    
    newStudent.Id = Students.Count > 0 ? Students.Max(s => s.Id) + 1 : 1;
    Students.Add(newStudent);

    return CreatedAtAction(nameof(GetById), new { id = newStudent.Id }, newStudent);
}
```

```http
### Создание нового ресурса (Body)
POST http://localhost:5000/api/students
Content-Type: application/json

{
  "name": "Ирина Козлова",
  "group": "ИП-23"
}
```

---

### 3.3 Передача параметров в PUT и PATCH-запросах

| Способ передачи          | Как выглядит в URL / Запросе                             | Когда использовать (Best Practice)                                                                      |
| ------------------------ | -------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| Комбинация: Route + Body | URL: `/api/students/1`  <br>Body: Измененные данные JSON | Стандарт для обновления. ID ресурса передается в маршруте URL, а новые значения полей — в теле запроса. |

> 💡 Важно помнить: `PUT` полностью заменяет объект (если опустить поле, оно затрется дефолтным значением). `PATCH` обновляет объект частично (только переданные свойства).

Пример реализации в коде и HTTP:

```csharp
// PUT: Полная замена (Route для ID + Body для объекта)
// PUT api/students/1
[HttpPut("{id:int}")]
public IActionResult Update([FromRoute] int id, [FromBody] Student updatedStudent)
{
    var index = Students.FindIndex(s => s.Id == id);
    if (index == -1) return NotFound($"Студент с ID {id} не найден.");

    updatedStudent.Id = id; // Гарантируем сохранение ID
    Students[index] = updatedStudent;
    return NoContent(); // 204 No Content
}

// PATCH: Частичное обновление группы (Route для ID + Body для строки)
// PATCH api/students/1/group
[Patch("{id:int}/group")]
public IActionResult UpdateGroup([FromRoute] int id, [FromBody] string newGroup)
{
    var student = Students.FirstOrDefault(s => s.Id == id);
    if (student == null) return NotFound($"Студент с ID {id} не найден.");
    if (string.IsNullOrWhiteSpace(newGroup)) return BadRequest("Группа пустая.");

    student.Group = newGroup;
    return Ok(student);
}
```

```http
### Полное обновление (PUT)
PUT http://localhost:5000/api/students/1
Content-Type: application/json

{
  "name": "Иван Обновленный",
  "group": "ИП-25"
}

### Частичное обновление группы (PATCH)
PATCH http://localhost:5000/api/students/1/group
Content-Type: application/json

"ИП-99"
```

---

### 3.4 Передача параметров в DELETE-запросах

| Способ передачи                       | Как выглядит в URL / Запросе           | Когда использовать (Best Practice)                                                                                                                                               |
| ------------------------------------- | -------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Параметры маршрута (Route parameters) | `/api/students/4`                      | Основной способ. Для точечного удаления конкретного ресурса по его уникальному ID. Без этого параметра запрос обычно не имеет смысла.                                            |
| Строка запроса (Query parameters)     | `/api/students?group=ИП-21`            | Для массового или условного удаления по определенному фильтру (например, удалить все старые сессии, очистить корзину или логи за конкретную дату).                               |
| Тело запроса (Request Body)           | Скрыто из URL (передается внутри Body) | Не рекомендуется. Архитектура многих прокси-серверов настроена так, что тело в DELETE игнорируется. Применяется в редких исключениях (например, удаление пачки из 100 ID сразу). |

Пример реализации в коде и HTTP:

```csharp
// Удаление по ID (Route-параметр)
// DELETE api/students/1
[HttpDelete("{id:int}")]
public IActionResult Delete([FromRoute] int id)
{
    var student = Students.FirstOrDefault(s => s.Id == id);
    if (student == null) return NotFound($"Студент с ID {id} не найден.");

    Students.Remove(student);
    return Ok($"Студент с ID {id} успешно удален.");
}
```

```http
### Удаление конкретного студента (Route)
DELETE http://localhost:5000/api/students/1
```

---

## ❓ Быстрые вопросы для проверки (5 минут)

1. В чем разница между PUT и PATCH запросами? _(Ответ: PUT обновляет объект целиком, заменяя его, а PATCH вносит точечные, частичные изменения)._
2. Какой статус ответа сервера возвращает метод `NoContent()` и когда он уместен? _(Ответ: HTTP Status 204 — запрос обработан успешно, но серверу не нужно возвращать контент в теле. Уместен при PUT/изменении данных)._
3. Почему передавать тело (Body) в DELETE-запросе — это плохая практика? _(Ответ: По спецификации HTTP и логике работы многих сетевых прокси-серверов, тело в DELETE может быть проигнорировано или отрезано до того, как запрос дойдет до контроллера)._

## 4. Настройка маршрутизации в ASP.NET Core

Маршрутизация (Routing) — это процесс сопоставления входящего HTTP-запроса с конкретным методом контроллера (Action).
В Web API стандартом является маршрутизация с помощью атрибутов (Attribute Routing), когда пути прописываются прямо над классами и методами в квадратных скобках.

Роль файла `Program.cs` в маршрутизации:  
Именно в `Program.cs` подключаются middleware-компоненты маршрутизации:

1. `app.UseRouting();` — (в .NET 6-9 вызывается неявно под капотом, но важно понимать логику) сопоставляет URL с конечной точкой.
2. `app.MapControllers();` — сканирует приложение, находит все классы с атрибутом `[ApiController]` и регистрирует их маршруты.

_Что будет, если убрать атрибуты маршрутов у методов?_  
Если у методов контроллера API не указаны шаблоны (например, просто `[HttpGet]`), возникнет конфликт AmbiguousMatchException (неоднозначный матч), так как ASP.NET Core не поймет, какой именно из методов `HttpGet` вызвать при обращении к `/api/students`.

### 4.1 Атрибуты маршрутизации (Attribute Routing)

Маршруты задаются специальными атрибутами, которые могут дополнять друг друга.

| Атрибут                                                        | Описание                                                |
| -------------------------------------------------------------- | ------------------------------------------------------- |
| `[Route("api/[controller]")]` или<br>`[Route("api/students")]` | Базовый путь ко всем методам контроллера.               |
| `[HttpGet]`                                                    | Обрабатывает GET-запросы к базовому пути.               |
| `[HttpGet("{id}")]`                                            | GET-запрос к вложенному пути: `/api/students/{id}`.     |
| `[HttpPost]`                                                   | Создание ресурса (POST к базовому пути).                |
| `[HttpPut("{id}")]`                                            | Полное обновление ресурса по ID.                        |
| `[HttpDelete("{id}")]`                                         | Удаление ресурса по ID.                                 |
| `[HttpGet("labs")]`                                            | Дочерний путь для смежной логики: `/api/students/labs`. |

Пример структуры нашего контроллера:

```csharp
[ApiController]
[Route("api/[controller]")] 
public class StudentsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() 
    {
        return Ok(new[] { "Иван Иванов", "Мария Сидорова" });
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        return Ok($"Студент с ID #{id}");
    }

    [HttpPost]
    public IActionResult Create([FromBody] string name)
    {
        return Created("api/students/5", $"Студент '{name}' добавлен");
    }
}
```

---
### 4.2 Параметры маршрутов

Параметры маршрутов позволяют передавать значения прямо в URL. Они задаются в фигурных скобках `{}` и автоматически сопоставляются с аргументами метода по их именам.

#### Одиночный параметр

```csharp
[HttpGet("{id}")]
public IActionResult GetStudent(int id)
{
    return Ok($"Студент №{id}");
}
```

- Запрос: `GET /api/students/5`
- Ответ: `Студент №5`

> 📘 Правило: Если имя параметра в фигурных скобках `{id}` строго совпадает с именем аргумента метода `int id`, ASP.NET Core автоматически подставит значение.

#### Несколько параметров в одном пути

Иногда требуется передать несколько параметров для построения иерархии:

```csharp
[HttpGet("{group}/{id}")]
public IActionResult GetStudentByGroup(string group, int id)
{
    return Ok($"Группа: {group}, ID студента: {id}");
}
```

- Запрос: `GET /api/students/ИП-21/3`
- Ответ: `Группа: ИП-21, ID студента: 3`

#### Необязательные параметры

Если параметр в URL может отсутствовать, после его имени ставится знак вопроса `?`, а тип в C# должен поддерживать `null` (быть nullable):

```csharp
[HttpGet("{id?}")]
public IActionResult GetStudents(int? id)
{
    if (id.HasValue) return Ok($"Студент {id}");
    
    return Ok("Список всех студентов группы");
}
```

_Теперь корректны оба вызова:_ `/api/students` и `/api/students/10`.

#### Параметры с дефолтным (значением по умолчанию)

Можно задать значение, которое подставится само, если клиент его не передал:

```csharp
[HttpGet("filter/{status=active}")]
public IActionResult GetByStatus(string status)
{
    return Ok($"Фильтр по статусу: {status}");
}
```

- Запрос `/api/students/filter` вернёт `Фильтр по статусу: active`.
- Запрос `/api/students/filter/expelled` вернёт `Фильтр по статусу: expelled`.

---

### 4.3 Ограничения маршрутов (Route Constraints)

Ограничения добавляют строгие правила валидации к параметрам. Если входящий URL не соответствует правилу, роутер просто проигнорирует этот метод и сервер вернет стандартную ошибку `404 Not Found`.

|Ограничение|Пример использования|Описание|
|---|---|---|
|`int`|`[HttpGet("{id:int}")]`|Только целые числа.|
|`long`|`[HttpGet("{id:long}")]`|Длинные целые числа.|
|`bool`|`[HttpGet("{isStar:bool}")]`|Значения `true`/`false`.|
|`guid`|`[HttpGet("{id:guid}")]`|Глобальный уникальный идентификатор GUID.|
|`alpha`|`[HttpGet("{name:alpha}")]`|Только буквы (строка не должна содержать цифры).|
|`min(x)`|`[HttpGet("{id:int:min(1)}")]`|Число не может быть меньше 1.|
|`max(x)`|`[HttpGet("{id:int:max(500)}")]`|Число не может быть больше 500.|
|`range(a,b)`|`[HttpGet("{age:int:range(16,70)}")]`|Значение должно попадать в диапазон от 16 до 70.|
|`length(n)`|`[HttpGet("{code:length(6)}")]`|Строка строго фиксированной длины (6 символов).|
|`length(a,b)`|`[HttpGet("{group:length(3,10)}")]`|Длина строки от 3 до 10 символов.|
|`regex(...)`|`[HttpGet("{group:regex(^[А-Я]{2}-[0-9]{2}$)}")]`|Валидация по регулярному выражению (например, "ИП-21").|

Примеры комбинирования ограничений в коде:

```csharp
// Проверка на тип данных
[HttpGet("{id:int}")]
public IActionResult GetById(int id)
{
    return Ok($"Поиск студента по числовому ID: {id}");
}

// Проверка на диапазон длины строки (название учебной группы)
[HttpGet("group/{code:length(3,10)}")]
public IActionResult GetByGroupCode(string code)
{
    return Ok($"Поиск студентов группы: {code}");
}

// Связывание ограничений через двоеточие (ID должен быть от 1 до 1000)
[HttpGet("{id:int:min(1):max(1000)}")]
public IActionResult GetByRange(int id)
{
    return Ok($"Запрос студента в допустимом диапазоне ID: {id}");
}
```

## ❓ Быстрые вопросы для проверки (5 минут)

1. Зачем нужны ограничения маршрутов, такие как `:int` в `[HttpGet("{id:int}")]`? _(Ответ: Чтобы роутер отсекал неподходящие запросы на этапе парсинга URL и не пытался передать строку туда, где ожидается число, предотвращая ошибки или путаницу с другими маршрутами)._
2. Какой метод в `Program.cs` отвечает за то, чтобы наше приложение "увидело" атрибуты маршрутизации в контроллерах? _(Ответ: `app.MapControllers()`)._
3. Что подставится вместо токена `[controller]` в атрибуте `[Route("api/[controller]")]`, если класс называется `ProductsController`? _(Ответ: `api/products`)._

---

## ИТОГ

Что мы сегодня изучили:

1. Как связывать данные из HTTP-запросов с C# кодом с помощью Model Binding (`[FromBody]`, `[FromRoute]`, `[FromQuery]`).
2. Как спроектировать правильный, красивый RESTful CRUD интерфейс без "грязных" хаков в URL.
3. Как работает современная маршрутизация на атрибутах и как защитить роуты через Constraints.

## ❓ Вопросы для самопроверки

1. Что такое **Model Binding** в ASP.NET Core и для чего он нужен?
   *(Автоматически получает данные из HTTP-запроса и передаёт их в параметры методов контроллера.)*

2. Чем отличаются `[FromBody]`, `[FromRoute]` и `[FromQuery]`?
   *(Они получают данные соответственно из тела запроса, из пути URL и из строки запроса.)*

3. Какие операции входят в **CRUD** и какие HTTP-методы им соответствуют?
   *(Create — POST, Read — GET, Update — PUT/PATCH, Delete — DELETE.)*

4. В чём разница между `PUT` и `PATCH` при обновлении студента?
   *(PUT полностью заменяет объект, PATCH изменяет только указанные поля.)*

5. Как передаются параметры в `GET /api/students/5` и в `GET /api/students?search=Иван`?
   *(В первом случае используется Route-параметр, во втором — Query-параметр.)*

6. Где обычно передаются данные нового студента при `POST` и почему?
   *(В Body в формате JSON, потому что там передаётся структура создаваемого объекта.)*

7. Зачем в маршруте контроллера используется `[Route("api/[controller]")]` и что означает `[controller]`?
   *(Он задаёт базовый путь контроллера; `[controller]` заменяется именем контроллера без суффикса `Controller`, например `StudentsController` → `students`.)*

8. Что делают `app.UseRouting()` и `app.MapControllers()` в `Program.cs`?
   *(`UseRouting` участвует в сопоставлении запроса с маршрутом, а `MapControllers` регистрирует маршруты контроллеров.)*

9. Что означает ограничение `{id:int}` и что произойдёт, если вместо числа передать строку?
   *(Параметр должен быть целым числом; неподходящий URL не будет соответствовать этому маршруту, обычно вернётся 404.)*

---

➡️ **Перейти к выполнению самостоятельной работы:** [Инструкция к практическому заданию (PRACTICE.md)](./PRACTICE.md)
