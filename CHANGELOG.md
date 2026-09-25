# Changelog — RadishDeck

История изменений / Change history. Новые записи расположены первыми; исторические функции не обязательно доступны в текущей оболочке.

Newest entries first; historical features are not necessarily exposed in the current shell.

## Stage 5 — Application Shell + WebView2 Foundation (Unreleased)

**RU:** Stage 5 завершён и вручную проверен; новая версия релиза не назначалась. Версия сборки остаётся 0.3.1-alpha.

**EN:** Stage 5 is complete and manually validated; no new release version has been assigned. The assembly version remains 0.3.1-alpha.

## Добавлено / Added

**RU:** Локальный WebView2 UI, единая навигация, структурированный C# ↔ JavaScript bridge и Web Designer prototype. Canvas 1920×1080, zoom 50%, создание Button, выделение, список элементов, свойства, drag, clamp и сохранение координат.

**EN:** Local WebView2 UI, shared navigation, structured C# ↔ JavaScript bridge and Web Designer prototype. A 1920×1080 Canvas, 50% zoom, Button creation, selection, element listing, properties, dragging, clamping and coordinate persistence.

## Изменено / Changed

**RU:** WPF MainWindow стал WebView2 host. WpfCanvasRenderer сохранён как legacy/prototype. Существующий ServerLauncherService подключён к новой оболочке: Start/Stop/Restart, IPv4 selector, порт, статус, URL, открытие и копирование адреса. Drag выполняется локально в JS с финальной синхронизацией C#.

**EN:** WPF MainWindow became a WebView2 host. WpfCanvasRenderer remains legacy/prototype code. The existing ServerLauncherService is connected to the new shell: Start/Stop/Restart, IPv4 selection, port, status, URL, open and copy actions. Dragging stays local to JS with final C# synchronization.

## Исправлено / Fixed

**RU:** Двойной JSON parsing, несовпадение nested layout contract, undefined/NaN, ошибки ElementMove, прерывание drag из-за DOM rerender, попадание Designer errors в Server Status. Стандартное контекстное меню WebView2 отключено настройкой host; внешний Web Runtime не изменён.

**EN:** Fixed double JSON parsing, nested layout contract mismatches, undefined/NaN values, ElementMove errors, drag interruptions caused by DOM replacement and Designer errors appearing in Server Status. The default WebView2 context menu is disabled at host level; external Web Runtime behavior is unchanged.

## Проверено / Validated

**RU:** Пользователем подтверждены запуск, навигация, server lifecycle, bind/port/URL, открытие Web Runtime, создание/выбор/drag/clamp, сохранение после перезапуска и отсутствие браузерного контекстного меню. Restore/build/diff-check проходили. Команда dotnet test завершалась без ошибки, но console acceptance runner требует отдельного запуска: это не подтверждение прохождения всех проверок.

**EN:** User manual validation confirmed startup, navigation, server lifecycle, bind/port/URL, Web Runtime opening, creation/selection/drag/clamp, persistence after restart and context-menu suppression. Restore/build/diff-check passed. The dotnet test command completed without errors, but the console acceptance runner requires a separate invocation; this is not evidence that all checks ran.

## Ограничения / Limitations

**RU:** Полный Designer, resize, undo/redo, Pages/Layers editing, widgets, QR pairing и production shared renderer не входят в завершённый scope. Logs/Clients/Settings остаются заглушками. Stage 6 не начат.

**EN:** A full Designer, resize, undo/redo, Pages/Layers editing, widgets, QR pairing and production shared rendering are outside the completed scope. Logs/Clients/Settings remain placeholders. Stage 6 has not started.

## Основа 0.4 — Stages 1–4 (Unreleased)

**RU:** Добавлены CanvasProfile, DeviceProfile, секции Element V2 Layout/Content/Appearance/Behavior/Action и nullable-подключение к существующему Element без замены deck.v1. Введены IRenderer, IRenderContext и RenderElement. Stage 4 добавил WPF Canvas prototype с Button, выбором и перемещением. Согласованы документы Vision v3 и Designer.

**EN:** Added CanvasProfile, DeviceProfile, Element V2 Layout/Content/Appearance/Behavior/Action sections and nullable integration into the existing Element without replacing deck.v1. Introduced IRenderer, IRenderContext and RenderElement. Stage 4 added a WPF Canvas prototype with Button rendering, selection and movement. Aligned Vision v3 and Designer documentation.

