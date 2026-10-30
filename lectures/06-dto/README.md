# Лекция 6. DTO, сервисный слой в Class Library, настройка миграций в Class Library.

**Цель занятия:** научиться разделять зоны ответственности в Web API, избавиться от «толстых» контроллеров с помощью сервисов, обезопасить данные через DTO и вынести логику работы с БД в отдельный проект (Class Library).

## 1. Паттерн DTO
**Entity (Сущность)** — это класс, который жестко привязан к структуре таблицы в базе данных (ORM EF Core).

**DTO (Data Transfer Object / Объект переноса данных)** — это простой класс-контейнер, который содержит только те данные, которые необходимы для передачи по сети между частями системы, например, клиентом (фронтендом) и сервером (бэкендом).

**Зачем разделять Entity и DTO?**
1. **Безопасность:** Нельзя давать клиенту возможность напрямую менять системные поля (например, `RegistrationDate`, `IsDeleted`, `Role`).
2. **Циклические ссылки (Ошибка сериализации):** Если у `Group` есть список `Students`, а у `Student` есть ссылка на `Group`, то при попытке превратить Group в JSON, сериализатор уйдет в бесконечный цикл (`Group` -> `Student` -> `Group` -> `Student`...) и приложение упадет с ошибкой `JsonException`.
3. **Оптимизация трафика:** Передаем только то, что нужно отобразить на форме.

### Реализация на практике

Перейименуем папку `Models` в `Entities` (Shift + F6). Поправим неймспейсы в соответствии с новым названием (клик правой кнопкой мышки по папке `Refactor this -> Adjust namespaces`). 

Создадим новую папку `DTOs` в проекте Web API и добавим туда четыре класса:

```csharp
// DTOs/GroupCreateDto.cs - для создания
namespace StudentApiWithDB.DTOs;

public class GroupCreateDto
{
    public string Name { get; set; } = string.Empty;
}
```

```csharp
// DTOs/GroupUpdateDto.cs - для обновления
namespace StudentApiWithDB.DTOs;

public class GroupCreateDto
{
    public string Name { get; set; } = string.Empty;
}
```

```csharp
// DTOs/GroupGetDto.cs - для отдачи клиенту
namespace StudentApiWithDB.DTOs;

public class GroupGetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<StudentShortDto> Students { get; set; } = new List<StudentShortDto>();
}
```

```csharp
namespace StudentApiWithDB.DTOs;

// DTOs/StudentShortDto.cs - для отдачи клиенту
public class StudentShortDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

#### Рефакторинг GroupController под DTO

Перепишем контроллер так, чтобы он принимал и возвращал `DTO`, выполняя ручной маппинг (сопоставление).

```csharp
using Microsoft.AspNetCore.Mvc;  
using Microsoft.EntityFrameworkCore;  
using StudentApiWithDB.Data;  
using StudentApiWithDB.Models;  
using StudentApiWithDB.DTOs; // Подключаем DTO
  
namespace GroupApiWithDB.Controllers;  
  
[ApiController]  
[Route("api/[controller]")]  
public class GroupController : ControllerBase  
{  
    private readonly AppDbContext context;  
    public GroupController(AppDbContext dbContext)  
    {        
        context = dbContext;  
    }    

