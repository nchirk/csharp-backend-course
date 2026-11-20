# Лекция 9 — Razor Pages: вывод связанных сущностей, валидация, bootstrap.

**Цели занятия:**
- Изучить способы отображения связанных сущностей в Razor Pages.
- Познакомиться с механизмами валидации данных в ASP.NET Core.
- Освоить основные возможности Bootstrap для оформления пользовательского интерфейса.

## 1. Связанные сущности: Group в Details, Create и Update

Вводить `GroupId` вручную числом неудобно и чревато ошибками внешнего ключа. Сделаем отображение названия группы в `Details` и выпадающий список (`<select>`) в `Create` и `Update`.

### Шаг 1.1. Название группы в `Details` и `Index`

**В `Pages/Students/Details.cshtml`:** добавляем название группы в Details аналогично другим полям.

```html  
<div>
    <p><strong>ID:</strong> @Model.Student.Id</p>
    <p><strong>Имя:</strong> @Model.Student.Name</p>
    <p><strong>Возраст:</strong> @Model.Student.Age лет</p>
    <p><strong>Группа:</strong> @Model.Student.GroupName</p>
</div>
```  

**В `Pages/Students/Index.cshtml`:** добавляем название группы в Index аналогично другим полям.

```html  
<tr>
    <th>ID</th>
    <th>Имя</th>
    <th>Возраст</th>
    <th>Группа</th>
    <th>Действия</th>
</tr>
```  
**`Pages/Students/_StudentRow.cshtml`**:

```html
<tr>
    <td>@Model.Id</td>
    <td>@Model.Name</td>
    <td>@Model.Age</td>
    <td>@Model.GroupName</td>
```

### Шаг 1.2. Выпадающий список `<select>` в `Create` и `Update`

Добавляем `<select>` на форму в **`Pages/Students/Create.cshtml` (аналогично для `Update.cshtml`).** Посмотрите, как отдельно задается опция для пустой группы через `<option>`:

```html
<div>
    <label asp-for="Input.GroupId">Группа</label><br />
    <select asp-for="Input.GroupId" asp-items="Model.GroupList">
        <option value="">-- Выберите группу --</option>
    </select>
</div>
```

Теперь нам нужно сформировать список `Model.GroupList`. Добавляем `IGroupService` из `StudentApi.Data.Services` через конструктор и используем метод `GetAllAsync()`. Предзагружаем список групп в момент получения страницы (`OnGet`).

**В `Pages/Students/Create.cshtml.cs` (аналогично для `Update.cshtml.cs`):**  
```csharp  
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
```

Убедитесь, что в `Program.cs` настроено DI для `IGroupService`:
```csharp
var builder = WebApplication.CreateBuilder(args);

//другой код...

//обязательно ПОСЛЕ создания builder и ДО вызова AddRazorPages
builder.Services.AddScoped<IGroupService, GroupService>();

//другой код...

// Add services to the container.
builder.Services.AddRazorPages();
```

## 2. Валидация данных в Razor Pages

Валидация гарантирует, что пользователь ввёл корректные данные перед тем, как приложение обратится к базе данных или бизнес-логике.

Правила валидации настраиваются через **Атрибуты валидации (Data Annotations).** Они задаются прямо в классах DTO или моделях (`[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`) и определяют правила и текст ошибок.

**Популярные атрибуты валидации `System.ComponentModel.DataAnnotations`:**

| Атрибут | Назначение | Пример использования |
| :--- | :--- | :--- |
| `[Required]` | Поле обязательно для заполнения (не `null` и не пустая строка). | `[Required(ErrorMessage = "Укажите имя")]` |
| `[StringLength]` | Задаёт максимальную (и опционально минимальную) длину строки. | `[StringLength(50, MinimumLength = 2)]` |
| `[Range]` | Проверяет попадание числа или даты в заданный диапазон. | `[Range(16, 100, ErrorMessage = "Возраст от 16 до 100")]` |
| `[EmailAddress]` | Проверяет, что строка соответствует формату электронной почты. | `[EmailAddress(ErrorMessage = "Некорректный Email")]` |
| `[Phone]` | Проверяет формат телефонного номера. | `[Phone(ErrorMessage = "Неверный формат телефона")]` |
| `[RegularExpression]` | Проверяет соответствие строки регулярному выражению. | `[RegularExpression(@"^[А-Яа-яЁё]+$", ErrorMessage = "Только кириллица")]` |
| `[Compare]` | Сравнивает значение с другим свойством модели (часто для паролей). | `[Compare("Password", ErrorMessage = "Пароли не совпадают")]` |

