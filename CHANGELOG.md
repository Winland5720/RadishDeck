# Changelog

Все значимые изменения проекта Radish Deck.

All significant changes of the Radish Deck project.

------------------------------------------------------------------------

# 0.1.0-alpha

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

# 0.1.1-alpha

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

# 0.1.2-alpha

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

# 0.1.3-alpha

## Added / Добавлено

-   Added Core Element model.
-   Added Core Action model.
-   Added Core State model.
-   Centralized application version source.
-   Unified version format with development stage suffix.

Создан базовый фундамент моделей ядра Radish Deck.

Creation of the Radish Deck Core model foundation.

## Verification / Проверка

-   `dotnet build RadishDeck.sln` успешно.
-   Core не содержит ссылок на Desktop, Server, Infrastructure или сторонние библиотеки.

------------------------------------------------------------------------

# 0.1.4-alpha

## Added / Добавлено

-   First Element execution pipeline.
-   Action execution mechanism.
-   Desktop dashboard prototype.

------------------------------------------------------------------------

# 0.1.5-alpha

## Changed / Изменено

- Server поставляется вместе с Desktop в `publish/server` и запускается как отдельный процесс без исходников и `dotnet run`.
- Добавлено согласованное состояние жизненного цикла Server: Stopped, Starting, Running, Stopping, Error.
- IP и Port фиксируются снимком до запуска и блокируются до полной остановки.
- stdout/stderr Server читаются асинхронно с ограниченным диагностическим буфером.
- `/status` проверяется по name, version, status и PID дочернего процесса.
- Обрабатываются неожиданный exit, отмена запуска, занятый порт и диагностические сообщения.
- Добавлены интеграционные проверки жизненного цикла.

## Verification / Проверка

- `dotnet build RadishDeck.sln -c Release` — 0 ошибок, 0 предупреждений.
- 20/20 lifecycle checks passed.
------------------------------------------------------------------------

# Versioning

Формат версии:

MAJOR.MINOR.PATCH-stage (alpha, beta или release)

Alpha версии используются для ранней разработки и проверки архитектуры.

Alpha versions are used for early development and architecture
validation.


------------------------------------------------------------------------

# 0.2.0-alpha

## Added / Добавлено

- First usable Deck Editor WPF layout with Pages, workspace grid and properties panel.
- Added Core `Deck`, `Page` and extended `Element` domain models.
- Added `ActionDefinition`, `ActionRegistry` and `ActionDispatcher`.
- Added JSON persistence with a versioned `deck.v1.json` local store and default Deck.
- Added registered `server.start`, `server.stop` and safe `url.open` actions.
- Added Button element creation, selection, deletion, name/action/position editing.
- Preserved the 0.1.5 Server lifecycle and bottom server control panel.

## Deferred / Отложено

- Full Plugin System, AI, Web/Tablet clients, SQLite, Profiles, arbitrary shell execution and advanced drag-and-drop remain planned.

------------------------------------------------------------------------

# 0.2.1-alpha

## Added

- Added `process.start` action.
- Added `ProcessStartActionExecutor`.

## Changed

- Added first real executable action.

## Deferred

- Server execution pipeline.
- Web client.
- Mobile client.

------------------------------------------------------------------------

# 0.2.2-alpha

## Added

- Server Deck read API.
- `GET /deck` endpoint.

## Changed

- Server can read Deck data from the existing `deck.v1.json` store.

## Deferred

- `PUT /deck`.
- Desktop Server synchronization.
- Web client.
- 0.3.1-alpha
  - Added persistent local ServerSettings (IP, port and name) in `server.settings.json`.
  - Added lifecycle Restart without changing the Action System or Deck format.

## Documentation alignment — Vision v3

- Aligned UI, elements, roadmap and technology documentation with Vision v3, Architecture v3 and Designer Specification.
- Clarified Canvas/device profiles, shared rendering, optional action bindings, and the distinction between planned features and historical implementation notes.