    // 1. GET ALL / SEARCH (Async)
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search)
    {
        var groupsQuery = context.Groups
            .Include(g => g.Students)
            .AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            groupsQuery = groupsQuery.Where(s => s.Name.ToLower().Contains(search.ToLower()));
        }

        var groupsSelected = groupsQuery.Select(g => new GroupGetDto()
        {
            Id = g.Id,
            Name = g.Name,
            Students = g.Students.Select(s => new StudentShortDto()
            {
                Id = s.Id,
                Name = s.Name
            }).ToList()
        });
        return Ok(await groupsSelected.ToListAsync());
    }
    
    // 2. GET BY ID (Async)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        // EF Core сам скомпилирует это в SQL: SELECT ... WHERE Id = id LIMIT 1
        var group = await context.Groups.Select(g => new GroupGetDto()
        {
            Id = g.Id,
            Name = g.Name,
            Students = g.Students.Select(s => new StudentShortDto()
            {
                Id = s.Id,
                Name = s.Name
            }).ToList()
        }).FirstOrDefaultAsync(s => s.Id == id);
        
        if (group == null)
        {
            return NotFound($"Группа с ID {id} не найдена.");
        }
    
        return Ok(group);
    }  

    // 3. POST
    [HttpPost]  
    public async Task<IActionResult> Add( [FromBody] GroupCreateDto dto)  
    {        
        if (string.IsNullOrWhiteSpace(dto.Name))  
        {            
            return BadRequest("Поле Name обязательно.");  
        }   

        var newGroup = new Group {
            Name = dto.Name
        };
        
        await context.Groups.AddAsync(newGroup);  
        await context.SaveChangesAsync();  

        var resultDto = new GroupGetDto { Id = newGroup.Id, Name = newGroup.Name };
        
        return CreatedAtAction(nameof(GetById), resultDto);
    }   

    // 4. PUT  
    [HttpPut("{id:int}")]  
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] GroupUpdateDto dto)  
    {        
        if (string.IsNullOrWhiteSpace(dto.Name))  
        {            
            return BadRequest("Поле Name обязательно.");  
        }        
        
        var group = await context.Groups.FirstOrDefaultAsync(s => s.Id == id);  
        if (group == null)  
        {            
            return NotFound($"Группа с ID {id} не найдена.");  
        }  
        
        group.Name = dto.Name;  
        await context.SaveChangesAsync();  
        
        return NoContent();  
    }  

    // 5. DELETE
    // Тут ничего не изменилось, DTO нет
    [HttpDelete("{id:int}")]  
    public async Task<IActionResult> Delete([FromRoute] int id)  
    {        
        var group = await context.Groups.FirstOrDefaultAsync(s => s.Id == id);  
        if (group == null)  
        {            
            return NotFound($"Группа с ID {id} не найдена.");  
        }  
        
        context.Groups.Remove(group);  
        await context.SaveChangesAsync();  
        return Ok($"Группа с ID {id} успешно удалена.");  
    }
}
```

### Архитектурная справка: Как EF Core выполняет сложные LINQ-запросы

После рефакторинга код получения списка групп со студентами выглядит так:

```csharp
// 1. GET ALL / SEARCH (Async)
[HttpGet]
public async Task<IActionResult> Get([FromQuery] string? search)
{
    var groupsQuery = context.Groups
        .Include(g => g.Students)
        .AsQueryable();
    
    if (!string.IsNullOrWhiteSpace(search))
    {
        groupsQuery = groupsQuery.Where(s => s.Name.ToLower().Contains(search.ToLower()));
    }

    var groupsSelected = groupsQuery.Select(g => new GroupGetDto()
    {
        Id = g.Id,
        Name = g.Name,
        Students = g.Students.Select(s => new StudentShortDto()
        {
            Id = s.Id,
            Name = s.Name
        }).ToList()
    });
    return Ok(await groupsSelected.ToListAsync());
}
```

Разберем три главных вопроса производительности и синтаксиса этого метода.

#### 1. В какой момент выполняется запрос к базе данных?

Запрос к базе данных выполняется **строго в момент вызова замыкающего асинхронного метода** — в нашем случае это `ToListAsync()` (или `FirstOrDefaultAsync()` в методе получения единственной модели).

Эти методы работают как «триггер» или «кнопка запуска». До тех пор, пока цепочка LINQ-методов не дошла до них, запрос существует только в виде дерева выражений в памяти C#. Как только вызывается `FirstOrDefaultAsync()`, EF Core:
1. Берет всю построенную цепочку условий.
2. Переводит её на чистый язык SQL (в данном случае сгенерирует эффективный `LEFT JOIN`).
3. Открывает сетевое соединение с СУБД, выполняет запрос и мгновенно возвращает результат.

#### 2. Почему внешние методы асинхронные (`Async`), а вложенные списки — нет (`ToList`)?

Попытка написать `ToListAsync()` внутри вложенного `Select` приведет к ошибке компиляции. Это связано с архитектурой работы баз данных:

* **Внешний `FirstOrDefaultAsync()`** делает реальную сетевую поездку в базу данных. Это долгая операция ввода-вывода (I/O-bound), поэтому она обязана быть асинхронной, чтобы поток веб-сервера не блокировался в ожидании ответа от СУБД.
* **Внутренний `.ToList()`** — это не самостоятельный запрос к базе данных. Это всего лишь *инструкция по упаковке данных* внутри одного общего SQL-запроса. Она говорит серверу: *«Когда СУБД вернет нам результат одного общего JOIN-запроса, пожалуйста, сгруппируй полученные строки со студентами и упакуй их в объект `List`»*. Вложенный `Select` выполняется монолитно на стороне СУБД, поэтому создавать параллельные асинхронные потоки внутри него технически невозможно.

#### 3. Что такое `AsQueryable()`? Почему в динамических запросах без него плохо?

> [!NOTE]
> **TL;DR** Пока список  `IQueryable` — мы просто собирам запрос в памяти приложения. Как только список превращается в любой другой тип (массив Array, List, IEnumerable, ...), приложение выполняет запрос к базе данных.

Метод `AsQueryable()` (а также методы `Where`, `Select`, `AsNoTracking`) переводит коллекцию в интерфейс `IQueryable<T>`. Это явная декларация для компилятора C#: **«Мы строим SQL-запрос на бумаге, не выполняй его в памяти!»**.

* **С использованием `AsQueryable()` (или `IQueryable`):** Вы можете пошагово (например, через конструкции `if`) дописывать условия к будущему SQL-запросу. База данных получит уже готовый, отфильтрованный пирог.
* **Что будет, если НЕ написать `AsQueryable()` в динамических фильтрах:**
  Если вы решите объявить общую переменную через `var query = _context.Students;` и попытаетесь переприсвоить её внутри блоков `if` (например, `query = query.Where(...)`), код **не скомпилируется**, так как `DbSet` не позволяет динамически перезаписывать себя без приведения к интерфейсу запроса.
  
  Если же вы попытаетесь решить проблему компиляции «в лоб» и приведете коллекцию к `IEnumerable` (написав `IEnumerable<Student> query = _context.Students;`), **приложение потеряет производительность**.
  
  Без `AsQueryable()` интерфейс `IEnumerable` заставит программу выполнить слепой запрос `SELECT * FROM Students` и **выкачать абсолютно все строки из базы данных в оперативную память сервера**. И только потом C# начнет фильтровать их циклом. Если в базе 100 000 студентов, сервер упадет от нехватки памяти (Out of Memory).

## 2 Вынесение в Class Library и создание Сервисов

Сейчас наш Web API проект делает всё: принимает HTTP запросы, хостит базу данных, обрабатывает бизнес-логику. В реальной разработке проект делят на слои. Это позволяет лучше ориентироваться в проекте и переиспользовать код.

Мы вынесем работу с данными и логику в отдельную библиотеку классов (`Class Library`).

### Пошаговый алгоритм разделения.

1. Создаем в решении новый проект типа `Class Library` с именем `StudentApi.Data`.
2. Добавляем в `StudentApi.Data` пакеты Nuget: `Microsoft.EntityFrameworkCore.Sqlite` и `Microsoft.EntityFrameworkCore.Design`.
3. В основном проекте Web API добавляем Project Reference на проект `StudentApi.Data`.
4. Переносим в `StudentApi.Data` папки `Data`, `DTOs`, `Entites`, `Migrations`. Нажимаем правой кнопкой мышкой по каждой папке кроме Migrations и выполняем `Refactor this -> Adjust namespaces`.
5. Команда `Adjust namespaces` автоматически пропустит файлы в папке `Migrations`. Чтобы завершить их перенос и «подружить» со сменившим адрес `AppDbContext`, выполните глобальную замену:
    * Нажмите `Ctrl + Shift + H` (в Visual Studio) или `Ctrl + Shift + R` (в Rider).
    * В поле **Найти** введите старый namespace: `StudentApiWithDB.Migrations`
    * В поле **Заменить на** введите новый: `StudentApi.Data.Migrations`
    * В области поиска (Directory) выберите перенесенную папку `Migrations`. Нажмите **Replace All**.
5. В главном меню выполняем команду `Tools -> Clean Solution`.

### Создание интерфейса и сервиса

В проекте `StudentApi.Data` создаем логику сервиса GroupService. Сервис будет заниматься обработкой данных. Контроллер будет только обрабатывать сами запросы и дальше просто прокидывать данные в сервис.

```csharp
// Интерфейс сервиса в Class Library
using StudentApi.Data.DTOs; // В реальном проекте DTO выносятся в общую либу

