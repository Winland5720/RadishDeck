# RADISH DECK

# ARCHITECTURE

Версия документа:

Alpha 0.1.0


---

# 1. Общая архитектура


Radish Deck состоит из:


             RadishDeck.exe

                   |

    --------------------------------

    |              |              |

   Core          Server          UI

    |              |              |

    |          API/WebSocket      |

    |              |              |

    --------- Renderer ------------

                   |

          Radish Deck Client


---

# 2. Главный принцип


Radish Deck использует единую модель данных.


Источник истины:

Database


Поток данных:


Database

↓

Layout Model

↓

Renderer

↓

Desktop UI

↓

Web Client

↓

Mobile Client



Все клиенты используют одинаковый Layout.



---

# 3. Desktop Application


Файл:



RadishDeck.exe



Desktop приложение является главным компонентом.


Отвечает за:


- запуск программы;
- управление сервером;
- настройки;
- Designer;
- выполнение действий;
- управление плагинами;
- отображение интерфейса.


---

# 4. Core


Core — основная логика приложения.


Отвечает за:


- конфигурацию;
- состояние приложения;
- работу модулей;
- управление данными.


Core не должен зависеть от UI.

В Alpha 0.1.3 в Core добавлены независимые модели `Element`, `Action` и
`State` в папке `Models`. Они содержат только данные и не зависят от Desktop,
Server, Infrastructure, UI, базы данных или сторонних библиотек.



---

# 5. Server


Server встроен в RadishDeck.exe.


Отдельный Server Manager не создаётся.



Server отвечает за:


## API


REST API:


Используется для:


- получения данных;
- изменения настроек;
- управления объектами.



## WebSocket


Используется для:


- обновления состояния;
- отправки команд;
- синхронизации клиентов.



---

# 6. Database


Используется SQLite.


База хранит:


## Settings


Настройки:


- IP;
- Port;
- параметры сервера.



## Pages


Страницы интерфейса.


Пример:



Home

PC

Sound

Apps

System

Docker



## Elements


Элементы страниц.


Хранят:


- тип;
- позицию;
- размер;
- свойства.



## Actions


Действия.


Например:



open_app

shutdown

volume_up

docker_restart




---

# 7. Layout System


Layout отвечает за расположение элементов.


Используется Grid Layout.


Не использовать:



X:190

Y:150

Width:300

Height:200




Использовать:



grid_x

grid_y

grid_width

grid_height




Пример:


```json
{
"type":"button",
"name":"Docker",
"grid_x":0,
"grid_y":0,
"grid_width":2,
"grid_height":1
}
8. Renderer

Renderer отвечает за отображение интерфейса.

Один Renderer используется для:

Desktop;
Web;
Mobile.

Renderer получает:

Layout

+

State

+

Theme

И создаёт интерфейс.

9. Element System

Каждый элемент состоит из:

Element

|

├── Type

├── Properties

├── State

├── Action

└── Style

Пример:

Button:

Type:

button


State:

enabled


Action:

open_app
10. Action Engine

Action Engine выполняет команды.

Поток:

User Click

↓

Element

↓

Action

↓

Executor

↓

Result

↓

Update State

Пример:

Button:

Steam


Action:

start_program


Executor:

Windows API


Result:

Running
11. Plugin System

Плагины расширяют возможности.

Plugin может добавлять:

Elements;
Actions;
Services.

Примеры:

Docker Plugin

Добавляет:

Elements:

Container Status

Actions:

Start
Stop
Restart
Logs
12. Client Architecture

Radish Deck Client:

Не хранит логику.

Получает:

Layout;
Elements;
State.

Отправляет:

Events;
Commands.
13. Network

Система не должна использовать жёсткие адреса.

Запрещено:

localhost

127.0.0.1

фиксированный IP

Система должна:

определить сетевые интерфейсы;
показать доступные IP;
позволить выбрать интерфейс;
позволить изменить Port.
14. Security

Будущая система:

Pairing;
Authentication;
Client management.
15. Ошибки архитектуры

Запрещено:

❌ отдельный Web Renderer

❌ отдельный Mobile Renderer

❌ отдельная логика клиента

❌ копирование состояния между клиентами

Правильно:

Одна модель.

Один Renderer.

Много клиентов.

16. Цель архитектуры

Radish Deck должен быть:

расширяемым;
стабильным;
модульным;
готовым к плагинам.

Главный принцип:

State

+

Action

+

Interface

---

# Application Version Source

Единый источник версии приложения находится в:

    src/RadishDeck.Core/AppVersion.cs

Desktop и Server используют `RadishDeck.Core.AppVersion.Current`. Версия не
должна дублироваться в разных проектах или задаваться отдельными строками в
клиенте и сервере. Значение всегда использует полный формат с этапом
разработки, например `0.1.3-alpha`.

`Directory.Build.props` получает из этого же файла значение `Version` для
всех проектов, чтобы метаданные сборок не требовали ручного дублирования.
