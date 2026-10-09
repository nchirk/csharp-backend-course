# csharp-backend-course
Практический курс по разработке веб-приложений на C# и ASP.NET Core для студентов колледжа. Лекции, лабораторные работы и примеры кода.

## Системные требования и софт
Для прохождения курса вам понадобятся:
* **SDK:** [.NET 8.0, .NET 9.0 или .NET 10.0](https://microsoft.com)
* **IDE:** [JetBrains Rider](https://jetbrains.com) (рекомендуется) или Visual Studio.
* **Git:** [Скачать Git](https://git-scm.com)


## Карта курса (Лекции и контрольные)

### Часть 1: Web API
* Лекция 1. Введение в ASP.NET Core
* [Лекция 2. Основы ASP.NET Core Web API: Основы маршрутизации, CRUD и протокол HTTP](./lectures/02-routing-and-http/README.md)
* [Лекция 3. Модели и привязка данных, полный CRUD, настройка маршрутизации](./lectures/03-models/README.md)
* [Контрольная 1. Разработка Web API на ASP.NET Core](./labs/01-web-api/README.md)

### Часть 2: Базы данных, EF Core
* [Лекция 4. Основы EF Core, async/await, Dependency Injection](./lectures/04-ef-core-intro/README.md)

## Структура этого репозитория
```
csharp-backend-course/
├── .gitignore
├── README.md                 <-- Стартовая страница (вы читаете её прямо сейчас)
│
lectures/
└── 02-routing-and-http/      <-- Папка второй лекции
    ├── README.md             <-- Теория и разбор
    ├── PRACTICE.md           <-- Задание на самостоятельную работу
    └── src/                  <-- ТУТ ЖИВУТ ПРОЕКТЫ С КОДОМ
        └── lecture-demo/     <-- код преподавателя с лекции

```


## Как скачать материалы одной лекции

Чтобы не скачивать весь репозиторий со всеми будущими проектами, вы можете скачать папку только нужной вам лекции. Откройте терминал на компьютере и выполните команды (на примере Лекции 3):

```bash
# 1. Скачиваем структуру репозитория (без самих файлов)
git clone --depth 1 --sparse https://github.com/nchirk/csharp-backend-course
cd csharp-backend-course

# 2. Скачиваем только конкретную папку лекции вместе с кодом
git sparse-checkout set lectures/03-models
```

Если вам пока некомфортно работать с гитом, вы можете выкачать папку как zip-архив через сторонний сервис, например этот https://download-directory.github.io/. Вставьте в поле ввода ссылку на папку с лекцией из карты курса. Ссылка для Лекции 3:
```
https://github.com/nchirk/csharp-backend-course/tree/main/lectures/03-models
```
---
*Если у вас возникли вопросы по коду или вы нашли опечатку в лекции — пишите в учебный чат группы!*