В ASP.NET Core валидация работает на двух уровнях:
1. **Серверная проверка (`ModelState`):** При отправке формы ASP.NET Core автоматически проверяет входные данные по правилам атрибутов и записывает результат в словарь `ModelState`.
   * `ModelState.IsValid` возвращает `true`, если ошибок нет, и `false`, если хотя бы одно правило нарушено.
   * `ModelState.AddModelError("Поле", "Текст ошибки")` позволяет вручную добавить ошибку бизнес-логики (например, «Такой студент уже существует»).
2. **Клиентская проверка (Unobtrusive Validation):** Подключение частичного представления `_ValidationScriptsPartial` (библиотеки jQuery Validation) позволяет браузеру проверять поля «на лету» без перезагрузки страницы, используя те же атрибуты из C#-класса.

Желательно выполнять валидацию на обоих уровнях: на уровне клиента она быстрее и позволяет проверить форму без отправки, а на уровне сервера помогает убедиться, что скрипты на клиенте корректно отработали, и мы точно запишем корректные данные.

Для вывода ошибок применяют **Tag Helpers:**
   * `asp-validation-for="Input.Name"` — выводит текст ошибки под конкретным полем ввода.
   * `asp-validation-summary="ModelOnly"` (или `"All"`) — выводит общий список ошибок всей формы.

Добавим валидацию на наши страницы.

### Шаг 1: Настройка правил в DTO
Чтобы валидация заработала на страницах, в DTO должны быть расставлены атрибуты:

```csharp
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
```
*(Аналогичные атрибуты задаются в `StudentUpdateDto`).*

### Шаг 2: Страница `Create` (Создание студента)
Здесь нужна полная валидация: проверка обязательных полей, вывод ошибок рядом с инпутами и подключение клиентских скриптов.

**`Pages/Students/Create.cshtml.cs`:**
```csharp
public async Task<IActionResult> OnPostAsync()
{
    // 1. Проверяем состояние модели на сервере
    if (!ModelState.IsValid)
    {
        // Обязательно заново загружаем список групп для <select>, иначе страница упадёт
        await LoadGroupsAsync();
        return Page();
    }

    await _studentService.CreateAsync(Input);
    return RedirectToPage("./Index");
}
```

**`Pages/Students/Create.cshtml`:**
```html
@page
@model StudentRazorPages.Pages.Students.Create

@{
    ViewData["Title"] = "Добавить нового студента";
}

<h2>Добавить нового студента</h2>

<form method="post">
    <!-- Сводка общих ошибок формы -->
    <div asp-validation-summary="ModelOnly"></div>

    <div>
        <label asp-for="Input.Name">Имя студента</label><br/>
        <input asp-for="Input.Name"/>
        <!-- Сообщение об ошибке для поля Name -->
        <span asp-validation-for="Input.Name"></span>
    </div>

    <div>
        <label asp-for="Input.Age">Возраст</label><br/>
        <input asp-for="Input.Age" type="number"/>
        <!-- Сообщение об ошибке для поля Age -->
        <span asp-validation-for="Input.Age"></span>
    </div>

    <div>
        <label asp-for="Input.GroupId">Группа</label><br/>
        <select asp-for="Input.GroupId" asp-items="Model.GroupList">
            <option value="">-- Выберите группу --</option>
        </select>
    </div>

    <p>
        <button type="submit" class="btn btn-success">Создать</button>
        <a asp-page="./Index" class="btn btn-secondary">Отмена</a>
    </p>
</form>

<!-- Подключаем клиентскую валидацию -->
@section Scripts
{
    <partial name="Shared/_ValidationScriptsPartial"/>
}
```

> [!INFO]
>**Подробнее про @section и клиентскую валидацию**
>* **`@section Scripts`** — переносит JS-код с конкретной страницы в самый низ общего шаблона (`_Layout.cshtml`). Это гарантирует, что скрипты страницы загрузятся **после** jQuery и не будут грузиться там, где они не нужны.
>    В самом низу перед закрывающим тегом `</body>` должен быть слот для секции (обычно он уже добавлен в стандартный шаблон):
>
>    ```html
>    <script src="~/lib/jquery/dist/jquery.min.js"></script>
>    <script src="~/js/site.js"></script>
>
>    @await RenderSectionAsync("Scripts", required: false)
>    </body>
>    ```
>* **`_ValidationScriptsPartial`** — встроенное частичное представление (`Views/Shared/_ValidationScriptsPartial.cshtml`), которое подключает `jquery.validate` и включает проверку форм прямо в браузере без перезагрузки страницы (на основе атрибутов `[Required]`, `[Range]` и др.).

### Шаг 3: Страница `Update` (Редактирование студента)
Логика аналогична `Create`, но добавим пример **ручной ошибки валидации** (`ModelState.AddModelError`), если при попытке сохранения выяснилось, что запись уже удалена или не найдена.

