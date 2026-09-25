# RadishDeck Technology Stack

Целевая архитектура: [Architecture v3](ARCHITECTURE.md).

## Реализованная основа

- C# / .NET 8.
- WPF — оболочка Windows Desktop Designer.
- ASP.NET Core — отдельный процесс Server.
- Core — общие модели и контракты без зависимости от UI.
- JSON — текущее хранение Deck и локальных настроек Server.

## Целевое отображение

Общий Renderer используется в Desktop Preview и Web Runtime.
План интеграции HTML Renderer в Desktop предусматривает WebView2.
WPF-оболочка не должна становиться вторым независимым Renderer элементов.

## Будущие подсистемы

REST API развивается для взаимодействия клиентов с Server.
WebSocket, SQLite и плагины описывают будущие возможности и не являются
обязательными зависимостями текущего прототипа. Их внедрение требует
отдельного этапа и не меняет распределение ролей:

Desktop создаёт → Deck → Renderer → Desktop Preview / Web Runtime.
Web вызывает действия → Server → Action System → Executor.
