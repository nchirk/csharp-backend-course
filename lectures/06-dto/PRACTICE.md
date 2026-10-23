⬅️ [Назад к тексту лекции (README.md)](./README.md)

---

# Практическое задание: «Рефакторинг работы с сущностью Student»

Используя созданную архитектуру и пример с GroupController, самостоятельно отрефакторить работу со второй сущностью — Student.

## Возьмите за основу код лекции
Перейдите на сайт https://download-directory.github.io/. Вставьте в поле ввода ссылку на код с лекции:
```
https://github.com/nchirk/csharp-backend-course/tree/main/lectures/06-dto/src/lecture-demo
```

Разархивируйте приложение. Если не запускается, обновите на .NET 10.

### Как обновить с .NET 9 на .NET 10 в Rider

1. Установите **.NET 10 SDK** с [официального сайта](https://dotnet.microsoft.com/download/dotnet/10.0) и перезапустите Rider.
2. В окне **Solution** нажмите правой кнопкой мыши на проект и выберите `Properties`.
3. В разделе `Application` → `General` найдите **Target framework** и выберите `.NET 10.0`.
4. Сохраните изменения, дождитесь восстановления NuGet-пакетов и выполните `Build → Build Solution`.
5. Если `Target framework` недоступен в свойствах, откройте файл `.csproj` и измените `net9.0` на `net10.0` вручную.

**Проверьте:** проект успешно собирается и запускается. Если используется EF Core 9, проверьте совместимость пакетов с .NET 10 и при необходимости обновите их до EF Core 10.

### Если пакеты NuGet не обновились сами:

1. В окне **Solution** нажмите правой кнопкой мыши на проект и выберите `Manage NuGet Packages`.
2. Откройте вкладку **Updates**, выберите нужные пакеты и нажмите **Update**.
3. Если переходите на .NET 10, обновите пакеты `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Sqlite` и `Microsoft.EntityFrameworkCore.Design` до совместимой версии **10.0.x**.
4. Дождитесь завершения обновления и выполните `Build → Build Solution`.

## Пошаговый план выполнения задания

### Шаг 1. Создание DTO моделей
В проекте **Web API** внутри папки `DTOs/Students/` создайте четыре стандартных класса DTO. 

> Один класс — один физический файл с точно таким же именем.

1. **`StudentCreateDto.cs`** — для создания студента (`POST`). Содержит только те поля, которые вводит пользователь:
   ```csharp
   namespace StudentApiWithDB.DTOs.Students;

   public class StudentCreateDto
   {
       public string Name { get; set; } = string.Empty;
       public int Age { get; set; }
       public int GroupId { get; set; }
   }
   ```
2. **`StudentUpdateDto.cs`** — для редактирования профиля (`PUT`). 
   ```csharp
   namespace StudentApiWithDB.DTOs.Students;

   public class StudentUpdateDto
   {
       public string Name { get; set; } = string.Empty;
       public int Age { get; set; }
       public int GroupId { get; set; }
   }
   ```
    Набор полей может отличаться от создания (например, мы запрещаем менять группу при обычном редактировании). Если хотите, можете убрать часть полей.

3. **`StudentGetDto.cs`** — плоский DTO для выдачи полной информации о студенте (`GET`). Вместо системного `GroupId` он должен возвращать клиенту красивое вычисляемое поле `GroupName`:
   ```csharp
   namespace StudentApiWithDB.DTOs.Students;

   public class StudentGetDto
   {
       public int Id { get; set; }
       public string Name { get; set; } = string.Empty;
       public int Age { get; set; }
       public string GroupName { get; set; } = string.Empty;
   }
   ```
4. **`StudentShortDto.cs`** — облегченный DTO без упоминания группы. Он нужен для вложения в детальную информацию о группе (мы создавали его в первой части пары):
   ```csharp
   namespace StudentApiWithDB.DTOs.Students;

   public class StudentShortDto
   {
       public int Id { get; set; }
       public string Name { get; set; } = string.Empty;
   }
   ```

---

### Шаг 2. Создание интерфейса и сервиса данных
В проекте **Class Library (`StudentApi.Data`)** внутри папки `Services/`:

1. Создайте интерфейс **`IStudentService.cs`**, описывающий контракт для работы со студентами:
   ```csharp
   using StudentApiWithDB.DTOs.Students;

   namespace StudentApi.Data.Services;

   public interface IStudentService
   {
       Task<IEnumerable<StudentGetDto>> GetAllAsync(string? search);
       Task<StudentGetDto?> GetByIdAsync(int id);
       Task<StudentGetDto> CreateAsync(StudentCreateDto dto);
       Task<bool> UpdateAsync(int id, StudentUpdateDto dto);
       Task<bool> DeleteAsync(int id);
   }
   ```
2. Создайте класс **`StudentService.cs`**, который реализует этот интерфейс и принимает `AppDbContext` через конструктор.

#### ⚠️ Критически важные правила при написании методов сервиса:
* В методах `GetAllAsync` и `GetByIdAsync` пишите оператор `.Select(...)` **СТРОГО ДО** вызова асинхронных методов (`ToListAsync` / `FirstOrDefaultAsync`), чтобы EF Core тянул из базы только нужные колонки.
* Внутри `Select` при формировании `StudentGetDto` имя группы забирайте через навигационное свойство:
  ```csharp
  .Select(s => new StudentGetDto
  {
      // ...тут присвоение других полей

      GroupName = s.Group.Name
  })
  ```
* *Подсказка:* Писать `.Include(s => s.Group)` при использовании `Select` **не обязательно** — EF Core сам догадается какие подтянуть таблицы из-за Select.

---

### Шаг 3. Регистрация в Dependency Injection
Перейдите в файл `Program.cs` проекта **Web API** и зарегистрируйте ваш новый сервис в контейнере зависимостей рядом с сервисом групп:

```csharp
builder.Services.AddScoped<IStudentService, StudentService>();
```

---

### Шаг 4. Очистка StudentController («Тонкий контроллер»)
Полностью перепишите **`StudentController.cs`**. 
1. **Удалите** из него прямое внедрение `AppDbContext`. Контроллер больше не имеет права взаимодействовать с базой данных напрямую!
2. Внедрите через конструктор интерфейс `IStudentService`.
3. Перенаправьте все методы контроллера (`Get`, `GetById`, `Post`, `Put`, `Delete`) на соответствующие методы сервиса. Контроллер должен только принимать HTTP-запрос, передавать DTO-класс в сервис и возвращать JSON с правильным статус-кодом (`Ok`, `NotFound`, `NoContent`, `CreatedAtAction`).
4. Метод HttpPatch можно удалить. Либо по вашему желанию можете добавить еще один метод в `IStudentService` и `StudentService`

---

## Критерии успешности (Как проверить свою работу)

1. Проект компилируется без ошибок.
2. В коде `StudentController` **нет ни одного упоминания** `AppDbContext` или сущностей из папки `Entities`.
3. Вы запустили проект, открыли http-файлы и проверили эндпоинты:
   * При вызове `GET /api/student` сервер возвращает список плоских объектов, где поле `GroupName` заполнено реальным текстовым названием группы из базы данных (например, `"ИСП-401"`), а не числовым ID.
   * При создании студента через `POST` передается `StudentCreateDto`, а в ответ возвращается статус `201 Created` с заполненным `Id` нового студента.
   * Методы Update и Delete работают корректно.