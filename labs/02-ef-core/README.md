# Контрольная работа 2: Расширение БД и сервисного слоя

Мы расширяем базу данных новой сущностью **StudyProgram (Направление подготовки / Специальность)**. Ваша задача — связать направления с учебными группами отношением «один ко многим» (у направления много групп) и реализовать эту логику на всех уровнях приложения: от таблиц в SQLite до C#-сервисов и рабочих API-эндпоинтов в контроллерах.

Каждая следующая оценка требует полного выполнения заданий на предыдущие (4 включает 3, 5 включает 4).

## Возьмите за основу код из папки `/src/starter`
Перейдите на сайт https://download-directory.github.io/. Вставьте в поле ввода ссылку на код:
```
https://github.com/nchirk/csharp-backend-course/tree/main/labs/02-ef-core/src/.net10/starter
```

Приложение под .NET 10. Разархивируйте и запустите его, проверьте, что нет ошибок:
* внизу среды нет красного текста с Error
* метод `### 1. Получить весь список групп` из файла `HttpRequests/group.http` работает и возвращает группы (запуск по зеленому треугольнику слева от метода).

Если на этом этапе возникнут сложности, позовите преподавателя, на оценку это не повлияет.

---

## ЗАДАНИЕ НА ОЦЕНКУ «3»

### 1. Создание сущности
В проекте `StudentApi.Data` (папка `Entities`) создайте файл `StudyProgram.cs`:
* `Id` (int)
* `Name` (string)

### 2. Регистрация в контексте
В `AppDbContext.cs` добавьте свойство:  
`public DbSet<StudyProgram> StudyPrograms { get; set; }`

### 3. Миграция
Создайте и примените миграцию через графический интерфейс Rider:
> 1. Щелкните правой кнопкой мышки по проекту `StudentApi.Data` и выберите пункт `Entity Framework Core -> Add Migration`.
> 
> 2. Настройка базовых параметров в окне:
>    * **Migrations project:** Укажите проект, в котором находится класс контекста данных (`AppDbContext`) и папка с миграциями — **StudentApi.Data**
>    * **Startup project:** Выберите исполняемый проект Web Api — **StudentApiWithDB**
>    * DbContext class: Выберите из выпадающего списка нужный контекст (если их несколько). *AppDbContext*
> 
> 3. Задайте название миграции и нажмите кнопку `Add`
> 
> 4. Примените миграцию к базе данных: правая кнопка мышки по проекту, `Entity Framework Core -> Update Database`.
> 

Альтенативный способ через Терминал Rider:

```bash
# 1. Добавить миграцию
dotnet ef migrations add AddStudyProgram --project StudentApi.Data --startup-project StudentApiWithDB

# 2. Обновить базу данных
dotnet ef database update --project StudentApi.Data --startup-project StudentApiWithDB
```
*> **Важно:** `--project` — где лежит контекст (`StudentApi.Data`), `--startup-project` — запускаемый веб-проект (`StudentApiWithDB`).*

### 4. Проверка
Покажите преподавателю новую таблицу `StudyPrograms` во вкладке **Database** в Rider или в DB Browser for SQLite.

---

## ЗАДАНИЕ НА ОЦЕНКУ «4»

### 1. Связь «Один ко многим» (StudyProgram -> Group)
* Свяжите `StudyProgram` и `Group` связью 1 ко многим (у одной специальности много групп). Используйте навигационные свойства.

### 2. Конфигурация Fluent API
1. В `StudentApi.Data` (папка `Data/Configurations`) создайте класс `StudyProgramConfiguration.cs` (`IEntityTypeConfiguration<StudyProgram>`).
2. Настройте первичный ключ: `.HasKey(x => x.Id)`.
3. В конфигурации группы или направления явно настройте связь через `.HasOne()`, `.WithMany()` и `.HasForeignKey()`.
4. Создайте и примените новую миграцию.

### 3. Логика сервиса
В классе `StudyProgramService.cs` реализуйте методы:
* `GetAllAsync`: возвращает список направлений, спроецированных в `StudyProgramGetDto`.
* `CreateAsync`: принимает `StudyProgramCreateDto`, маппит в сущность `StudyProgram`, сохраняет в БД через `SaveChangesAsync()`.

Необходимые DTO и методы контроллера уже реализованы. Вам нужно дописать только `StudyProgramService`.

Используйте `async/await`.

### 4. Проверка
Запустите проект. Протестируйте добавление (`POST`) и получение (`GET`) направлений через `HttpRequests/study-program.http`.

---

## ЗАДАНИЕ НА ОЦЕНКУ «5»

### 1. Новые операции
Самостоятельно добавьте функции **детального просмотра** (по ID) и **редактирования** направления.

### 2. DTO модели
В папке `DTOs/StudyProgram/` создайте необходимые классы для этих операций.

### 3. Сервис и контроллер
1. Добавьте методы получения по ID и обновления в интерфейс `IStudyProgramService` и класс `StudyProgramService`.
2. Напишите соответствующие эндпоинты в `StudyProgramController`. Возвращайте правильные REST-статусы (`Ok`, `NotFound`, `NoContent`).

### 4. Проверка
Продемонстрируйте преподавателю работу новых эндпоинтов получения по ID и обновления через `HttpRequests/study-program.http`.