namespace StudentApi.Data.Services;

public interface IGroupService
{
    Task<IEnumerable<GroupGetDto>> GetAllAsync(string? search);
    Task<GroupGetDto?> GetByIdAsync(int id);
    Task<GroupGetDto> CreateAsync(GroupCreateDto dto);
    Task<bool> UpdateAsync(int id, GroupUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
```

Реализация сервиса `GroupService`:
```csharp
using Microsoft.EntityFrameworkCore;
using StudentApi.Data.Data;
using StudentApi.Data.DTOs;
using StudentApi.Data.Entities;

namespace StudentApi.Data.Services;

public class GroupService : IGroupService
{
    private readonly AppDbContext context;

    public GroupService(AppDbContext context)
    {
        this.context = context;
    }
    
    public async Task<IEnumerable<GroupGetDto>> GetAllAsync(string? search)
    {
        var groupsQuery = context.Groups
            .Include(g => g.Students)
            .AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            groupsQuery = groupsQuery.Where(s => s.Name.ToLower().Contains(search.ToLower()));
        }

        var groups = await groupsQuery.Select(g => new GroupGetDto()
        {
            Id = g.Id,
            Name = g.Name,
            Students = g.Students.Select(s => new StudentShortDto()
            {
                Id = s.Id,
                Name = s.Name
            }).ToList()
        }).ToListAsync();
        return groups;
    }

    public async Task<GroupGetDto?> GetByIdAsync(int id)
    {
        // EF Core сам скомпилирует это в SQL: SELECT ... WHERE Id = id LIMIT 1
        var group = await context.Groups.Select(g => new GroupGetDto()
        {
            Id = g.Id,
            Name = g.Name,
            Students = g.Students.Select(s => new StudentShortDto()
            {
                Id = s.Id,
                Name = s.Name
            }).ToList()
        }).FirstOrDefaultAsync(s => s.Id == id);

        return group;
    }

    public async Task<GroupGetDto> CreateAsync(GroupCreateDto dto)
    {
        // Маппинг DTO -> Entity
        var newGroup = new Group 
        {
            Name = dto.Name
        };

        await context.Groups.AddAsync(newGroup);  
        await context.SaveChangesAsync();  
        
        var resultDto = new GroupGetDto { Id = newGroup.Id, Name = newGroup.Name };

        return resultDto;
    }

    public async Task<bool> UpdateAsync(int id, GroupUpdateDto dto)
    {
        var group = await context.Groups.FirstOrDefaultAsync(s => s.Id == id);  
        if (group == null) { return false; }  
        
        group.Name = dto.Name;  
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var group = await context.Groups.FirstOrDefaultAsync(s => s.Id == id);  
        if (group == null) { return false; }  
        
        context.Groups.Remove(group);  
        await context.SaveChangesAsync();
        return true;
    }
}
```

### Регистрация в Dependency Injection (Program.cs)

Так как `DbContext` и сервис `GroupService` теперь лежат в Class Library, в главном файле `Program.cs` веб-приложения нам нужно зарегистрировать их:

```csharp
using Microsoft.EntityFrameworkCore;
using StudentApiWithDB.Data;
using StudentApi.Data.Services;

var builder = WebApplication.CreateBuilder(args);

// Внедрение DbContext (он подтянется из Class Library)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрация нашего нового сервиса
builder.Services.AddScoped<IGroupService, GroupService>();

builder.Services.AddControllers();
```

### Чистый контроллер

Теперь `GroupController` становится максимально тонким:

```csharp
using Microsoft.AspNetCore.Mvc;
using StudentApi.Data.DTOs;
using StudentApi.Data.DTOs.Group;
using StudentApi.Data.Services;

namespace StudentApiWithDB.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupController : ControllerBase
{
    private readonly IGroupService groupService;
    