## 0.3.1-alpha

**RU:** Добавлены локальные настройки сервера server.settings.json (IP, порт, имя) и Restart через существующий lifecycle service без изменения формата Deck и Action System.

**EN:** Added local server.settings.json configuration (IP, port, name) and Restart through the existing lifecycle service without changing the Deck format or Action System.

## 0.2.2-alpha

**RU:** Добавлен GET /deck: Server читает Deck из существующего deck.v1.json.

**EN:** Added GET /deck: Server reads Deck data from the existing deck.v1.json store.

## 0.2.1-alpha

**RU:** Добавлены process.start и ProcessStartActionExecutor для запуска программ.

**EN:** Added process.start and ProcessStartActionExecutor for launching applications.

## 0.2.0-alpha

**RU:** Добавлены WPF Deck Editor со страницами, сеткой и свойствами; Deck/Page/Element; ActionDefinition/Registry/Dispatcher; versioned JSON storage; действия server.start, server.stop и url.open; создание, выбор, удаление и редактирование Button. Сохранён server lifecycle 0.1.5.

**EN:** Added the WPF Deck Editor with pages, grid and properties; Deck/Page/Element models; ActionDefinition/Registry/Dispatcher; versioned JSON storage; server.start, server.stop and url.open actions; Button creation, selection, deletion and editing. Preserved the 0.1.5 server lifecycle.

## 0.1.5-alpha

**RU:** Server поставляется рядом с Desktop и запускается отдельным процессом. Добавлены Stopped/Starting/Running/Stopping/Error, фиксация конфигурации запуска, диагностика stdout/stderr, проверка /status по имени, версии, состоянию и PID, обработка занятого порта, отмены и аварийного завершения. Историческая проверка этапа: 20/20 lifecycle checks.

**EN:** Server is bundled alongside Desktop and runs as a separate process. Added Stopped/Starting/Running/Stopping/Error states, captured startup configuration, stdout/stderr diagnostics, /status validation by name/version/state/PID and handling of occupied ports, cancellation and unexpected exit. Historical stage validation: 20/20 lifecycle checks.

## 0.1.4-alpha

**RU:** Добавлены первый pipeline выполнения Element, механизм действий и Desktop dashboard prototype.

**EN:** Added the first Element execution pipeline, action execution mechanism and Desktop dashboard prototype.

## 0.1.3-alpha

**RU:** Добавлены Core-модели Element, Action и State; централизован источник версии приложения и формат с суффиксом этапа разработки.

**EN:** Added Core Element, Action and State models; centralized application versioning with a development-stage suffix.

## 0.1.2-alpha

**RU:** Desktop подключён к Server Status API. Добавлены ServerLauncherService, запуск/остановка сервера, настройка IP и порта, отображение статуса.

**EN:** Connected Desktop to the Server Status API. Added ServerLauncherService, server start/stop controls, IP/port configuration and status display.

## 0.1.1-alpha

**RU:** Созданы RadishDeck.sln и проекты Core, Desktop, Server, Infrastructure. Добавлены запускаемые Desktop/Server и GET /status.

**EN:** Created RadishDeck.sln and Core, Desktop, Server and Infrastructure projects. Added runnable Desktop/Server applications and GET /status.

## 0.1.0-alpha — 19.09.2026

**RU:** Создан проект, начальная архитектура и документация технологий, API, WebSocket, элементов, действий, плагинов, безопасности и тестирования; добавлены брендовые материалы. Документация будущих подсистем не означала их реализацию.

**EN:** Created the project, initial architecture and documentation for technology, API, WebSocket, elements, actions, plugins, security and testing; added branding assets. Documentation of future subsystems did not imply implementation.

## Версии / Versioning

**RU:** MAJOR.MINOR.PATCH-stage (alpha, beta, release). Номер сборки берётся из `src/RadishDeck.Core/AppVersion.cs`; номер этапа разработки не меняет версию автоматически.

**EN:** MAJOR.MINOR.PATCH-stage (alpha, beta, release). The assembly version comes from `src/RadishDeck.Core/AppVersion.cs`; a development stage does not automatically change the version.