**`Pages/Students/Update.cshtml.cs`:**
```csharp
public async Task<IActionResult> OnPostAsync(int id)
{
    if (!ModelState.IsValid)
    {
        await LoadGroupsAsync();
        return Page();
    }

    var updated = await _studentService.UpdateAsync(id, Input);
    if (!updated)
    {
        // Добавляем ошибку уровня модели (она отобразится в asp-validation-summary)
        ModelState.AddModelError(string.Empty, "Не удалось обновить данные. Возможно, студент был удалён.");
        await LoadGroupsAsync();
        return Page();
    }

    return RedirectToPage("./Index");
}
```

**`Pages/Students/Update.cshtml`:**
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
        <span asp-validation-for="Input.Name"></span>
    </div>

    <div>
        <label asp-for="Input.Age">Возраст</label><br />
        <input asp-for="Input.Age" type="number" />
        <span asp-validation-for="Input.Age"></span>
    </div>
    
    <div>
        <label asp-for="Input.GroupId">Группа</label><br />
        <select asp-for="Input.GroupId" asp-items="Model.GroupList">
            <option value="">-- Выберите группу --</option>
        </select>
    </div>

    <p>
        <button type="submit" class="btn btn-warning">Сохранить</button>
        <a asp-page="./Index" class="btn btn-secondary">Отмена</a>
    </p>
</form>

@section Scripts
{
    <partial name="Shared/_ValidationScriptsPartial" />
}
```

### Шаг 4: Страница `Index` (Валидация строки поиска и удаление)
На главной странице нет форм редактирования студентов, но есть **строка поиска (`SearchTerm`)** и **кнопка удаления**.
* Для `SearchTerm` можно задать ограничение длины (`[StringLength]`), чтобы пользователь не отправил слишком длинный запрос.
* При удалении (`OnPostDeleteAsync`) нужно обработать ситуацию, если студент уже не существует в БД, и передать сообщение об ошибке через `[TempData]`.

> [!INFO]
>**Почему `ModelState` не работает при редиректе (`Delete`), а нужен `[TempData]`?**
>
>**`TempData`** — это специальное временное хранилище в ASP.NET Core (работающее поверх cookie или сессии), которое предназначено для передачи данных **между двумя последовательными HTTP-запросами**. Главная особенность `TempData` в том, что данные в нём **автоматически удаляются сразу после того, как их прочитали**.
>
>Это напрямую связано с работой `ModelState` и паттерном PRG:
>* `ModelState` живёт ровно **один** HTTP-запрос.
>* Когда сервер вызывает `RedirectToPage()`, текущий `POST`-запрос завершается, а браузер делает новый чистый `GET`-запрос с пустым `ModelState`.
>* Если записать ошибку в `ModelState` перед редиректом, она пропадёт. А свойство с атрибутом `[TempData]` работает как «одноразовый чемоданчик»: оно переживает редирект, выводит сообщение пользователю на новой странице и тут же очищается (при повторном нажатии `F5` сообщение исчезнет).
>
>| Сценарий | Как возвращаем ответ? | Сколько HTTP-запросов? | Где хранить ошибку? |
>| :--- | :--- | :--- | :--- |
>| **Ошибка в форме (`Create`, `Update`)** | `return Page()` | **1 запрос** (остаёмся в POST) | `ModelState` (живёт внутри текущего запроса) |
>| **Ошибка после действия (`Delete` и др.)** | `return RedirectToPage()` | **2 запроса** (POST → 302 → GET) | `[TempData]` (переживает один редирект и затем удаляется) |


**`Pages/Students/Index.cshtml.cs`:**
```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
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
    
    [BindProperty(SupportsGet = true)] // Разрешаем привязку из Query-строки (?SearchTerm=Mike)
    [StringLength(200, ErrorMessage = "Поисковый запрос не должен превышать 200 символов")]
    public string? SearchTerm { get; set; }

    [TempData] public string? DeleteErrorMessage { get; set; }
    
    // Свойства, в которые мы запишем данные. 
    // Страница .cshtml сможет обратиться к ним напрямую через @Model.Students и @Model.TotalCountMessage
    public IEnumerable<StudentGetDto> Students { get; set; } = [];
    public string TotalCountMessage => $"Всего студентов в базе: {Students.Count()}";

    // Обработчик HTTP GET запроса
    public async Task<IActionResult> OnGetAsync()
    {
        if (!ModelState.IsValid)
        {
            Students = [];
            return Page();
        }
        // Напрямую берем данные из сервиса. 
        Students = await _studentService.GetAllAsync(SearchTerm);
        return Page();
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(int id) {
        var success = await _studentService.DeleteAsync(id);
        if (!success)
        {
            DeleteErrorMessage = "Не удалось удалить студента, возможно он был уже удален.";
        }
        return RedirectToPage(); // Перезагружаем текущую страницу через GET (PRG)
    }
}
```

**`Pages/Students/Index.cshtml`:**
```html
@page
@model StudentRazorPages.Pages.Students.Index
@{
    ViewData["Title"] = "Список студентов";
}

