# Контрольная работа 3: Разработка CRUD-приложения на ASP.NET Core Razor Pages

**Цель работы:** Закрепить навыки создания серверных веб-страниц на Razor Pages, работы с формами, паттерном PRG, связанными сущностями и валидацией данных.

**Исходные условия:**
У вас есть существующий солюшен с проектами `StudentApi.Data` и `StudentRazorPages`, в котором уже реализован функционал для сущности `Student`. Ваша задача — по аналогии реализовать модуль управления учебными группами (`Group`), учитывая их связь с направлением подготовки (`StudyProgram`).

Каждая следующая оценка требует полного выполнения заданий на предыдущие (4 включает 3, 5 включает 4).

**Красивое оформление** с помощью Bootstrap **НЕ ТРЕБУЕТСЯ**, по вашему желанию.

## Возьмите за основу код из папки `/src/starter`
Перейдите на сайт https://download-directory.github.io/. Вставьте в поле ввода ссылку на код:
```
https://github.com/nchirk/csharp-backend-course/tree/main/labs/03-razor-pages/src/.net10/starter
```

Приложение под .NET 10. Разархивируйте и запустите его, проверьте, что нет ошибок:
* внизу среды нет красного текста с Error
* метод `### 1. Получить весь список групп` из файла `HttpRequests/group.http` работает и возвращает группы (запуск по зеленому треугольнику слева от метода).

Если на этом этапе возникнут сложности, позовите преподавателя, на оценку это не повлияет.

---

## Задание на оценку «3» (Страница `Index` со списком групп)

Реализуйте вывод списка учебных групп на главной странице раздела `Pages/Groups`:

1. **Настройка DI и навигации:**
   - Убедитесь, что в файле `Program.cs` зарегистрирован сервис `IGroupService`.
   - Добавьте ссылку **«Список групп»** (`/Groups/Index`) в главное меню в файле `Pages/Shared/_Layout.cshtml`.

2. **Модель страницы (`Pages/Groups/Index.cshtml.cs`):**
   - Внедрите `IGroupService` через конструктор.
   - Объявите свойство для хранения коллекции групп (например, `IEnumerable<GroupGetDto> Groups`) и вычисляемое свойство для вывода общего количества групп.
   - В методе `OnGetAsync()` получите все группы через `_groupService.GetAllAsync(null)`.

3. **Представление (`Pages/Groups/Index.cshtml`):**
   - Задайте заголовок вкладки через `ViewData["Title"]`.
   - Выведите общее количество групп и HTML-таблицу (`<table>`) с основными полями группы (ID, название) с помощью цикла `@foreach`.


## Задание на оценку «4» (Поиск, удаление, страницы `Details`, `Create`, `Update` и Partial View)

*Выполняется поверх рабочего кода на оценку «3».*

1. **Декомпозиция таблицы и поиск на странице `Index`:**
   - Вынесите разметку одной строки таблицы в частичное представление `Pages/Groups/_GroupRow.cshtml` (без директивы `@page`) и подключите его в `Index.cshtml` через `<partial name="_GroupRow" model="..." />`.
   - Добавьте в `Index.cshtml.cs` свойство `SearchTerm` с атрибутом `[BindProperty(SupportsGet = true)]` и передавайте его в `_groupService.GetAllAsync(SearchTerm)`.
   - Добавьте на `Index.cshtml` форму поиска с `method="get"` и ссылку сброса фильтра.

2. **Удаление группы на странице `Index`:**
   - В `Index.cshtml.cs` реализуйте именованный POST-обработчик `OnPostDeleteAsync(int id)`, вызывающий `_groupService.DeleteAsync(id)` и выполняющий редирект (`RedirectToPage()`).
   - В `_GroupRow.cshtml` добавьте мини-форму `<form method="post" asp-page-handler="Delete" asp-route-id="@Model.Id" ...>` с подтверждением удаления через `onsubmit="return confirm(...);"`.

3. **Страница просмотра (`Pages/Groups/Details.cshtml` и `.cshtml.cs`):**
   - Настройте маршрут `@page "{id:int}"`.
   - Загружайте группу по `id`. Если группа не найдена (`null`), возвращайте `NotFound()`.
   - Выведите основные данные группы и ссылки «Редактировать» и «Назад к списку».

4. **Страницы создания и редактирования (`Create.cshtml` и `Update.cshtml`):**
   - Над таблицей в `Index.cshtml` добавьте ссылку на страницу `Create`, а в `_GroupRow.cshtml` — ссылки на `Details` и `Update`.
   - Реализуйте формы создания (`GroupCreateDto`) и редактирования (`GroupUpdateDto`) с использованием атрибута `[BindProperty]`.
   - После успешного сохранения данных примените паттерн **PRG** (`return RedirectToPage("./Index")`).

