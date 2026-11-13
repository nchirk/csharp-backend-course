# Лекция 8 — CRUD на Razor Pages.

**Цели занятия:**
1. Научиться строить серверный рендеринг страниц с помощью **Razor Pages**.
2. Освоить работу с формами, привязкой моделей (`[BindProperty]`), Tag Helpers и паттерном **PRG (Post-Redirect-Get)**.
3. Научиться декомпозировать интерфейс с помощью **Partial Views**.

## Теория

**Razor Pages** — это фреймворк в составе ASP.NET Core для создания веб-приложений, ориентированный на страницы. Каждая страница объединяет разметку (`.cshtml`) и серверную логику в модели страницы (`.cshtml.cs`), что упрощает разработку по сравнению с классическим MVC.

---
### 1. Синтаксис Razor

**Razor** — это движок представлений, позволяющий встраивать серверный код на языке C# прямо в HTML-разметку с помощью символа `@`.

- **Встроенные выражения (Implicit):** Выводят значение переменной или свойства.
    ``` html
    <p>Текущее время: @DateTime.Now.ToShortTimeString()</p>
    <p>Привет, @Model.UserName!</p>
    ```
- **Явные выражения (Explicit):** Используются, когда C#-код нужно отделить от текста.
    ``` html
    <p>Цена со скидкой: @(Model.Price * 0.9m) руб.</p>
    ```
- **Блоки кода:** Объявляют переменные и содержат логику без прямого вывода в HTML.
    ``` html
    @{
        var greeting = "Добро пожаловать";
        var isWeekend = DateTime.Now.DayOfWeek == DayOfWeek.Saturday;
    }
    ```
- **Управляющие конструкции:** Работают стандартные циклы и условия C#.
    ``` html
    @if (Model.Items.Any())
    {
        <ul>
        @foreach (var item in Model.Items)
        {
            <li>@item.Name</li>
        }
        </ul>
    }
    else
    {
        <p>Список пуст.</p>
    }
    ```
- **Директивы Razor:** Управляют поведением страницы.
    - `@page` — делает файл `.cshtml` самостоятельной страницей Razor Pages (должна быть первой строкой).
    - `@model` — указывает класс `PageModel`, связанный со страницей.
    - `@using` — подключает пространства имён.
    - `@inject` — внедряет зависимости (DI) напрямую в представление.

---
### 2. Tag Helpers (Тег-хелперы)

**Tag Helpers** позволяют серверному коду участвовать в создании и рендеринге HTML-элементов в файлах Razor. Они выглядят как обычные HTML-атрибуты с префиксом `asp-`, что делает код чистым и понятным для верстальщиков.

Основные группы Tag Helpers:

| Категория         | Тег / Атрибут                      | Назначение и пример                                                                                                                              |
| ----------------- | ---------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Навигация**     | `<a>` с `asp-page`                 | Генерирует ссылку на другую страницу Razor Pages.  <br>`<a asp-page="/Products/Details" asp-route-id="5">Товар</a>`                              |
| **Формы**         | `<form>` с `asp-page-handler`      | Указывает метод-обработчик в `PageModel` при отправке формы.  <br>`<form method="post" asp-page-handler="Delete">`                               |
| **Поля ввода**    | `<input>` с `asp-for`              | Привязывает поле ввода к свойству модели (генерирует `id`, `name`, `type`, `value`).  <br>`<input asp-for="Input.Email" class="form-control" />` |
| **Метки**         | `<label>` с `asp-for`              | Создаёт тег `<label>` с текстом из атрибута `[Display(Name = "...")]`.  <br>`<label asp-for="Input.Email"></label>`                              |
| **Валидация**     | `<span>` с `asp-validation-for`    | Выводит сообщение об ошибке валидации для конкретного поля.  <br>`<span asp-validation-for="Input.Email" class="text-danger"></span>`            |
| **Сводка ошибок** | `<div>` с `asp-validation-summary` | Выводит общий список ошибок валидации модели.  <br>`<div asp-validation-summary="ModelOnly"></div>`                                              |
|                   |                                    |                                                                                                                                                  |

