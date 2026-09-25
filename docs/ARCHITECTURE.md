# RadishDeck Architecture v3

## Общая архитектура

    RadishDeck Desktop Designer

              |
              |
           Project

              |
              |
           Deck Model

              |
              |
       RadishDeck Renderer

          /              \

    Desktop Preview    Web Runtime

------------------------------------------------------------------------

# Desktop Designer

Desktop является главным приложением.

Ответственность:

-   визуальное создание интерфейса;
-   редактирование страниц;
-   добавление элементов;
-   управление Canvas;
-   управление профилями устройств;
-   настройка внешнего вида;
-   запуск Server.

------------------------------------------------------------------------

# Canvas

Canvas описывает рабочую область проекта.

Canvas содержит:

    Canvas

    ├── Width
    ├── Height
    ├── Resolution
    ├── Orientation
    ├── Background
    └── Assets

Примеры:

    1920x1080 Landscape
    390x844 Portrait
    1280x800 Landscape

------------------------------------------------------------------------

# Device Profiles

Профиль устройства определяет способ отображения.

    DeviceProfile

    ├── Name
    ├── Width
    ├── Height
    ├── Orientation
    └── Layout Rules

Профили:

-   Desktop;
-   Tablet;
-   Mobile;
-   Custom.

------------------------------------------------------------------------

# Renderer

Renderer является общей частью системы.

Он используется:

-   Desktop Preview;
-   Web Runtime.

Цель:

    What You See Is What You Get

------------------------------------------------------------------------

# Element Architecture

    Element

    ├── Layout
    ├── Content
    ├── Appearance
    ├── Behavior
    └── Action

Element не ограничивается кнопкой.

------------------------------------------------------------------------

# Button

    Button

    ├── Visual
    │   ├── Image
    │   ├── Icon
    │   ├── Text
    │   └── Style
    │
    ├── Behavior
    │   ├── Animation
    │   └── States
    │
    └── Action

------------------------------------------------------------------------

# Web Runtime

Web:

-   получает Deck;
-   использует Renderer;
-   показывает интерфейс;
-   вызывает действия через Server.

Web не является конструктором.

------------------------------------------------------------------------

# Server

Server выполняет действия:

    Element
       |
    Action
       |
    Executor
       |
    Result
       |
    State Update

------------------------------------------------------------------------

# Основной принцип

Desktop создаёт.

Renderer отображает.

Server выполняет.

Web показывает созданное.

## Renderer Contract

До 0.4.0-alpha представления были связаны с конкретными технологиями:

```text
Desktop → WPF Grid
Web     → HTML/CSS
```

Stage 3 вводит независимый Core-контракт:

```text
Element V2
    ↓
Renderer Contract
    ↓
Desktop Renderer
или
Web Renderer
```

`RadishDeck.Core.Rendering` содержит `IRenderer`, `IRenderContext` и
`RenderElement`. Контракт передаёт Identity типа, Canvas Layout, Content и
Appearance. Он не зависит от WPF, XAML, HTML, DOM, CSS, ASP.NET или другого
UI-фреймворка. Конкретный renderer и адаптер контекста будут реализованы
отдельными этапами; Stage 3 не меняет Desktop Preview или Web Runtime.

## Desktop Canvas Renderer

Stage 4 добавляет первый Desktop adapter:

```text
Element V2
    ↓
Renderer Contract
    ↓
WPF Canvas Renderer
```

`WpfCanvasRenderer` использует только `RenderElement` и `IRenderContext`.
WPF-типы находятся в Desktop и не проникают в Core. На этом этапе поддержан
Button, его Canvas-позиция, размеры, текст, фон и граница, а также базовые
выделение и перетаскивание.

## 0.4.0-alpha Architecture Changes

Old:

    Element -> Grid -> Button

New (структура данных):

    Element
    ├── Identity (существующие Id, Name, Type)
    ├── Layout?
    ├── Content?
    ├── Appearance?
    ├── Behavior?
    └── Action?

Stage 1 добавил независимые V2 Core-модели, Stage 2 подключил пять nullable-секций
к существующему Element. Legacy-поля сохранены; преобразование Grid в Canvas
не выполняется. Identity остаётся логической группой, без нового класса.

Незаполненные секции отсутствуют в JSON. Формат и версия deck.v1, DeckJsonStore,
Desktop UI и Web Runtime не менялись. Заполненные секции могут передаваться
стандартной сериализацией, но ещё не используются текущим отображением.
Action System продолжает читать старые привязки. Общий Renderer Contract
предстоит подготовить отдельно; второй Renderer этим этапом не создаётся.

## Stage 5 Application Shell

The WPF host now embeds Microsoft WebView2 and serves local assets from src/RadishDeck.Desktop/Web. The shell provides shared navigation (Главная, Редактор, Логи, Клиенты, Настройки). A JSON bridge uses WebView2 PostWebMessageAsJson and WebMessageReceived; the browser Canvas renders Core Element V2 layout data and reports clamped drag coordinates back to the desktop model. WpfCanvasRenderer remains a legacy/prototype adapter; shared web rendering is the strategic direction.

## Stage 5 — Application Shell + WebView2 Foundation

`RadishDeck.Desktop` is a WPF host for a local WebView2 application. WPF owns window lifecycle, Windows integration and `ServerLauncherService`; local assets live in `src/RadishDeck.Desktop/Web` and require no CDN. The shell provides Главная, Редактор, Логи, Клиенты and Настройки.

Desktop Web UI and Web Runtime are separate products: the former is the RadishDeck application UI inside WebView2, while the latter is the remote user control panel served by `RadishDeck.Server` to browsers and other devices. They may share rendering concepts later, but they do not share a hosting lifetime.

Core remains framework-agnostic. Element V2, CanvasProfile, DeviceProfile, Renderer Contract and RenderElement contain no WPF, WebView2, HTML, CSS or JavaScript dependencies. The Web boundary uses explicit camelCase JSON: render elements contain nested `layout.x`, `layout.y`, `layout.width`, `layout.height` and `content.text`; interaction messages return structured coordinates to C# for model update, persistence and render confirmation.

The prototype supports a logical 1920x1080 Canvas with 50% visual zoom, Button creation, selection, Layers/Properties summaries, pointer-capture drag, local JavaScript movement, logical clamping and persistence across restart. The default WebView2 context menu is disabled through `CoreWebView2Settings.AreDefaultContextMenusEnabled`; this does not affect external-browser Web Runtime behavior.

`WpfCanvasRenderer` remains legacy/prototype code. It is not the strategic Designer direction. At a Canvas boundary the element stops logically even if the physical pointer continues outside the Canvas; future work may improve drag-anchor feedback without locking the Windows cursor.
