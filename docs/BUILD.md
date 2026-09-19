# RADISH DECK

# BUILD DOCUMENTATION

Версия:

Alpha 0.1.0


---

# 1. Требования

Среда разработки:

- Windows 10/11
- .NET SDK 8+
- Visual Studio 2022
- Git


---

# 2. Технологический стек

Основной язык:

C#

Платформа:

.NET 8+

Desktop:

WPF (основной вариант)

Server:

ASP.NET Core

Database:

SQLite


---

# 3. Структура проекта

RadishDeck

├── src

├── docs

├── assets

├── tests

└── README.md


---

# 4. Debug сборка

Команда:

dotnet build


Используется для разработки.


---

# 5. Release сборка

Команда:

dotnet publish -c Release


Результат:

RadishDeck.exe


---

# 6. Запуск приложения

Порядок запуска:

1. Splash Screen
2. Загрузка конфигурации
3. Подключение базы данных
4. Запуск сервера
5. Загрузка плагинов
6. Открытие интерфейса


---

# 7. Release Checklist

Перед выпуском:

- код проверен;
- тесты пройдены;
- документация обновлена;
- версия изменена;
- создан Git tag.


---

# 8. Правила

Каждый релиз должен иметь:

- версию;
- changelog;
- сборку;
- документацию.