---

### 3. Partial Views (Частичные представления)

**Partial View** — это отдельный файл `.cshtml` (без директивы `@page`), который рендерит фрагмент HTML-разметки внутри другой страницы.

- **Зачем нужны:** Позволяют разбивать сложные страницы на мелкие компоненты и переиспользовать повторяющиеся блоки (карточка товара, форма поиска, меню, модальное окно).
- **Соглашение об именовании:** Имена файлов принято начинать с нижнего подчёркивания (например, `_ProductCard.cshtml`). Они размещаются в папке `Pages/Shared` или в папке конкретной страницы.
- **Способы подключения:**
    1. Через Tag Helper (рекомендуемый способ):

        ``` html
        <partial name="_ProductCard" model="item" />
        ```
        
    2. Через асинхронный HTML Helper:
        
        ``` html
        @await Html.PartialAsync("_ProductCard", item)
        ```
        
- **Передача данных:** Если атрибут `model` не указан, частичное представление автоматически получает ту же модель (`Model`), что и родительская страница.

---
### 4. Мастер-страницы (_Layout)

**_Layout.cshtml** — это файл общего шаблона (макета), который задаёт единый каркас для всех страниц сайта: теги `<html>`, `<head>`, подключение CSS/JS, шапку (`<header>`) и подвал (`<footer>`).

- **Где находится:** По умолчанию лежит в `Pages/Shared/_Layout.cshtml` и подключается ко всем страницам через файл `Pages/_ViewStart.cshtml`.
- **Ключевые методы в `_Layout.cshtml`:**
    - `@RenderBody()` — обязательный метод. Указывает место, куда будет вставлено основное содержимое конкретной страницы (`.cshtml`).
    - `@RenderSection("Scripts", required: false)` — необязательная секция. Позволяет конкретным страницам вставлять свой код (например, скрипты валидации) в строго определённое место макета.

**Пример структуры `_Layout.cshtml`:**

``` html
<!DOCTYPE html>
<html>
<head>
    <title>@ViewData["Title"] - Мой сайт</title>
    <link rel="stylesheet" href="~/css/site.css" />
</head>
<body>
    <header>Общее меню навигации</header>

    <main>
        <!-- Сюда подставится контент текущей страницы -->
        @RenderBody()
    </main>

    <footer>© 2026 Учебный проект</footer>

    <!-- Сюда подставятся скрипты, если страница объявила @section Scripts -->
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

**Как страница использует секцию и переопределяет макет:**

``` html
@page
@{
    ViewData["Title"] = "Контакты";
    // При необходимости можно отключить или сменить макет:
    // Layout = null; 
}

<h1>Наши контакты</h1>

@section Scripts {
    <script src="~/js/map.js"></script>
}
```

---
### 5. Передача данных через ViewData

**ViewData** — это словарь (`ViewDataDictionary`), который используется для передачи небольших порций нестрого типизированных данных из `PageModel` в представление или из страницы в мастер-страницу (`_Layout`).

- **Особенности:** Ключами являются строки, а значения имеют тип `object` (при чтении в коде требуется приведение типов, кроме вывода в HTML). Время жизни ограничено одним HTTP-запросом.
- **Частый сценарий:** Передача заголовка вкладки браузера (`Title`) из конкретной страницы в общий макет `_Layout.cshtml`.

``` csharp
// Внутри страницы Index.cshtml (или в PageModel)
@{
    ViewData["Title"] = "Главная страница";
    ViewData["Message"] = "Сегодня действуют скидки!";
}

