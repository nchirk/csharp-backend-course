⬅️ [Назад к тексту лекции (README.md)](./README.md)

---

# Практическое задание: «CRUD Razor Pages для списка групп»

**Цель работы:** научиться создавать CRUD на Razor Pages для сущности группа. 

## Возьмите за основу код лекции
Перейдите на сайт https://download-directory.github.io/.

1. Для первого задания вставьте в поле ввода ссылку на код предыдущей лекции:

```
https://github.com/nchirk/csharp-backend-course/tree/main/lectures/07-razor/src/lecture-demo
```

2. Для второго задания вставьте в поле ввода ссылку на код текущей лекции:
```
https://github.com/nchirk/csharp-backend-course/tree/main/lectures/08-razor-pages/src/lecture-demo
```

Разархивируйте приложение. Если не запускается, обновите на .NET 10.

[Как обновить с .NET 9 на .NET 10 в Rider](/info/upgrade-dotnet.md)

Если не получилось собрать проект, позовите преподавателя.

## Задание

Сделайте одно из двух заданий на выбор.

### Задание 1

Возьмите код предыдцщей лекции и попробуйте повторить создание сущности Student ориентируясь на [текст лекции](./README.md).

### Задание 2
Используя `IGroupService`, создайте в папке `Pages/Groups` полный набор страниц по аналогии со студентами:

1. `Pages/Groups/Index.cshtml` — таблица со списком всех групп (ID, Название), поиск по названию и кнопка удаления через `OnPostDeleteAsync`.
2. `Pages/Groups/Create.cshtml` — форма добавления новой группы с валидацией и паттерном PRG.
3. `Pages/Groups/Update.cshtml` — форма редактирования названия группы.
4. Добавьте ссылку **«Группы»** в верхнее меню навигации в файле `Pages/Shared/_Layout.cshtml`.