<h2>Список студентов</h2>

@if (!string.IsNullOrEmpty(Model.DeleteErrorMessage))
{
    <p><strong>@Model.DeleteErrorMessage</strong></p>
}

<p>
    <a asp-page="./Create" class="btn btn-primary">+ Добавить студента</a>
</p>

<form method="get">
    <input asp-for="SearchTerm" placeholder="Поиск по имени..." />
    <span asp-validation-for="SearchTerm"></span>
    <button type="submit" class="btn btn-secondary">Найти</button>
    <a asp-page="./Index">Сбросить</a>
</form>

<p>@Model.TotalCountMessage</p>

<table class="table">
    <thead>
    <tr>
        <th>ID</th>
        <th>Имя</th>
        <th>Возраст</th>
        <th>Группа</th>
        <th>Действия</th>
    </tr>
    </thead>
    <tbody>
    @foreach (var student in Model.Students)
    {
        <partial name="_StudentRow" model="student"></partial>
    }
    </tbody>
</table>

@section Scripts{
    <partial name="Shared/_ValidationScriptsPartial"/>
}
```

### Шаг 5:  Страница `Details` (Валидация параметров маршрута)
На странице `Details` нет HTML-форм, поэтому атрибуты валидации DTO здесь не применяются. Валидация сводится к **проверке входного параметра маршрута (`id`)**:
1. Ограничение типа прямо в директиве маршрута: `@page "{id:int}"` (если ввести `/Students/Details/abc`, сервер сразу вернёт `404 Not Found`, даже не запуская метод `OnGetAsync`).
2. Проверка существования записи в БД: `if (student is null) return NotFound();`.

## 3. Использование Bootstrap

**Bootstrap** — это готовый набор CSS-стилей и JavaScript-компонентов, который позволяет быстро создавать красивые и адаптивные сайты, не прописывая весь CSS самостоятельно.

В **ASP.NET Core Razor Pages** Bootstrap используется точно так же, как в обычном HTML: добавляем классы Bootstrap к HTML-элементам.

<details>
  <summary><strong>В стандартном проекте ASP.NET Core Razor Pages Bootstrap часто уже подключён.</strong></summary>

>В папке со статическими элементами сайта `wwwroot` есть скрипты Bootstrap:
>```
>wwwroot/
>    lib/
>        bootstrap/
>            dist/
>                css/
>                    bootstrap.min.css
>                js/
>                    bootstrap.bundle.min.js
>```
>
>В файле `Pages/Shared/_Layout.cshtml` подключен CSS:
>``` html
><link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
>```
>
>А перед закрывающим `</body>`:
>```html
><script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
>```
</details>

---
Чтобы сделать страницы аккуратными, достаточно знать **5 базовых групп классов**:

### 1. Сетка и выравнивание (Grid & Flex)
* `container` — главный обёрточный класс Bootstrap. Он ограничивает максимальную ширину страницы, центрирует её по горизонтали и добавляет боковые отступы. **Важно:** в Razor Pages он уже прописан в `_Layout.cshtml` вокруг `@RenderBody()`, поэтому на отдельных страницах его дублировать не нужно. (Если нужен блок на 100% ширины экрана, используют `container-fluid`).
* `row` и `col-*` — 12-колоночная сетка внутри контейнера (работает только внутри `container` или `container-fluid`). Например, `col-md-6` означает «занять половину ширины (6 из 12 колонок) на средних и больших экранах».
* `d-flex` — включает Flexbox-контейнер (элементы выстраиваются в строку).
* `justify-content-between` — раздвигает элементы по краям строки (например, заголовок слева, кнопка справа).
* `align-items-center` — выравнивает элементы по вертикальному центру.


#### Как устроены `.container`, `.row` и `.col-*` (12-колоночная сетка):
```
Окно браузера (100% ширины)
┌─────────────────────────────────────────────────────────────────────────┐
│  отступ  │                  .container (по центру)           │  отступ  │
│          │ ┌───────────────────────────────────────────────┐ │          │
│          │ │ .row (всегда делится на 12 условных колонок)  │ │          │
│          │ │ ┌───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┤          │
│          │ │ │ 1 │ 2 │ 3 │ 4 │ 5 │ 6 │ 7 │ 8 │ 9 │10 │11 │12 │          │
│          │ │ └───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┤          │
│          │ │ │       .col-md-6       │       .col-md-6       │          │
│          │ │ │   (половина ширины)   │   (половина ширины)   │          │
│          │ │ └───────────────────────┴───────────────────────┘          │
│          │ └───────────────────────────────────────────────┘ │          │
└─────────────────────────────────────────────────────────────────────────┘
```

#### Как работает `.d-flex` с выравниванием:
```
Без d-flex (обычные блочные <div> идут друг под другом):
┌──────────────────────────────────────┐
│ <h2>Список студентов</h2>            │
├──────────────────────────────────────┤
│ <a class="btn">+ Добавить</a>        │
└──────────────────────────────────────┘