<h2>@ViewData["Message"]</h2>
```

---
### 6. Паттерн PRG

**PRG (Post-Redirect-Get)** — это архитектурный паттерн веб-разработки, предотвращающий повторную отправку данных формы при обновлении страницы пользователем (`F5`).

Проблема без PRG:

1. Пользователь отправляет форму методом `POST` (например, создаёт заказ).
2. Сервер обрабатывает запрос и сразу возвращает HTML-страницу в ответ на `POST`.
3. Пользователь нажимает «Обновить» (`F5`). Браузер повторяет последний запрос (`POST`), что приводит к **дублированию заказа**.

Как работает PRG в Razor Pages:

1. **POST:** Браузер отправляет данные формы в метод `OnPost()` или `OnPostAsync()`.
2. **REDIRECT:** Сервер сохраняет данные в БД, при необходимости записывает уведомление в `TempData` и возвращает ответ `RedirectToPage(...)` (HTTP статус `302 Found`).
3. **GET:** Браузер автоматически делает новый `GET`-запрос по указанному адресу (`OnGet()`). Теперь при нажатии `F5` безопасно обновится только `GET`-страница.

## Практика: Разработка CRUD-приложения на ASP.NET Core Razor Pages

---
## Подготовка к работе (Исходное состояние)

Откройте в **JetBrains Rider** солюшен с прошлого занятия. В нём находятся два основных проекта:
1. `StudentApi.Data` — библиотека классов с DTO, контекстом БД и готовым сервисом `IStudentService` (а также `IGroupService`).
2. `StudentRazorPages` — веб-проект, в котором уже настроено внедрение зависимостей (DI) и создана стартовая страница `Pages/Students/Index.cshtml`.

> **Важно:** Убедитесь, что в проекте `StudentRazorPages` в файле `Program.cs` зарегистрированы `IStudentService` и `IGroupService`, а база данных содержит хотя бы одну учебную группу.

## 1. Таблица студентов, страница Details и навигация

Сейчас страница `Pages/Students/Index.cshtml` выводит список строк `List<string>`. Чтобы мы могли переходить на страницу конкретного студента или удалять его, нам нужен его `Id`.

### Шаг 1.1. Обновляем `Index.cshtml.cs`
Изменим тип свойства `Students` со списка строк на коллекцию `StudentGetDto`.

```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentApi.Data.DTOs.Student;
using StudentApi.Data.Services;

namespace StudentRazorPages.Pages.Students;

public class Index : PageModel
{
    private readonly IStudentService _studentService;

    public Index(IStudentService studentService)
    {
        _studentService = studentService;
    }

    public IEnumerable<StudentGetDto> Students { get; set; } = [];
    public string TotalCountMessage => $"Всего студентов в базе: {Students.Count()}";

    public async Task OnGetAsync()
    {
        Students = await _studentService.GetAllAsync(null);
    }
}
```

### Шаг 1.2. Перевёрстываем `Pages/Students/Index.cshtml` в таблицу
Заменим маркированный список `<ul>` на таблицу Bootstrap. Добавим кнопку **Создать** над таблицей и три кнопки действий в каждую строку.

```html
@page
@model StudentRazorPages.Pages.Students.Index
@{
    ViewData["Title"] = "Список студентов";
}

<h2>Список студентов</h2>

<p>
    <a asp-page="./Create" class="btn btn-primary">+ Добавить студента</a>
</p>

<p>@Model.TotalCountMessage</p>

<table class="table">
    <thead>
        <tr>
            <th>ID</th>
            <th>Имя</th>
            <th>Возраст</th>
            <th>Действия</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var student in Model.Students)
        {
            <tr>
                <td>@student.Id</td>
                <td>@student.Name</td>
                <td>@student.Age</td>
                <td>
                    <a asp-page="./Details" asp-route-id="@student.Id" class="btn btn-info">Просмотреть</a>
                    <a asp-page="./Update" asp-route-id="@student.Id" class="btn btn-warning">Обновить</a>
                    <button type="button" class="btn btn-danger" disabled>Удалить</button>
                </td>
            </tr>
        }
    </tbody>
