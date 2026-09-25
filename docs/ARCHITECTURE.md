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