С классом .d-flex .justify-content-between .align-items-center:
┌──────────────────────────────────────┐
│ <h2>Список студентов</h2>   [+ Доб.] │  <-- элементы в одну строку,
└──────────────────────────────────────┘      разведены по краям и по центру

```

### 2. Отступы (Spacing)
Формула класса: `{свойство}{сторона}-{размер}` (размер от `0` до `5`):
* **Свойство:** `m` — внешний отступ (`margin`), `p` — внутренний отступ (`padding`).
* **Сторона:** `t` (верх), `b` (низ), `s` (лево), `e` (право), `x` (лево + право), `y` (верх + низ).

*Примеры:* `mb-3` (отступ снизу), `mt-4` (отступ сверху), `p-3` (внутренний отступ со всех сторон).

* `g-*` (от слова *gutter* — промежуток, значения от `0` до `5`) — задаёт **расстояние между элементами внутри `.row` или `.d-flex`**, чтобы они не слипались друг с другом:
  * `g-2` — одинаковый промежуток по вертикали и горизонтали;
  * `gx-2` — промежуток только по горизонтали (между колонками);
  * `gy-2` — промежуток только по вертикали (при переносе строк).
  *(Это удобнее, чем вешать `me-2` на каждую кнопку, так как `g-2` ставит отступы строго **между** элементами, не оставляя пустого места у последнего элемента).*


#### Внешний отступ (`margin` — `m*`) vs Внутренний отступ (`padding` — `p*`):
```

               mt-3 (внешний отступ сверху)
                     │
                     ▼
        ┌─────────────────────────┐  <-- граница элемента (border)
        │          pt-3           │
        │        (внутри)         │
        │  ┌───────────────────┐  │
  ms-3  │  │                   │  │
  ───►  │  │  КОНТЕНТ БЛОКА    │  │ ◄─── pe-3 (внутри справа)
(снаружи│  │  (текст / кнопка) │  │
 слева) │  │                   │  │
        │  └───────────────────┘  │
        │          pb-3           │
        └─────────────────────────┘
                     ▲
                     │
               mb-3 (внешний отступ снизу)

Направления:
  t = top (верх)      b = bottom (низ)
  s = start (лево)    e = end (право)
  x = s + e (горизонталь)   y = t + b (вертикаль)
```

#### Как работают `.d-flex` и `.g-2`:

```
Без g-2 (элементы внутри .row или .d-flex слиплись):
┌───────────────────┬───────┬──────────┐
│ Поиск по имени... │ Найти │ Сбросить │
└───────────────────┴───────┴──────────┘

С классом .row.g-2 (аккуратные промежутки строго между элементами):
┌───────────────────┐  ┌───────┐  ┌──────────┐
│ Поиск по имени... │  │ Найти │  │ Сбросить │
└───────────────────┘  └───────┘  └──────────┘
                     ▲          ▲
                    g-2        g-2
```

### 3. Кнопки и цвета (Buttons & Colors)
Bootstrap использует семантическую палитру цветов:
* `primary` (синий — основное действие)
* `success` (зелёный — создание, сохранение)
* `warning` (жёлтый — редактирование)
* `danger` (красный — удаление, ошибка)
* `info` (голубой — просмотр деталей)
* `secondary` (серый — отмена, назад)

Классы кнопок: `btn btn-primary`, `btn btn-outline-secondary` (кнопка с рамкой без заливки), `btn-sm` (уменьшенная кнопка, например, для таблиц), `btn-lg` (увеличенная кнопка).

### 4. Таблицы и карточки (Tables & Cards)
* `table` — базовая таблица.
* `table-striped` — «зебра» (чередование цвета строк).
* `table-hover` — подсветка строки при наведении курсора.
* `align-middle` — выравнивание текста в ячейках по вертикали по центру.
* `card` + `card-body` — аккуратный блок с рамкой и внутренними отступами (например, для страницы `Details`).

#### Как классы преображают обычную HTML-таблицу:
```
Без классов (голый <table>):
ID Имя Возраст Действия
1 Иван 19 Просмотреть Обновить Удалить