</table>
```

> **Как задаётся Title и подключается Layout?**
> В папке `Pages/` есть файл `_ViewStart.cshtml`, который автоматически назначает `Layout = "_Layout";` для всех страниц. Когда мы пишем `ViewData["Title"] = "Список студентов";` внутри страницы, это значение попадает в словарь `ViewData`, который затем считывается в `Pages/Shared/_Layout.cshtml` внутри тега `<title>@ViewData["Title"]</title>`. (`ViewBag` — это динамическая обёртка над `ViewData`, в Razor Pages предпочтительнее использовать `ViewData`).

### Шаг 1.3. Создаём страницу `Details`
В Rider нажмите правой кнопкой мыши на папку `Pages/Students` -> **Add** -> **Razor Page**. Назовите её `Details`.

**Файл `Pages/Students/Details.cshtml.cs`:**
Используем маршрут `@page "{id:int}"`, чтобы ID передавался как часть URL (`/Students/Details/1`), аналогично `GET /api/student/1` в Web API.

```csharp
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
        var student = await _studentService.GetByIdAsync(id);
        if (student is null)
        {
            return NotFound();
        }

        Student = student;
        return Page();
    }
}
```

**Файл `Pages/Students/Details.cshtml`:**
```html
@page "{id:int}"
@model StudentRazorPages.Pages.Students.Details
@{
    ViewData["Title"] = "Карточка студента";
}

<h2>Информация о студенте</h2>

<div class="card mt-3" style="max-width: 500px;">
    <div class="card-body">
        <h5 class="card-title">@Model.Student.Name</h5>
        <p class="card-text"><strong>ID:</strong> @Model.Student.Id</p>
        <p class="card-text"><strong>Возраст:</strong> @Model.Student.Age лет</p>
        
        <a asp-page="./Update" asp-route-id="@Model.Student.Id" class="btn btn-warning">Редактировать</a>
        <a asp-page="./Index" class="btn btn-secondary">Назад к списку</a>
    </div>
</div>
```

### Шаг 1.4. Добавляем ссылку в главное меню (`_Layout.cshtml`)
Откройте `Pages/Shared/_Layout.cshtml`, найдите навигационное меню (`<ul class="navbar-nav flex-grow-1">`) и добавьте пункт меню:

```html 
<li class="nav-item">  
    <a class="nav-link text-dark" asp-area="" asp-page="/Students/Index">Список студентов</a>  
</li>
```

---
## 2. Формы, POST-запросы, PRG и удаление

### Почему в HTML-формах не используются PUT и DELETE?
В Web API мы отправляли запросы с помощью HTTP-клиента, который поддерживает любые глаголы: `GET`, `POST`, `PUT`, `DELETE`.  
Однако стандарт HTML5 для тега `<form method="...">` поддерживает **только два метода: `GET` и `POST`**. Поэтому в Razor Pages операции создания, обновления и удаления выполняются через `POST`-запросы, а разделение логики происходит с помощью разных страниц (`Create`, `Update`) или **именованных обработчиков** (`asp-page-handler="Delete"`).

### Паттерн PRG (Post-Redirect-Get)
Если после успешной отправки формы (`POST`) сервер сразу вернёт HTML-страницу (`return Page()`), то при нажатии клавиши **F5 (Обновить)** браузер повторно отправит `POST`-запрос, и студент создастся дважды.  
**Решение (PRG):**
1. **P (Post):** Браузер отправляет форму методом `POST`.
2. **R (Redirect):** Сервер сохраняет данные и возвращает HTTP 302 (`return RedirectToPage("./Index")`).
3. **G (Get):** Браузер сам делает безопасный `GET`-запрос на страницу списка.

---
### Шаг 2.1. Поиск на странице `Students/Index`
Поиск не меняет данные на сервере, поэтому используется форма с `method="get"`.

**В `Pages/Students/Index.cshtml.cs`:**
```csharp
[BindProperty(SupportsGet = true)] // Разрешаем привязку из Query-строки (?SearchTerm=Mike)
public string? SearchTerm { get; set; }

public async Task OnGetAsync()
{
    Students = await _studentService.GetAllAsync(SearchTerm);
}
```

**В `Pages/Students/Index.cshtml` (добавляем перед таблицей):**
```html
<form method="get">
	<input asp-for="SearchTerm" placeholder="Поиск по имени..." />
	<button type="submit" class="btn btn-secondary">Найти</button>
	<a asp-page="./Index">Сбросить</a>