    public GroupController(IGroupService groupService)
    {
        this.groupService = groupService;
    }
    
    // 1. GET ALL / SEARCH (Async)
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search)
    {
        return Ok(await groupService.GetAllAsync(search));
    }
    
    // 2. GET BY ID (Async)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var group = await groupService.GetByIdAsync(id);
        if (group == null)
        {
            return NotFound($"Группа с ID {id} не найдена.");
        }
    
        return Ok(group);
    }
    
    // 3. POST (Async)
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] GroupCreateDto dto)  
    {        
        if (string.IsNullOrWhiteSpace(dto.Name))  
        {            
            return BadRequest("Имя группы обязательно.");  
        }
        var result =  await groupService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), result);
    }

    // 4. PUT  
    [HttpPut("{id:int}")]  
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] GroupUpdateDto dto)  
    {        
        if (string.IsNullOrWhiteSpace(dto.Name))  
        {            
            return BadRequest("Поле Name обязательно.");  
        }
        
        var success = await groupService.UpdateAsync(id, dto);
        if (success) { return NoContent(); }
        else { return NotFound($"Группа с ID {id} не найдена."); }
    }  

    // 5. DELETE
    [HttpDelete("{id:int}")]  
    public async Task<IActionResult> Delete([FromRoute] int id)  
    {        
        var success = await groupService.DeleteAsync(id);
        if (success) { return Ok($"Группа с ID {id} успешно удалена."); }
        else { return NotFound($"Группа с ID {id} не найдена."); }
    }
}
```

### Архитектурная справка: интерфейсы и регистрация в DI

#### 1. Почему у сервиса (`GroupService`) есть интерфейс (`IGroupService`), а у контекста базы данных (`AppDbContext`) — нет?

* **Интерфейс сервиса** нужен для того, чтобы полностью отделить контроллер (веб-интерфейс) от кода работы с данными. Контроллер знает только правила вызова методов (контракт), благодаря чему мы можем легко менять логику внутри сервиса или подменять его на «фейковый» при тестировании, не ломая код самого контроллера.
* **Интерфейс для базы данных** не создается, потому что класс `DbContext` в Entity Framework Core сам по себе является готовой абстракцией. Он уже объединяет в себе встроенные шаблоны для управления таблицами и сохранения данных. Повторное оборачивание его в интерфейс внутри одного и того же проекта данных лишь раздует код и добавит лишнюю работу без реальной пользы. При тестировании мы просто подменим реальную БД на стандартную InMemory самого фреймворка.

#### 2. Почему база данных регистрируется в файле `Program.cs` проекта Web API?

Несмотря на то, что код контекста БД находится в изолированной библиотеке классов, его настройка происходит в запускаемом веб-проекте по следующим причинам:

* **Доступ к настройкам:** Строка подключения к базе данных хранится в конфигурационном файле `appsettings.json`, который находится именно в проекте Web API.
* **Управление памятью:** Веб-сервер запускает базу данных в момент прихода HTTP-запроса от пользователя, передает её в сервис, а после отправки ответа клиенту автоматически закрывает её, освобождая оперативную память.
* **Точка сборки:** Проект библиотеки классов сам по себе запуститься не может — это просто набор файлов-инструкций. Файл `Program.cs` является главным управляющим всего приложения: он берёт логику из библиотеки, связывает её с настройками веб-проекта и запускает всю систему в работу.

### Коротко про паттерн Repository (Репозиторий)
Многие архитекторы создают поверх `DbContext` интерфейсы вроде `IRepository<T>`. Мы не используем его на этой лекции, потому что сам `DbContext` в EF Core уже является паттерном *Unit of Work*, а `DbSet` — паттерном *Repository*. Создание абстракции над абстракцией усложняет код (анти-паттерн "Interface Soup") без явной необходимости, если мы не планируем менять EF Core на чистый ADO.NET или Dapper.

## 3. Настройка миграций в Class Library, связь многие ко многим

### Startup Project vs Migrations Project
Мы перенесли `AppDbContext` в Class Library => в команде добавления миграций теперь нужно указывать два проекта:
* **Startup Project (Целевой проект)**: Проект, который запускается (наш Web API, где лежит `Program.cs` и `appsettings.json`).
* **Migrations Project (Проект миграций)**: Проект, где физически хранится папка `Migrations` и сам контекст базы данных (`StudentApi.Data`).

### Связь многие ко многим
Чтобы протестировать создание миграций, добавим таблицу со связью **Многие ко многим (N:M)**. Одна группа изучает много предметов (`Subject`), и один предмет изучают много групп (`Group`).

`Entities/Subject.cs`
```csharp
namespace StudentApi.Data.Entites;

