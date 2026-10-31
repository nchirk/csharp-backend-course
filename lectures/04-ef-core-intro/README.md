# Лекция 4. Основы EF Core, async/await, Dependency Injection

На прошлом занятии мы создали Web API для управления списком студентов. Данные хранились в конструкции `private static readonly List<Student>`.

Проблема текущего подхода: при перезапуске приложения все добавленные или измененные студенты стираются. Оперативная память энергозависима. Нам нужно постоянное (персистентное) хранилище — база данных. Напрямую писать SQL-запросы в C# долго и неудобно, поэтому мы используем специальный инструмент — EF Core.

---

## 1. Введение в EF Core, настройка SQLite и первая модель

**Entity Framework Core (EF Core)** — это современный ORM (Object-Relational Mapper) от компании Microsoft для платформ .NET.
*   **Что делает ORM?** Он связывает объектно-ориентированный код (классы C#) с реляционными таблицами БД (SQL). Вместо написания `SELECT * FROM Students` мы пишем привычный C# код: `db.Students.ToList()`.
*   **Подходы к разработке:**
    1.  **Code-First (Сначала код):** Мы пишем классы на C# (сущности), а EF Core на их основе сам генерирует таблицы в БД. Это **современный стандарт** для большинства новых проектов. Разработчик управляет структурой через *миграции*.
    2.  **Database-First (Сначала БД):** База данных уже существует, её спроектировал сисадмин или DBA. Мы запускаем команду утилиты EF Core, и она генерирует C# классы на основе готовых таблиц. Используется для поддержки старых (Legacy) систем.

**Главные компоненты EF Core:**
*   **DbContext (Контекст данных):** Главный класс-мост. Представляет собой сессию работы с базой данных. Отвечает за подключение, отслеживание изменений и транзакции.
*   **DbSet\<T>:** Коллекция внутри контекста, представляющая конкретную таблицу в БД. С ней мы работаем как с обычным списком, но все операции транслируются в SQL.

### Реализация на практике
Для начала убедимся, что у нас есть модель `Student`. Создайте (или проверьте) файл `Models/Student.cs`:

```csharp
namespace StudentsApiWithModel.Models;

public class Student
{
    // EF Core автоматически распознает свойство Id (или StudentId) как Primary Key (Первичный ключ)
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
}
```

#### Установка пакетов

1. **Вариант через интерфейс.** Открываем в Rider `Tools -> Nuget -> Manage Nuget Packages for Solution`. Снимаем галочку `Prerelease`. В появившемся окне поиска пакетов находим провайдер базы данных 
`Microsoft.EntityFrameworkCore.Sqlite`. В окне справа выбираем нужный проект и нажимаем рядом с ним кнопку `+` — установить пакет.
2. **Вариант через Package Manager Console (PMC).**
Открываем в Rider консоль разработчика Alt-F12 либо в в VS консоль диспетчера пакетов (`Tools -> NuGet Package Manager -> Package Manager Console`) и устанавливаем провайдер базы данных SQLite:

```powershell
Install-Package Microsoft.EntityFrameworkCore.Sqlite
```

#### Cоздание DbContext
Создаем в корне проекта или в отдельной папке `Data` класс контекста `AppDbContext.cs`. *На этом шаге специально делаем жесткую привязку к SQLite внутри контекста (без DI), чтобы понять внутреннее устройство:*

```csharp
using Microsoft.EntityFrameworkCore;
using StudentsApiWithModel.Models;

namespace StudentsApiWithModel.Data;

public class AppDbContext : DbContext
{
    // Таблица студентов
    public DbSet<Student> Students => Set<Student>();

    // Настройка источника данных
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Указываем, что используем SQLite и задаем имя файла БД
        optionsBuilder.UseSqlite("Data Source=students.db");
    }
}
```

#### Внедрение в контроллер (временное решение «в лоб»)
Открываем контроллер, удаляем статический список `private static readonly List<Student> Students` и заменяем его локальным созданием контекста в методах.

Пример для `Get` метода (пока в синхронном варианте):
```csharp
// Внутри класса StudentsController:
// Заменяем работу со списком на работу с new AppDbContext()
[HttpGet]
public IActionResult Get([FromQuery] string? search)
{
    using var db = new AppDbContext(); // Открываем соединение с БД
    
    // Автоматически создаем БД и таблицы, если их еще нет на диске (для учебных целей)
    db.Database.EnsureCreated();

    if (string.IsNullOrWhiteSpace(search))
    {
        return Ok(db.Students.ToList()); 
    }

    var filtered = db.Students
        .Where(s => s.Name.Contains(search))
        .ToList();

    return Ok(filtered);
}
```
<details>
  <summary>Справочный блок: Как работать в режиме Database-First (БД уже есть)</summary>

  Если база данных уже создана сторонними средствами, мы не пишем классы вручную. Вместо этого мы используем инструмент скаффолдинга (Scaffolding).

  **Пошаговый алгоритм для справки:**

  1. Установить дополнительные инструменты в проект через PMC:
     ```powershell
     Install-Package Microsoft.EntityFrameworkCore.Design
     Install-Package Microsoft.EntityFrameworkCore.Tools
     ```

  2. Выполнить команду генерации кода в консоли PMC:
     ```powershell
     Scaffold-DbContext "Data Source=existing_database.db" Microsoft.EntityFrameworkCore.Sqlite -OutputDir ModelsFromDb -Context AppDbContextFromDb
     ```
</details>


## 2. Реализация CRUD, Async/Await

**Зачем нужен Async/Await при работе с БД?**
Запросы к базе данных — это операции ввода-вывода (I/O Bound). Сервер БД может находиться на другом компьютере или читать данные с медленного диска. 
*   **Синхронный подход:** Поток веб-сервера (который обрабатывает HTTP-запрос пользователя) отправляет запрос в БД и *блокируется* (ждет ответа). Он ничего не делает, но оперативная память под него выделена. Если придет 1000 пользователей одновременно — потоки закончатся, сервер начнет «виснуть».
*   **Асинхронный подход (`async/await`):** Поток отправляет запрос в БД и *освобождается* для обработки других HTTP-запросов. Когда БД пришлет ответ, свободный поток (не обязательно тот же самый) подхватит выполнение метода. Сервер выдерживает огромные нагрузки.

###  Мысленный эксперимент: Синхронный vs Асинхронный официант
> Представьте ресторан. 
> *Синхронный официант* принял заказ, ушел на кухню и стоит ждет, пока повар пожарит стейк (15 минут). Другие столики ждут и злятся. 
> *Асинхронный официант* принял заказ, отдал на кухню и пошел принимать заказы у других клиентов. Когда стейк готов, любой свободный официант выносит его.

### Живой эксперимент:  Синхронная vs Асинхронная блокировка потоков

Представим, что мы добавили в эндпоинт получения студентов тяжелую операцию на **2 секунды**.

*   **Синхронный плохой вариант (Блокирует поток):**
    ```csharp
    [HttpGet("sync-heavy")]
    public IActionResult GetSyncHeavy()
    {
        // Поток веб-сервера полностью засыпает на 2 секунды. 
        // Он не может обрабатывать другие запросы!
        Thread.Sleep(2000); 
        using var db = new AppDbContext();
        return Ok(db.Students.ToList());
    }
    ```

*   **Асинхронный правильный вариант (Освобождает поток):**
    ```csharp
    [HttpGet("async-heavy")]
    public async Task<IActionResult> GetAsyncHeavy()
    {
        // Поток отправляет задачу «подождать 2 секунды» в фоновый режим 
        // и моментально возвращается в пул потоков обрабатывать чужие HTTP-запросы.
        await Task.Delay(2000); 
        return Ok(await _db.Students.ToListAsync());
    }
    ```

Проведем «бомбардировку» API (нагрузочное тестирование). Для тестирования можно использовать консольную утилиту **Bombardier**, **Apache Benchmark (ab)** или инструмент **k6**. Мы отправляем **100 одновременных запросов** на оба эндпоинта.

1.  **Результат для `sync-heavy`:**
    *   Сервер Kestrel имеет ограниченный пул потоков (например, по умолчанию доступно 20-30 активных потоков).
    *   Первые 20 запросов захватывают все потоки и встают «намертво» на 2 секунды благодаря `Thread.Sleep`.
    *   Оставшиеся 80 запросов встают в очередь. Время ожидания (Latency) для последних пользователей возрастает до **6–8 секунд**! Сервер начинает захлебываться, процессор простаивает, а память забита.

2.  **Результат для `async-heavy`:**
    *   100 запросов прилетают одновременно. Потоки принимают запросы, регистрируют задачу `Task.Delay` и тут же освобождаются.
    *   Один и тот же поток успевает подхватить десятки запросов.
    *   Ровно через 2 секунды все 100 пользователей **одновременно** получают свой ответ. Время ожидания для всех составило ровно **2 секунды**. Сервер работает стабильно и готов принять еще тысячи запросов.


### Реализация асинхронного CRUD через БД 
Переписываем `StudentsController` целиком. Избавляемся от старого списка и переводим все методы на рельсы асинхронного EF Core.

``` csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentsApiWithModel.Data;
using StudentsApiWithModel.Models;
namespace StudentsApiWithModel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    // 1. GET ALL / SEARCH (Async)
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search)
    {
        using var db = new AppDbContext();
        db.Database.EnsureCreated(); // Гарантируем наличие БД

        if (string.IsNullOrWhiteSpace(search))
        {
            // Используем асинхронную версию ToListAsync()
            return Ok(await db.Students.ToListAsync());
        }

        var filtered = await db.Students
            .Where(s => s.Name.ToLower().Contains(search.ToLower()))
            .ToListAsync();

        return Ok(filtered);
    }

    // 2. GET BY ID (Async)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        using var db = new AppDbContext();
        
        // EF Core сам скомпилирует это в SQL: SELECT ... WHERE Id = id LIMIT 1
        var student = await db.Students.FirstOrDefaultAsync(s => s.Id == id);
        
        if (student == null)
        {
            return NotFound($"Студент с ID {id} не найден.");
        }

        return Ok(student);
    }

    // 3. POST (Async)
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] Student newStudent)
    {
        if (string.IsNullOrWhiteSpace(newStudent.Name) || string.IsNullOrWhiteSpace(newStudent.Group))
        {
            return BadRequest("Имя и группа обязательны.");
        }

        using var db = new AppDbContext();
        
        // ВАЖНО: Мы НЕ высчитываем ID вручную! 
        // БД SQLite сама сгенерирует автоинкрементный ID при сохранении.
        await db.Students.AddAsync(newStudent);
        
        // Фиксируем изменения в БД физически
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = newStudent.Id }, newStudent);
    }

    // 4. PUT (Async)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] Student updatedStudent)
    {
        if (string.IsNullOrWhiteSpace(updatedStudent.Name) || string.IsNullOrWhiteSpace(updatedStudent.Group))
        {
            return BadRequest("Поля Name и Group обязательны.");
        }

        using var db = new AppDbContext();
        var student = await db.Students.FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
        {
            return NotFound($"Студент с ID {id} не найден.");
        }

        // Обновляем свойства найденного объекта, EF Core отследит эти изменения
        student.Name = updatedStudent.Name;
        student.Group = updatedStudent.Group;

        await db.SaveChangesAsync();
        return NoContent();
    }

    // 5. PATCH (Async)
    [HttpPatch("{id:int}/group")]
    public async Task<IActionResult> UpdateGroup([FromRoute] int id, [FromBody] string newGroup)
    {
        if (string.IsNullOrWhiteSpace(newGroup))
        {
            return BadRequest("Название группы не может быть пустым.");
        }

        using var db = new AppDbContext();
        var student = await db.Students.FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
        {
            return NotFound($"Студент с ID {id} не найден.");
        }

        student.Group = newGroup.Trim();
        
        await db.SaveChangesAsync();
        return Ok(student);
    }

    // 6. DELETE (Async)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        using var db = new AppDbContext();
        var student = await db.Students.FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
        {
            return NotFound($"Студент с ID {id} не найден.");
        }

        // Удаляем объект из DbSet
        db.Students.Remove(student);
        
        await db.SaveChangesAsync();
        return Ok($"Студент с ID {id} успешно удален.");
    }
}
```

Проверка: запустите приложение, выполните POST запрос через http-файл. Проверьте, что в корне проекта появился файл `students.db`. Проверьте, что данные корректно записываются в базу и корректно читаются из нее.

### Как посмотреть содержимое базы данных SQLite

После запуска приложения и выполнения первого `POST`-запроса в корне проекта появится файл `students.db`. Так как это бинарный файл, открыть его обычным блокнотом не получится. 
#### Способ 1. Просмотр через JetBrains Rider
В Rider есть мощный встроенный менеджер баз данных. Настраивается он за несколько кликов:
1. В правом верхнем углу (или на правой панели инструментов) найдите и откройте вкладку **Database**.
2. Выберите **Data Sources...** (иконка со стопкой).
3. В открывшемся окне нажмите на иконку **`+`** (New) -> **Data Source** -> **SQLite**.
4. В открывшемся окне настроек:
   * Если внизу горит предупреждение *«Missing driver»*, нажмите ссылку **Download** (Rider сам скачает нужный драйвер за 5 секунд).
   * В поле **File** нажмите на иконку папки и выберите файл `students.db` из папки вашего проекта.
   * Нажмите кнопку **Test Connection** в левом нижнем углу. Должна появиться зеленая галочка.
   * Нажмите **OK**.
5. Теперь во вкладке *Database* разверните дерево: `students.db` -> `schemas` -> `main` -> `tables`.
6. Кликните дважды по таблице `Students` — откроется удобная интерактивная таблица, где можно смотреть, редактировать и удалять записи напрямую.

#### Способ 2. DB Browser for SQLite
1. Скачайте и установите бесплатную программу [DB Browser for SQLite](https://sqlitebrowser.org).
2. Запустите программу и нажмите **Open Database** (Открыть базу данных).
3. Укажите путь к файлу `students.db` вашего проекта.
4. Перейдите на вкладку **Browse Data** (Просмотр данных) и в выпадающем списке выберите таблицу `Students`. Вы увидите все строки, сохраненные вашим API.


## 3. Dependency Injection в ASP.NET Core

### Что такое Dependency Injection (Внедрение зависимостей)?
Создавать контекст через `using var db = new AppDbContext()` внутри каждого метода — плохая практика (жесткая связность, Hard Coupling). Контроллер не должен знать, какая именно БД используется и как к ней подключаться. Он должен просто просить: «Дайте мне контекст для работы».
В ASP.NET Core встроен мощный контейнер зависимостей (IoC-контейнер). Мы регистрируем AppDbContext в файле Program.cs, и фреймворк автоматически передает его в конструктор контроллера.

### Какая польза от DI при работе с БД?
Сегодня мы пишем код под SQLite. Завтра проект вырос, и нам нужен PostgreSQL или SQL Server. Если конфигурация зашита внутри контроллера или контекста, придется переписывать весь код. С DI мы меняем одну строку кода в Program.cs.
Также для быстрых Unit-тестов удобно использовать InMemory Database (БД в оперативной памяти, имитирующая реляционную базу, но работающая без создания файлов на диске). Давайте попробуем заменить SQLite на InMemory Database.

### Реализация на практике

#### Перестраиваем AppDbContext под DI
Убираем жесткую конфигурацию из класса контекста. Открываем `Data/AppDbContext.cs` и переписываем его:

```csharp
using Microsoft.EntityFrameworkCore;
using StudentsApiWithModel.Models;
namespace StudentsApiWithModel.Data;

public class AppDbContext : DbContext
{
    // Конструктор принимает настройки извне и передает их в базовый класс DbContext
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
}
```

### Регистрация в Program.cs (Сначала SQLite)
Открываем `Program.cs`. Регистрируем наш контекст в контейнере служб (до вызова `builder.Build()`):

```csharp
using Microsoft.EntityFrameworkCore;using StudentsApiWithModel.Data;
var builder = WebApplication.CreateBuilder(args);
// Регистрируем AppDbContext с использованием SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=students.db"));

builder.Services.AddControllers();// ... остальной стандартный код
```

### Внедрение через конструктор в Контроллере
Теперь переписываем контроллер `StudentsController`` так, чтобы он получал контекст через DI. Конструкции using var db = new AppDbContext()` больше не нужны!

```csharp
// Изменения в начале контроллера:
public class StudentsController : ControllerBase
{
    private readonly AppDbContext _db;

    // Внедрение зависимости через конструктор
    public StudentsController(AppDbContext db)
    {
        _db = db;
        // Автоматически создаем БД при первом обращении к контроллеру (для тестов)
        _db.Database.EnsureCreated(); 
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search)
    {
        // Теперь просто используем приватное поле _db без всяких using
        if (string.IsNullOrWhiteSpace(search))
        {
            return Ok(await _db.Students.ToListAsync());
        }

        var filtered = await _db.Students
            .Where(s => s.Name.ToLower().Contains(search.ToLower()))
            .ToListAsync();

        return Ok(filtered);
    }
    
    // ... аналогично заменяем 'db.' на '_db.' во всех остальных методах ...
}
```

### Легкая смена источника на InMemory
Покажем гибкость DI.

   1. Установим пакет InMemory через PMC: `Install-Package Microsoft.EntityFrameworkCore.InMemory`
   2. Заходим в Program.cs и комментируем строку с .UseSqlite(...).
   3. Вместо нее пишем:

```csharp
// Переключение на базу данных в памяти одной строчкой
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("StudentsInMemoryDb"));
```

Запускаем проект! Приложение работает точно так же, но файл students.db больше не используется — данные живут в памяти, пока работает приложение. Это доказывает силу DI и абстракции EF Core.

### Промышленный стандарт: Перенос строки подключения в appsettings.json

Писать строку подключения `"Data Source=students.db"` прямо в коде `Program.cs` — это плохая практика (hardcode). Если адрес базы данных изменится (например, при переносе на рабочий сервер), вам придется перекомпилировать весь код приложения. 

Правильный подход — вынести настройки в конфигурационный файл.

#### Шаг 1. Настройка appsettings.json
Откройте файл `appsettings.json` в корне проекта и добавьте секцию `ConnectionStrings` на самый верхний уровень:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=students.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

#### Шаг 2. Изменение конфигурации в Program.cs
Теперь в файле `Program.cs` мы динамически считываем эту строку с помощью встроенного объекта `builder.Configuration`:

```csharp
using Microsoft.EntityFrameworkCore;
using StudentsApiWithModel.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Извлекаем строку подключения из appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Передаем ее в настройки DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddControllers();
// ...
```

#### В чем выгода для разработчика?
Если завтра вам понадобится переключиться на другую базу данных (или на сервер тестирования), вы просто измените одну строчку в текстовом файле `appsettings.json` без изменения и пересборки C# кода.

---

## ИТОГ
Что мы сегодня изучили.

   1. Понятие ORM систем и архитектуру EF Core (DbContext, DbSet).
   2. Разницу между подходами Code-First (пишем код -> получаем БД) и Database-First.
   3. Как писать безопасные, масштабируемые асинхронные контроллеры с ключевыми словами async, await и методами ToListAsync(), SaveChangesAsync().
   4. Внедрение зависимостей (DI) для гибкой настройки провайдеров баз данных.

## ❓ Вопросы для самопроверки

   1. Что такое EF Core и какую основную задачу он решает?
   *(Ответ: Это ORM-система, которая связывает C#-классы с таблицами в реляционной БД, избавляя разработчика от написания ручного SQL-кода).*
   2. В чем разница между подходом Code-First и Database-First?
   *(Ответ: При Code-First мы сначала пишем классы сущностей в коде, а EF Core генерирует БД. При Database-First база уже есть, и структуры C# генерируются по готовой БД).*
   3. Зачем нужно вызывать метод SaveChangesAsync()? Что будет, если его забыть написать после Add()?
   *(Ответ: Этот метод физически транслирует изменения в БД и фиксирует транзакцию. Если его забыть, данные добавятся только в локальную память DbSet C#, но в саму базу на диске не запишутся).*
   4. Почему при работе с БД критически важно использовать асинхронные методы (с префиксом Async)?
   *(Ответ: Работа с БД — долгая I/O операция. Асинхронность освобождает потоки сервера во время ожидания ответа от БД, позволяя серверу обрабатывать другие запросы и держать высокую нагрузку).*
   5. Каким образом паттерн Dependency Injection (DI) помогает при смене провайдера БД (например, со SQLite на InMemory)?
   *(Ответ: Контроллер не привязан к конкретной БД. Мы меняем конфигурацию подключения в одном месте — файле Program.cs, а сам контроллер продолжает работать с абстрактным контекстом без изменений).*

## Частые ошибки

* Забытый await: Если написать var users = _db.Students.ToListAsync(); без ключевого слова await, то вместо списка студентов вы получите объект задачи Task<List<Student>>. Всегда проверяйте наличие await перед асинхронными вызовами.
* Использование синхронных методов внутри асинхронных: Не смешивайте ToListAsync() и обычный SaveChanges(). Используйте их парные асинхронные версии (SaveChangesAsync()).
* Конфликты контекста (Scoped): Контекст БД регистрируется со временем жизни Scoped (один экземпляр на один HTTP-запрос). Никогда не пытайтесь сохранить экземпляр AppDbContext в статическую переменную, это приведет к ошибкам многопоточности.

---

➡️ **Перейти к выполнению самостоятельной работы:** [Инструкция к практическому заданию (PRACTICE.md)](./PRACTICE.md)