</form>
```

### Шаг 2.2. Создание студента (`Create.cshtml`)
Создайте новую Razor Page `Create` в папке `Pages/Students`.

**Файл `Pages/Students/Create.cshtml.cs`:**
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentApi.Data.DTOs.Student;
using StudentApi.Data.Services;

namespace StudentRazorPages.Pages.Students;

public class Create : PageModel
{
    private readonly IStudentService _studentService;

    public Create(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [BindProperty]
    public StudentCreateDto Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _studentService.CreateAsync(Input);
        return RedirectToPage("./Index"); // Шаг Redirect паттерна PRG
    }
}
```

**Файл `Pages/Students/Create.cshtml`:**
```html
@page
@model StudentRazorPages.Pages.Students.Create
@{
    ViewData["Title"] = "Добавление студента";
}

<h2>Добавить нового студента</h2>

<form method="post">
    <div>
        <label asp-for="Input.Name">Имя студента</label><br />
        <input asp-for="Input.Name" />
    </div>

    <div>
        <label asp-for="Input.Age">Возраст</label><br />
        <input asp-for="Input.Age" type="number" />
    </div>

    <p>
        <button type="submit" class="btn btn-success">Создать</button>
        <a asp-page="./Index" class="btn btn-secondary">Отмена</a>
    </p>
</form>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
 
```

### Шаг 2.3. Редактирование студента (`Update.cshtml`)

Создайте Razor Page `Update` в папке `Pages/Students`.

**Файл `Pages/Students/Update.cshtml.cs`:**  
```csharp  
using Microsoft.AspNetCore.Mvc;  
using Microsoft.AspNetCore.Mvc.RazorPages;  
using StudentApi.Data.DTOs.Student;  
using StudentApi.Data.Services;

namespace StudentRazorPages.Pages.Students;

public class Update : PageModel  {    
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
```

**Файл `Pages/Students/Update.cshtml`:**  
```html  
@page "{id:int}"  
@model StudentRazorPages.Pages.Students.Update  
@{  
    ViewData["Title"] = "Редактирование студента";  
}  
  
<h2>Редактировать студента</h2>  
  
<form method="post">  
    <div asp-validation-summary="ModelOnly"></div>  
  
    <div>        
	    <label asp-for="Input.Name">Имя студента</label><br />  
        <input asp-for="Input.Name" />  
    </div>  
    <div>        
	    <label asp-for="Input.Age">Возраст</label><br />  
        <input asp-for="Input.Age" type="number" />  
    </div>  
    <p>        
	    <button type="submit" class="btn btn-warning">Сохранить</button>  
        <a asp-page="./Index" class="btn btn-secondary">Отмена</a>  
    </p>
</form>
```

### Шаг 2.4. Удаление студента на странице `Index` (Page Handlers)

Удаление не требует отдельной страницы, но должно происходить через `POST` (удалять через `GET`-ссылку небезопасно). Используем именованный обработчик `OnPostDeleteAsync`.

**В `Pages/Students/Index.cshtml.cs` добавляем метод:**  
``` csharp
public async Task<IActionResult> OnPostDeleteAsync(int id) { await _studentService.DeleteAsync(id); return RedirectToPage(); // Перезагружаем текущую страницу через GET (PRG) }
```

**В `Pages/Students/Index.cshtml` заменяем кнопку «Удалить» на мини-форму:**  
``` html
<form method="post" asp-page-handler="Delete" asp-route-id="@student.Id" class="d-inline" onsubmit="return confirm('Удалить студента @student.Name?');"> <button type="submit" class="btn btn-sm btn-danger">Удалить</button>
</form>
```

---

## 3. Декомпозиция с помощью Partial Views

Вынесем разметку одной строки таблицы в отдельное частичное представление, чтобы разгрузить `Index.cshtml`.