public class Subject
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;

    // Навигационное свойство для связи "многие ко многим"
    public ICollection<Group> Groups { get; set; } = new List<Group>();
}

```

Модифицируем существующий класс `Group.cs`:
```csharp
namespace StudentApi.Data.Entities;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Навигационное свойство 1:N: у одной группы много студентов
    public ICollection<Student> Students { get; set; } = new List<Student>();
    
    //Навигационное свойство N:M: много групп изучают много предметов
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
```

Регистрируем новые таблицы в контексте данных `Data/AppDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using StudentApi.Data.Entities;

namespace StudentApi.Data.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Subject> Subjects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Автоматически находит и применяет ВСЕ конфигурации (наследники IEntityTypeConfiguration), 
        // которые находятся в текущей сборке проекта.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

В EF Core 5+ промежуточная таблица и связи по внешнему ключу генерируются автоматически, если сущности содержат коллекции друг друга. Но мы можем прописать связи в конфигурациях сущностей в явном виде.

Добавляем код в `Data/Configurations/GroupConfiguration.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentApi.Data.Entities;

namespace StudentApi.Data.Data.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        // Установка первичного ключа
        builder.HasKey(g => g.Id);

        // Имя группы обязательно для заполнения и не должно превышать 20 символов
        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(20);

        // Создание уникального индекса: в базе данных не может быть двух групп с одинаковым именем
        builder.HasIndex(g => g.Name)
            .IsUnique();
        
        // Настройка чистой связи "многие ко многим"
        builder.HasMany(g => g.Subjects)
            .WithMany(s => s.Groups);
    }
}
```

