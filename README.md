# RadishDeck

<p align="center">
  <img src="assets/branding/radishdeck_logo_horizontal.png" alt="RadishDeck">
</p>

[Русский](#русский) · [English](#english)

## Русский

RadishDeck — конструктор персональных панелей управления компьютером и сервисами. Пользователь создаёт Deck в Desktop, а открывает пульт через Server в обычном браузере.

**Состояние:** alpha; Stage 5 — Application Shell + WebView2 Foundation — завершён и вручную проверен. Версия сборки в `src/RadishDeck.Core/AppVersion.cs`: **0.3.1-alpha**. План разработки 0.4 описывает этапы развития, а не уже выпущенную версию.

### Что работает

- Desktop на WPF с локальным HTML/CSS/JavaScript UI внутри WebView2, без CDN.
- Общая навигация: Главная, Редактор, Логи, Клиенты, Настройки. Последние три раздела пока содержат заглушки.
- Управление существующим Server: Start, Stop, Restart, реальное состояние, выбор IPv4 bind address, порт с проверкой диапазона 1–65535, адрес пульта, открытие и копирование URL.
- Designer prototype: логический Canvas 1920×1080, визуальный zoom 50%, создание Button, выделение, список элементов и просмотр свойств.
- Перетаскивание с pointer capture и ограничением координат границами Canvas. Движение выполняется локально в JavaScript; после завершения координаты передаются в C#, сохраняются и подтверждаются повторным render.
- Element V2 и независимый Renderer Contract в Core; сохранение Deck в JSON с совместимостью deck.v1.
- Отдельный Web Runtime с отображением Deck и вызовом действий через Server.

В Desktop отключено стандартное браузерное контекстное меню WebView2. Это не меняет поведение внешнего браузера.

### Архитектура

| Компонент | Назначение |
|---|---|
| RadishDeck.Desktop | WPF host: окно, Windows integration, ServerLauncherService и WebView2 |
| Desktop Web UI | Локальная оболочка и Designer в `src/RadishDeck.Desktop/Web` |
| RadishDeck.Core | Element V2, CanvasProfile, DeviceProfile, модели и Renderer Contract без UI-зависимостей |
| RadishDeck.Infrastructure | JSON persistence |
| RadishDeck.Server | Отдельный ASP.NET Core процесс, API и браузерный Web Runtime |

Desktop Web UI и Web Runtime имеют разные назначения. Общий production-ready renderer ещё не реализован. `WpfCanvasRenderer` сохранён как legacy/prototype.

Bridge использует camelCase JSON: `element.layout.x/y/width/height` и `element.content.text`. Zoom не меняет логические координаты модели.

### Запуск

Нужны Windows, .NET 8 SDK и установленный Microsoft Edge WebView2 Runtime. Для запуска без SDK нужны .NET 8 Desktop Runtime и ASP.NET Core Runtime 8.

Из корня репозитория:

```powershell
dotnet restore RadishDeck.sln
dotnet build RadishDeck.sln
dotnet run --project src/RadishDeck.Desktop/RadishDeck.Desktop.csproj
```

На Главной выберите bind address и порт, нажмите Start, затем «Открыть пульт». `127.0.0.1` доступен только на этом компьютере; для другого устройства выберите LAN-адрес. Backend не поддерживает wildcard bind `0.0.0.0`. Server собирается рядом с Desktop и запускается отдельным процессом.

Данные хранятся в `%LOCALAPPDATA%/RadishDeck/deck.v1.json`, настройки сервера — в `server.settings.json` в той же папке. API: `GET /status`, `GET /deck`, `POST /execute`.

### Ограничения

Это фундамент Designer. Нет resize, rotation, полного редактора свойств, управления Pages/Layers в новой оболочке, multi-selection, undo/redo, custom context menu, advanced grid/snap, widgets, plugins или QR pairing. Tablet/Mobile Designer и общий renderer остаются будущей работой.

Изображения в `assets/image concept/` — утверждённое визуальное направление; текущий UI не является финальным дизайном.

### Проверки и документация

`dotnet test RadishDeck.sln` не запускает существующий console acceptance runner автоматически: проект проверок является executable. После Debug build его можно запустить отдельно:

```powershell
dotnet run --project tests/RadishDeck.Lifecycle.Tests --no-build -- src/RadishDeck.Desktop/bin/Debug/net8.0-windows/server/RadishDeck.Server.exe tests/RadishDeck.TestServer/bin/Debug/net8.0/RadishDeck.TestServer.exe 127.0.0.1
```

Loopback проверяет локальный сценарий, но не доступ по LAN.

[История изменений](CHANGELOG.md) · [Архитектура](docs/ARCHITECTURE.md) · [Модель элементов](docs/ELEMENT_MODEL.md) · [План 0.4](docs/DEVELOPMENT_0.4_PLAN.md)

## English

RadishDeck is a constructor for personal computer and service control panels. Users create a Deck in Desktop and open the control panel through Server in a regular browser.

**Status:** alpha; Stage 5 — Application Shell + WebView2 Foundation — is complete and manually validated. The assembly version in `src/RadishDeck.Core/AppVersion.cs` is **0.3.1-alpha**. The 0.4 development plan describes ongoing stages, not an already released version.

### Available functionality

- WPF Desktop host with local HTML/CSS/JavaScript UI in WebView2, without CDN dependencies.
- Shared navigation: Home, Editor, Logs, Clients and Settings. The last three sections are placeholders.
- Existing Server controls: Start, Stop, Restart, actual lifecycle state, IPv4 bind-address selection, port validation (1–65535), access URL, open and copy actions.
- Designer prototype: 1920×1080 logical Canvas, 50% visual zoom, Button creation, selection, element listing and property display.
- Pointer-capture dragging with logical Canvas bounds. Movement stays local to JavaScript; final coordinates reach C#, are persisted and confirmed by a render response.
- Element V2 and a framework-independent Renderer Contract in Core; JSON Deck persistence with deck.v1 compatibility.
- Separate Web Runtime displaying the Deck and invoking actions through Server.

The default WebView2 browser context menu is disabled inside Desktop. External browser behavior is unchanged.

### Architecture

| Component | Responsibility |
|---|---|
| RadishDeck.Desktop | WPF host: window lifecycle, Windows integration, ServerLauncherService and WebView2 |
| Desktop Web UI | Local shell and Designer in `src/RadishDeck.Desktop/Web` |
| RadishDeck.Core | Element V2, CanvasProfile, DeviceProfile, models and Renderer Contract without UI dependencies |
| RadishDeck.Infrastructure | JSON persistence |
| RadishDeck.Server | Separate ASP.NET Core process, API and browser Web Runtime |

Desktop Web UI and Web Runtime serve different purposes. A production-ready shared renderer is not implemented yet. `WpfCanvasRenderer` remains legacy/prototype code.

The bridge uses camelCase JSON: `element.layout.x/y/width/height` and `element.content.text`. Zoom does not change logical model coordinates.

### Getting started

Requirements: Windows, .NET 8 SDK and Microsoft Edge WebView2 Runtime. Running without the SDK requires .NET 8 Desktop Runtime and ASP.NET Core Runtime 8.

From the repository root:

```powershell
dotnet restore RadishDeck.sln
dotnet build RadishDeck.sln
dotnet run --project src/RadishDeck.Desktop/RadishDeck.Desktop.csproj
```

On Home, select a bind address and port, press Start, then open the control panel. `127.0.0.1` works only on the same computer; select a LAN address for another device. The backend does not support wildcard binding to `0.0.0.0`. Server is built alongside Desktop and runs as a separate process.

Data is stored in `%LOCALAPPDATA%/RadishDeck/deck.v1.json`; server settings are stored in `server.settings.json` in the same directory. API: `GET /status`, `GET /deck`, `POST /execute`.

### Limitations

This is a Designer foundation. Resize, rotation, full property editing, Pages/Layers management in the new shell, multi-selection, undo/redo, custom context menus, advanced grid/snap, widgets, plugins and QR pairing are not implemented. Tablet/Mobile Designer and shared rendering remain future work.

Images in `assets/image concept/` are approved visual references; the current UI is not the final design.

### Validation and documentation

`dotnet test RadishDeck.sln` does not automatically execute the existing console acceptance runner: the checks project is an executable. After a Debug build, run it separately:

```powershell
dotnet run --project tests/RadishDeck.Lifecycle.Tests --no-build -- src/RadishDeck.Desktop/bin/Debug/net8.0-windows/server/RadishDeck.Server.exe tests/RadishDeck.TestServer/bin/Debug/net8.0/RadishDeck.TestServer.exe 127.0.0.1
```

Loopback covers local operation, not LAN connectivity.

[Changelog](CHANGELOG.md) · [Architecture](docs/ARCHITECTURE.md) · [Element model](docs/ELEMENT_MODEL.md) · [0.4 plan](docs/DEVELOPMENT_0.4_PLAN.md)

### Stage 6.1

A canonical shared Web renderer now serves both the Desktop WebView2 Designer and browser Runtime. The Runtime uses the first page, logical 1920×1080 Canvas fallback and proportional visual scaling. Existing persistence and `/execute` action behavior remain compatible.