### Шаг 3.1. Создаём `_StudentRow.cshtml`

В Rider в папке `Pages/Students` создайте файл `_StudentRow.cshtml` (имя с нижним подчёркиванием — соглашение для Partial Views; директива `@page` **не ставится**).

```html  
@model StudentApi.Data.DTOs.Student.StudentGetDto

<tr>
    <td>@Model.Id</td>
    <td>@Model.Name</td>
    <td>@Model.Age</td>
    <td>
        <a asp-page="./Details" asp-route-id="@Model.Id" class="btn btn-info">Просмотреть</a>
        <a asp-page="./Update" asp-route-id="@Model.Id" class="btn btn-warning">Обновить</a>
        <form method="post" asp-page-handler="Delete" asp-route-id="@Model.Id" style="display:inline;"
              onsubmit="return confirm('Удалить студента @Model.Name?');">
            <button type="submit" class="btn btn-danger">Удалить</button>
        </form>
    </td>
</tr>


```

### Шаг 3.2. Подключаем в `Pages/Students/Index.cshtml`

Заменяем содержимое цикла `foreach` на Tag Helper `<partial>`:

```html
@foreach (var student in Model.Students)
{
	<partial name="_StudentRow" model="student" />
}
```

---

## Итог
Что мы сегодня изучили:
- Структуру страницы Razor Pages (`.cshtml` + `.cshtml.cs`) и передачу метаданных через `ViewData` в `_Layout.cshtml`.
- Работу с маршрутами (`@page "{id:int}"`) и строкой запроса (`[BindProperty(SupportsGet = true)]`).
- Реализацию CRUD через HTML-формы, именованные обработчики (`asp-page-handler`) и паттерн **PRG**.
- Вынесение повторяющейся разметки в **Partial Views**.

## Частые ошибки
- **Забытый `[BindProperty]`:** Если свойства в `PageModel` не помечены атрибутом `[BindProperty]`, при отправке `POST`-формы они останутся пустыми (`null` или `0`).
- **Отсутствие `@page` или неверный маршрут:** Если забыть `{id:int}` в директиве `@page "{id:int}"`, параметр будет передаваться как `?id=1` вместо `/1`, а при несовпадении имён параметров в `asp-route-...` и в сигнатуре `OnGetAsync(int id)` значение не привяжется.
- **Директива `@page` в Partial View:** Если случайно добавить `@page` в `_StudentRow.cshtml`, Razor начнёт считать его отдельной страницей, и передача модели через `<partial model="..." />` сломается.

## ❓ Вопросы для самопроверки
1. **Зачем нужна директива `@page` в начале файла `.cshtml`?**  
    _(Ответ: Она превращает файл в самостоятельную страницу Razor Pages, способную обрабатывать HTTP-запросы напрямую, без контроллера. В Partial Views эта директива не ставится)._
2. **Почему для удаления студента мы обернули кнопку в `<form method="post">`, а не сделали обычную ссылку `<a asp-page-handler="Delete">`?**  
    _(Ответ: Обычные ссылки отправляют GET-запрос. Менять или удалять данные по GET-запросу небезопасно — например, поисковый бот или предзагрузка браузера могут случайно удалить данные, плюс не сработает встроенная защита от CSRF-атак)._
3. **В чём суть паттерна PRG и какую проблему он решает?**  
    _(Ответ: Post-Redirect-Get. После успешной обработки POST-формы сервер делает Redirect (302) на GET-страницу. Это предотвращает повторную отправку формы и дублирование записей в БД, если пользователь нажмёт F5)._
4. **Как передать параметр `id` при переходе по ссылке на страницу `Details`?**  
    _(Ответ: Использовать Tag Helper `asp-route-id="@student.Id"`, а на целевой странице указать шаблон маршрута `@page "{id:int}"` и принять `int id` в параметрах метода `OnGetAsync`)._

---

➡️ **Перейти к выполнению самостоятельной работы:** [Инструкция к практическому заданию (PRACTICE.md)](./PRACTICE.md)