Создаем конфигурацию `Data/Configurations/SubjectConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentApi.Data.Entities;

namespace StudentApi.Data.Data.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        // Первичный ключ
        builder.HasKey(s => s.Id);

        // Свойства
        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(150);
            
        // Связь с StudentGroup здесь повторно не настраиваем, 
        // EF Core уже знает о ней из конфигурации группы.
    }
}
```

### Добавление миграции в отдельную Class Library через Rider

1. Щелкните правой кнопкой мышки по проекту StudentApi.Data и выберите пункт `Entity Framework Core -> Add Migration`.

2. **Настройка базовых параметров в окне:**
   * **Migrations project:** Укажите проект, в котором находится класс контекста данных (`AppDbContext`) и папка с миграциями — **StudentApi.Data**
   * **Startup project:** Выберите исполняемый проект Web Api — **StudentApiWithDB**
   * DbContext class: Выберите из выпадающего списка нужный контекст (если их несколько). *AppDbContext*

3. Задайти название миграции и нажмите кнопку `Add`

4. Примените миграцию к базе данных: правая кнопка мышки по проекту, `Entity Framework Core -> Update Database`.

### Управление миграциями через консоль CLI

Чтобы создать миграцию, теперь нужно явно указать консоли оба проекта:
```bash
dotnet ef migrations add AddSubject --project StudentApi.Data --startup-project StudentApiWithDB
```
* --project (или -p): куда положить файлы миграции.
* --startup-project (или -s): какой проект будет запускаться.

Для обновления базы данных:
```bash
dotnet ef database update --project StudentApi.Data --startup-project StudentApiWithDB
```

## ИТОГ
Что мы сегодня изучили:
1. Безопасность данных и решение проблемы циклических зависимостей при помощи DTO.
2. Создание слоев приложения с использованием Class Library.
3. Очистку контроллеров от бизнес-логики с переносом обязанностей в Сервисный слой.
4. Управление комплексными миграциями между разными типами проектов.

## Частые ошибки
* Забыли ссылку (Reference): Если проект Web API не видит типы сервисов, проверьте, добавили ли вы Project Reference на вашу библиотеку классов Class Library.
* Ошибка DI (InvalidOperationException): Если пишет, что сервис не может быть распознан, проверьте, добавили ли вы строку `builder.Services.AddScoped<IGroupService, GroupService>();` в файл `Program.cs`.
* Контекст в консоли: Если работаете с консолью, обязательно переходите в терминале в корень решения (где лежит файл .sln), прежде чем выполнять команды миграций dotnet ef.

## ❓ Вопросы для самопроверки

1. В чем ключевая разница между Entity и DTO моделью?
*(Ответ: Entity отображает структуру таблицы БД, содержит служебную системную информацию. DTO — легковесный объект для обмена данными по API, хранящий только те поля, которые нужны клиенту).*
2. Почему контроллеры не должны напрямую работать с AppDbContext в крупных проектах?
*(Ответ: Это усложняет тестирование, приводит к дублированию кода бизнес-логики в разных контроллерах и нарушает принцип SRP — Single Responsibility Principle).*
3. Что делает флаг --startup-project при генерации миграций через CLI?
*(Ответ: Указывает на исполняемый проект (Web API), из которого среда разработки берет настройки подключения к базе данных и конфигурационные файлы).*
4. Что произойдет, если вернуть Entity со связью "Многие-ко-многим" напрямую в контроллере без DTO?
*(Ответ: Произойдет круговая циклическая ссылка при сериализации данных в формат JSON, и сервер выбросит системное исключение).*

---

➡️ **Перейти к выполнению самостоятельной работы:** [Инструкция к практическому заданию (PRACTICE.md)](./PRACTICE.md)