## Задание на оценку «5» (Связанная сущность `StudyProgram` и валидация данных)

*Выполняется поверх рабочего кода на оценку «4».*

1. **Отображение и выбор связанной сущности `StudyProgram`:**
   - Добавьте вывод названия направления подготовки (`StudyProgram`) в таблицу на странице `Index` (в `_GroupRow.cshtml`) и на страницу `Details`.
   - Внедрите `IStudyProgramService` в конструкторы моделей страниц `Create` и `Update` (проверьте регистрацию сервиса в `Program.cs`).
   - Реализуйте метод загрузки списка направлений в коллекцию `IEnumerable<SelectListItem>`.
   - Добавьте на формы `Create.cshtml` и `Update.cshtml` выпадающий список `<select asp-for="Input.StudyProgramId" asp-items="...">` с пустой опцией по умолчанию `<option value="">-- Выберите направление --</option>`.

2. **Настройка правил валидации в DTO и поиске:**
   - В классах `GroupCreateDto` и `GroupUpdateDto` добавьте атрибуты Data Annotations (`[Required]`, `[StringLength]` и др. с понятными текстами в `ErrorMessage`).
   - В `Groups/Index.cshtml.cs` ограничьте длину поискового запроса `SearchTerm` с помощью `[StringLength]` и проверяйте `ModelState.IsValid` в `OnGetAsync()`.

3. **Серверная и клиентская валидация в `Create` и `Update`:**
   - В `OnPostAsync` на страницах `Create` и `Update` добавьте проверку `if (!ModelState.IsValid)`. При наличии ошибок заново загрузите список `StudyProgram` для `<select>` и верните `Page()`.
   - На странице `Update`, если метод обновления сервиса вернул `false`, добавьте общую ошибку через `ModelState.AddModelError(string.Empty, ...)`.
   - Выведите ошибки на страницах с помощью Tag Helpers `asp-validation-summary="ModelOnly"` и `asp-validation-for`.
   - Подключите клиентскую валидацию на страницах `Create`, `Update` и `Index` через `@section Scripts` и `<partial name="Shared/_ValidationScriptsPartial" />`.

4. **Обработка ошибок при удалении через `[TempData]`:**
   - В `Groups/Index.cshtml.cs` добавьте свойство `[TempData] public string? DeleteErrorMessage { get; set; }`.
   - В `OnPostDeleteAsync(int id)` проверяйте результат: если `DeleteAsync` вернул `false`, записывайте сообщение об ошибке в `DeleteErrorMessage` и выводите его на странице `Index.cshtml`.

---

## Вопросы на понимание

### Проверочные вопросы на оценку «3»

1. **Зачем в первой строке файла `Index.cshtml` указывается директива `@page`, а во второй — `@model`?**

3. **Где отобразится значение из `ViewData["Title"]`, заданное внутри `Groups/Index.cshtml`?**

4. **В чём разница между директивой `@model` (с маленькой буквы) и свойством `@Model` (с заглавной буквы) внутри `.cshtml`?**

### Проверочные вопросы на оценку «4»

1. **Зачем в первой строке файла `Details.cshtml` пишется `@page "{id:int}"`?**

2. **В чём суть паттерна PRG (Post-Redirect-Get) и что случится при нажатии `F5`, если после создания группы вернуть `Page()` вместо `RedirectToPage("./Index")`?**

3. **Что произойдёт при отправке формы создания группы, если забыть поставить атрибут `[BindProperty]` над свойством `Input` в `Create.cshtml.cs`?**

### Проверочные вопросы на оценку «5»

1. **Покажите в своём коде `OnPostAsync` страницы `Create`: что произойдёт при отправке невалидной формы, если перед `return Page()` не вызвать повторную загрузку списка `StudyProgram`?**

2. **Какие два аргумента передаются в `new SelectListItem(..., ...)` и какое из этих значений реально отправится на сервер при выборе пункта из `<select>`?**

3. **В чём разница между `asp-validation-for` и `asp-validation-summary="ModelOnly"`?**

4. **Почему ошибку при неудачном удалении в `OnPostDeleteAsync` нельзя записать в `ModelState`, а приходится использовать атрибут `[TempData]`?**

5. **Зачем на сервере проверять `if (!ModelState.IsValid)`, если на страницу уже подключён скрипт клиентской валидации `_ValidationScriptsPartial`?**