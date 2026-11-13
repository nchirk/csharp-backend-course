⬅️ [Назад к тексту лекции (README.md)](./README.md)

---

# Практическое задание: «CRUD Razor Pages для списка групп»

**Цель работы:** научиться создавать CRUD на Razor Pages для сущности группа. 

## Возьмите за основу код лекции
Перейдите на сайт https://download-directory.github.io/. Вставьте в поле ввода ссылку на код с лекции:
```
https://github.com/nchirk/csharp-backend-course/tree/main/lectures/08-razor-pages/src/lecture-demo
```

Разархивируйте приложение. Если не запускается, обновите на .NET 10.

<details>
<summary><b>Как обновить с .NET 9 на .NET 10 в Rider</b></summary>

> ### Как обновить с .NET 9 на .NET 10 в Rider
>
>1. В окне **Solution** нажмите правой кнопкой мыши на проект и выберите `Properties`.
>2. В разделе `Application` → `General` найдите **Target framework** и выберите `.NET 10.0`.
>3. Сохраните изменения, дождитесь восстановления NuGet-пакетов и выполните `Build → Build Solution`.
>4. Если `Target framework` недоступен в свойствах, откройте файл `.csproj` и измените `net9.0` на `net10.0` вручную.
>
>**Проверьте:** проект успешно собирается и запускается. Если используется EF Core 9, проверьте совместимость пакетов с .NET 10 и при необходимости обновите их до EF Core 10.
>
>### Если пакеты NuGet не обновились сами:
>
>1. В окне **Solution** нажмите правой кнопкой мыши на проект и выберите `Manage NuGet Packages`.
>2. Откройте вкладку **Updates**, выберите нужные пакеты и нажмите **Update**.
>3. Если переходите на .NET 10, обновите пакеты `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Sqlite` и `Microsoft.EntityFrameworkCore.Design` до совместимой версии **10.0.x**.
>4. Дождитесь завершения обновления и выполните `Build → Build Solution`.
</details>

Если не получилось собрать проект, позовите преподавателя.

## Задание
Используя `IGroupService`, создайте в папке `Pages/Groups` полный набор страниц по аналогии со студентами:

1. `Pages/Groups/Index.cshtml` — таблица со списком всех групп (ID, Название), поиск по названию и кнопка удаления через `OnPostDeleteAsync`.
2. `Pages/Groups/Create.cshtml` — форма добавления новой группы с валидацией и паттерном PRG.
3. `Pages/Groups/Update.cshtml` — форма редактирования названия группы.
4. Добавьте ссылку **«Группы»** в верхнее меню навигации в файле `Pages/Shared/_Layout.cshtml`.