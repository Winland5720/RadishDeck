# Changelog

Все значимые изменения проекта Radish Deck.

All significant changes of the Radish Deck project.

------------------------------------------------------------------------

# Alpha 0.1.0

Дата / Date:

19.09.2026

## Added / Добавлено

-   Создан новый проект Radish Deck.

-   Created a new Radish Deck project.

-   Добавлена базовая документация проекта.

-   Added initial project documentation.

-   Описана архитектура системы.

-   Defined system architecture.

-   Определён технологический стек C# / .NET.

-   Defined technology stack C# / .NET.

-   Добавлена документация API и WebSocket.

-   Added API and WebSocket documentation.

-   Добавлена документация Element System, Action System и Plugin
    System.

-   Added Element System, Action System and Plugin System documentation.

-   Добавлены правила безопасности и тестирования.

-   Added security and testing guidelines.

-   Добавлены брендовые материалы Radish Deck.

-   Added Radish Deck branding materials.

------------------------------------------------------------------------

# Alpha 0.1.1

## Added / Добавлено

-   Создано решение RadishDeck.sln.

-   Created RadishDeck.sln solution.

-   Добавлены проекты:

    -   RadishDeck.Core
    -   RadishDeck.Desktop
    -   RadishDeck.Server
    -   RadishDeck.Infrastructure

-   Desktop приложение запускается.

-   Desktop application starts successfully.

-   Server приложение запускается.

-   Server application starts successfully.

-   Добавлен первый Server Status API.

-   Added first Server Status API.

Endpoint:

GET /status

------------------------------------------------------------------------

# Alpha 0.1.2

## Added / Добавлено

-   Desktop подключён к Server Status API.

-   Desktop connected to Server Status API.

-   Добавлено отображение статуса сервера.

-   Added server status display.

-   Добавлен ServerLauncherService.

-   Added ServerLauncherService.

-   Добавлен запуск Server из Desktop.

-   Added Server startup from Desktop.

-   Добавлена настройка:

    -   IP Address
    -   Port

-   Added configuration:

    -   IP Address
    -   Port

-   Добавлены кнопки:

    -   Start Server
    -   Stop Server

## Verification / Проверка

-   dotnet build RadishDeck.sln успешно.

-   dotnet build RadishDeck.sln completed successfully.

-   Desktop управляет запуском Server.

-   Desktop controls Server lifecycle.

------------------------------------------------------------------------

# Alpha 0.1.3

## Planned / Планируется

Core foundation:

-   Element model.
-   Action model.
-   State model.

Создание базовых моделей ядра Radish Deck.

Creation of Radish Deck core models.

------------------------------------------------------------------------

# Versioning

Формат версии:

MAJOR.MINOR.PATCH

Alpha версии используются для ранней разработки и проверки архитектуры.

Alpha versions are used for early development and architecture
validation.