С классами .table .table-striped .table-hover .align-middle:
┌─────┬──────────────┬─────────┬───────────────────────────────┐
│ ID  │ Имя          │ Возраст │ Действия (.text-end)          │ ◄── thead.table-dark
├─────┼──────────────┼─────────┼───────────────────────────────┤     (тёмная шапка)
│ 1   │ Иван Иванов  │ 19      │ [Просм.] [Обнов.] [Удалить]   │ ◄── белый фон
├─────┼──────────────┼─────────┼───────────────────────────────┤
│ 2   │ Анна Смирнова│ 20      │ [Просм.] [Обнов.] [Удалить]   │ ◄── серый фон (.table-striped)
├─────┼──────────────┼─────────┼───────────────────────────────┤
│ 3   │ Пётр Сидоров │ 18      │ [Просм.] [Обнов.] [Удалить]   │ ◄── подсвечивается при наведении
└─────┴──────────────┴─────────┴───────────────────────────────┘     мыши (.table-hover)
  ▲
  └── .align-middle: текст выровнен по вертикальному центру относительно высоких кнопок
```

#### Из чего состоит карточка Bootstrap (для страницы Details):
```
┌──────────────────────────────────────────┐ ◄── .card (внешняя рамка со
│  Студент #1                              │     скруглёнными углами)
├──────────────────────────────────────────┤ ◄── .card-header (необязательная
│                                          │     шапка с серым фоном)
│  Иван Иванов (h5.card-title)             │
│                                          │ ◄── .card-body (добавляет
│  Возраст: 19 лет (p.card-text)           │     внутренние отступы padding,
│  Группа: ИС-21 (p.card-text)             │     чтобы текст не лип к рамке)
│                                          │
│  [ Редактировать ]  [ Назад к списку ]   │
│                                          │
└──────────────────────────────────────────┘
```

### 5. Формы и уведомления (Forms & Alerts)
Позволяют сделать красивые формы.
* `form-label` — стиль подписи `<label>` над полем ввода.
* `form-control` — превращает обычный `<input>` в красивое поле на всю ширину блока.
* `form-select` — стиль для выпадающего списка `<select>`.
* `text-danger` — красный цвет текста (например, для сообщений об ошибках валидации `<span>`).
* `alert alert-danger` / `alert alert-success` — плашка уведомления (например, для вывода ошибок из `TempData`).

---

Отформатируем наши страницы.

### Страница 1: `Pages/Students/_StudentRow.cshtml` и `Index.cshtml`
**Что улучшаем:**
1. Выравниваем заголовок и кнопку «Добавить» в одну линию (`d-flex justify-content-between`).
2. Оформляем строку поиска с помощью сетки `row g-2` (аккуратные промежутки между полем и кнопками).
3. Добавляем таблице полосатость (`table-striped`), подсветку при наведении (`table-hover`) и тёмную шапку (`table-dark`).
4. Делаем кнопки в таблице компактными (`btn-sm`) и прижимаем их к правому краю (`text-end`).

**Обновлённый `Pages/Students/_StudentRow.cshtml`:**
```html
@model StudentApi.Data.DTOs.Student.StudentGetDto

<tr>
    <td>@Model.Id</td>
    <td>@Model.Name</td>
    <td>@Model.Age</td>
    <td>@Model.GroupName</td>
    <td class="text-end">
        <a asp-page="./Details" asp-route-id="@Model.Id" class="btn btn-sm btn-info">Просмотреть</a>
        <a asp-page="./Update" asp-route-id="@Model.Id" class="btn btn-sm btn-warning">Обновить</a>
        <form method="post" asp-page-handler="Delete" asp-route-id="@Model.Id" class="d-inline"
              onsubmit="return confirm('Удалить студента @Model.Name?');">
            <button type="submit" class="btn btn-sm btn-danger">Удалить</button>
        </form>
    </td>
</tr>
```

**Обновлённый `Pages/Students/Index.cshtml`:**
```html
@page
@model StudentRazorPages.Pages.Students.Index
@{
    ViewData["Title"] = "Список студентов";
}

<div class="d-flex justify-content-between align-items-center">
    <h2>Список студентов</h2>
    <a asp-page="./Create" class="btn btn-primary">+ Добавить студента</a>
</div>

@if (!string.IsNullOrEmpty(Model.DeleteErrorMessage))
{
    <p class="alert alert-danger">@Model.DeleteErrorMessage</p>
}

<form method="get" class="row g-2 mb-3">
    <div class="col-auto">
        <input asp-for="SearchTerm" class="form-control" placeholder="Поиск по имени..." />
        <span asp-validation-for="SearchTerm" class="text-danger"></span>
    </div>
    <div class="col-auto">
        <button type="submit" class="btn btn-secondary">Найти</button>
        <a asp-page="./Index" class="btn btn-outline-secondary">Сбросить</a>
    </div>
</form>

<p class="text-muted">@Model.TotalCountMessage</p>

<table class="table table-striped table-hover align-middle">
    <thead class="table-dark">
    <tr>
        <th>ID</th>
        <th>Имя</th>
        <th>Возраст</th>
        <th>Группа</th>
        <th class="text-end">Действия</th>
    </tr>
    </thead>
    <tbody>
    @foreach (var student in Model.Students)
    {
        <partial name="_StudentRow" model="student" />
    }
    </tbody>
</table>

@section Scripts{
    <partial name="Shared/_ValidationScriptsPartial"/>
}
```

### Страница 2: `Pages/Students/Details.cshtml`
**Что улучшаем:**
Оборачиваем информацию о студенте в компонент `card` и ограничиваем её ширину через сетку (`col-md-6`), чтобы карточка не растягивалась на весь широкий монитор.

**Обновлённый `Pages/Students/Details.cshtml`:**
```html
@page "{id:int}"
@model StudentRazorPages.Pages.Students.Details
@{
    ViewData["Title"] = "Карточка студента";
}

<h2 class="mb-3">Информация о студенте</h2>

<div class="row">
    <div class="col-md-6">
        <div class="card">
            <div class="card-header">
                Студент #@Model.Student.Id
            </div>
            <div class="card-body">
                <h5 class="card-title mb-3">@Model.Student.Name</h5>
                <p class="card-text"><strong>Возраст:</strong> @Model.Student.Age лет</p>
                <p class="card-text"><strong>Группа:</strong> @Model.Student.GroupName</p>

                <div class="mt-4">
                    <a asp-page="./Update" asp-route-id="@Model.Student.Id" class="btn btn-warning">Редактировать</a>
                    <a asp-page="./Index" class="btn btn-secondary">Назад к списку</a>
                </div>
            </div>
        </div>
    </div>
</div>
```

### Страница 3: `Pages/Students/Create.cshtml`
**Что улучшаем:**
1. Ограничиваем ширину формы (`col-md-5`), чтобы поля ввода не были слишком длинными.
2. Убираем теги `<br />` — вместо них используем блочные классы `form-label`, `form-control`, `form-select` и отступы `mb-3`.
3. Окрашиваем ошибки валидации в красный цвет с помощью `text-danger`.

**Обновлённый `Pages/Students/Create.cshtml`:**
```html
@page
@model StudentRazorPages.Pages.Students.Create

@{
    ViewData["Title"] = "Добавить нового студента";
}

<h2 class="mb-3">Добавить нового студента</h2>

<div class="row">
    <div class="col-md-5">
        <form method="post">
            <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>

            <div class="mb-3">
                <label asp-for="Input.Name" class="form-label">Имя студента</label>
                <input asp-for="Input.Name" class="form-control" />
                <span asp-validation-for="Input.Name" class="text-danger"></span>
            </div>
            
            <div class="mb-3">
                <label asp-for="Input.Age" class="form-label">Возраст</label><br/>
                <input asp-for="Input.Age" type="number" class="form-control"/>
                <!-- Сообщение об ошибке для поля Age -->
                <span asp-validation-for="Input.Age" class="text-danger"></span>
            </div>

            <div class="mb-3">
                <label asp-for="Input.GroupId" class="form-label">Группа</label>
                <select asp-for="Input.GroupId" asp-items="Model.GroupList" class="form-select">
                    <option value="">-- Выберите группу --</option>
                </select>
                <span asp-validation-for="Input.GroupId" class="text-danger"></span>
            </div>

            <div class="mt-4">
                <button type="submit" class="btn btn-success">Создать</button>
                <a asp-page="./Index" class="btn btn-secondary">Отмена</a>
            </div>
        </form>
    </div>
</div>

<!-- Подключаем клиентскую валидацию -->
@section Scripts
{
    <partial name="Shared/_ValidationScriptsPartial"/>
}
```

---

### Страница 4: `Pages/Students/Update.cshtml`
**Что улучшаем:**
Оформляем форму редактирования в едином стиле со страницей создания (`row` + `col-md-5`, `form-control`, `form-select`, `mb-3`).

**Обновлённый `Pages/Students/Update.cshtml`:**
```html
@page "{id:int}"
@model StudentRazorPages.Pages.Students.Update
@{
    ViewData["Title"] = "Редактирование студента";
}

<h2 class="mb-3">Редактировать студента</h2>

<div class="row">
    <div class="col-md-5">
        <form method="post">
            <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>

            <div class="mb-3">
                <label asp-for="Input.Name" class="form-label">Имя студента</label>
                <input asp-for="Input.Name" class="form-control" />
                <span asp-validation-for="Input.Name" class="text-danger"></span>
            </div>
            
            <div class="mb-3">
                <label asp-for="Input.Age" class="form-label">Возраст</label><br />
                <input asp-for="Input.Age" type="number" class="form-control" />
                <span asp-validation-for="Input.Age" class="text-danger"></span>
            </div>

            <div class="mb-3">
                <label asp-for="Input.GroupId" class="form-label">Группа</label>
                <select asp-for="Input.GroupId" asp-items="Model.GroupList" class="form-select">
                    <option value="">-- Выберите группу --</option>
                </select>
                <span asp-validation-for="Input.GroupId" class="text-danger"></span>
            </div>

            <div class="mt-4">
                <button type="submit" class="btn btn-warning">Сохранить</button>
                <a asp-page="./Index" class="btn btn-secondary">Отмена</a>
            </div>
        </form>
    </div>
</div>

@section Scripts
{
    <partial name="Shared/_ValidationScriptsPartial" />
}
```

## Итог
Что мы сегодня изучили:
1. Реализовали отображение связанных сущностей и выпадающие списки для выбора группы при создании и редактировании студента.
2. Настроили валидацию данных с помощью Data Annotations, `ModelState` и клиентских скриптов.
3. Рассмотрели обработку ошибок, использование `ModelState.AddModelError`, `TempData` и проверку параметров маршрута.
4. Изучили и применили Bootstrap для оформления таблиц, карточек, форм, кнопок и элементов управления.

## ❓ Вопросы для самопроверки
1. **Зачем при создании `SelectListItem` мы передаём два параметра (`new SelectListItem(x.Name, x.Id.ToString())`) и какой из них отправится на сервер при выборе пункта в `<select>`?**  
   *(Ответ: Первый параметр `x.Name` — это видимый пользователю текст в выпадающем списке (`Text`), а второй `x.Id.ToString()` — это скрытое значение (`Value`). При отправке формы на сервер в свойство `Input.GroupId` запишется именно `Value`, то есть числовой ID группы).*

2. **Что произойдёт при открытии страницы `Create` или `Update`, если забыть зарегистрировать `IGroupService` через `builder.Services.AddScoped<IGroupService, GroupService>()` в `Program.cs`?**  
   *(Ответ: Приложение выбросит исключение времени выполнения `InvalidOperationException` о том, что встроенный DI-контейнер не смог разрешить зависимость `IGroupService` при создании экземпляра модели страницы `Create` / `Update`).*

3. **Зачем нужна серверная проверка `if (!ModelState.IsValid)`, если на странице уже подключены скрипты клиентской валидации (`_ValidationScriptsPartial`)?**  
   *(Ответ: Клиентская валидация работает в браузере только для удобства пользователя и легко обходится — например, через DevTools, Postman или при отключённом JavaScript. Серверная проверка обязательна для безопасности и целостности данных).*
4. **В чём разница между `asp-validation-for` и `asp-validation-summary`?**  
   *(Ответ: `asp-validation-for` выводит ошибку для одного конкретного свойства модели рядом с его полем ввода, а `asp-validation-summary` выводит сводный список ошибок всей формы или общих ошибок модели, добавленных через `ModelState.AddModelError(string.Empty, ...)`).*
5. **Какая разница между значениями `"All"` и `"ModelOnly"` в атрибуте `asp-validation-summary`?**  
   *(Ответ: `"All"` выводит вообще все ошибки (и ошибки конкретных полей, и общие ошибки модели), а `"ModelOnly"` показывает только общие ошибки уровня модели, не дублируя ошибки полей, которые уже выведены через `asp-validation-for`).*
6. **Как валидировать параметры, передаваемые в URL (например, ID страницы `Details`), ещё до запуска метода `OnGetAsync`?**  
   *(Ответ: Использовать ограничения маршрута (Route Constraints) в директиве `@page`, например `@page "{id:int:min(1)}"`. Если параметр не подходит под условие, ASP.NET Core сразу вернёт статус `404 Not Found`).*

7. **Почему в файлах страниц (`Index.cshtml`, `Create.cshtml`) мы не добавляли `<div class="container">`, хотя сетка `row` и `col-*` работала корректно?**  
   *(Ответ: Класс `.container` уже прописан один раз в общем макете `Pages/Shared/_Layout.cshtml` вокруг `@RenderBody()`, а вкладывать один `.container` в другой в Bootstrap нельзя).*

8. **Сколько всего колонок в строке (`.row`) сетки Bootstrap, и какую часть ширины займёт блок `col-md-4`?**  
   *(Ответ: Всегда 12 колонок. Блок `col-md-4` займёт одну треть ширины контейнера — 4 из 12 колонок).*

9. **В чём разница между `mb-3`, `pb-3` и `g-2`?**  
   *(Ответ: `mb-3` — внешний отступ снизу (`margin`), `pb-3` — внутренний отступ снизу (`padding`), а `g-2` (gutter) — промежуток строго **между** элементами внутри `.row` или `.d-flex`).*

10. **Для чего в компоненте карточки Bootstrap нужен вложенный блок `<div class="card-body">`?**  
   *(Ответ: Сам класс `.card` рисует только внешнюю рамку со скруглёнными углами, а `.card-body` добавляет внутренние отступы, чтобы текст и кнопки не прилипали к краям карточки).*

---

➡️ **Перейти к выполнению самостоятельной работы:** [Инструкция к практическому заданию (PRACTICE.md)](./PRACTICE.md)