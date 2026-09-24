# RADISH DECK

# BUILD DOCUMENTATION

Версия:

Alpha 0.2.0


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

dotnet publish src/RadishDeck.Desktop -c Release


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

---

# 9. Текущий каркас (Alpha 0.2.0, Step 1)

Создан `RadishDeck.sln` с четырьмя проектами:

- `src/RadishDeck.Core` — библиотека .NET 8 без внешних зависимостей.
- `src/RadishDeck.Infrastructure` — библиотека .NET 8 со ссылкой на Core.
- `src/RadishDeck.Desktop` — WPF-приложение (`net8.0-windows`) со ссылкой на Core.
  `App.xaml` задаёт точку входа, `MainWindow.xaml` содержит главное окно управления Server;
  файлы `.xaml.cs` содержат минимальный код приложения и окна.
- `src/RadishDeck.Server` — ASP.NET Core-приложение (.NET 8) со ссылкой на Core.
  `Program.cs` создаёт и запускает host без маршрутов API.

Каждый проект содержит свой `.csproj`. Версия сборок: `0.1.0-alpha`.
Core и Infrastructure пока не содержат прикладного кода.

Для сборки нужны Windows и .NET SDK 8 или новее. Для запуска этих
framework-dependent приложений нужны .NET 8 Desktop Runtime и ASP.NET Core
Runtime 8 (новая версия SDK сама по себе не заменяет среды выполнения .NET 8).

Команды выполняются из корня репозитория:

```powershell
dotnet build RadishDeck.sln
dotnet build RadishDeck.sln -c Release

# Desktop: открывается окно управления жизненным циклом Server.
dotnet run --project src/RadishDeck.Desktop

# Server: укажите IPv4 выбранного сетевого интерфейса этого компьютера.
$radishDeckIp = Read-Host 'IP сетевого интерфейса'
dotnet run --project src/RadishDeck.Server -- --urls "http://${radishDeckIp}:8080" # только локальная разработка Server отдельно
```

Desktop и Server запускаются отдельно. Сервер останавливается через Ctrl+C.
Адрес передаётся через стандартную настройку ASP.NET Core `--urls`;
фиксированный IP и профиль запуска с localhost в проект не добавлены.
На текущем шаге сервер предоставляет `GET /status`; остальные HTTP-запросы
пока возвращают 404.

Порядок запуска из раздела 6 — план следующих шагов: Splash Screen,
база данных, запуск сервера из Desktop, API, WebSocket и плагины пока
не реализованы.

## Alpha 0.2.0 — publish и Server lifecycle

Desktop и Server остаются отдельными процессами. Публикация Desktop автоматически публикует Server в подпапку `server`:

```powershell
dotnet build RadishDeck.sln -c Release
dotnet publish src/RadishDeck.Desktop -c Release -o publish/desktop
```

В результате `publish/desktop/server/RadishDeck.Server.exe` поставляется вместе с Desktop. Desktop ищет Server относительно `AppContext.BaseDirectory`, поэтому скопированная publish-папка не зависит от рабочей директории и исходников репозитория. Server запускается напрямую, без `dotnet run`.

Start фиксирует IP и Port. Состояния UI: `Stopped`, `Starting`, `Running`, `Stopping`, `Error`; IP и Port редактируются только после полной остановки. `/status` принимается только при совпадении имени, версии, `running` и PID запущенного дочернего процесса. stdout/stderr читаются асинхронно; в интерфейсе доступен ограниченный диагностический хвост.

Интеграционная проверка (после build/publish):

```powershell
dotnet run --project tests/RadishDeck.Lifecycle.Tests -c Release --no-build -- `
  publish/desktop/server/RadishDeck.Server.exe `
  tests/RadishDeck.TestServer/bin/Release/net8.0/RadishDeck.TestServer.exe `
  10.16.0.20
```

Она проверяет опубликованный Server, гонки Start/Stop, отмену, аварийное завершение, неверный `/status`, занятый порт и потоки диагностики.



## 0.2.0-alpha Deck verification

The lifecycle test executable also runs the Deck acceptance checks. It verifies registry lookup and duplicate handling, dispatcher routing, unknown/failing actions, Page/Element editing rules, 3x3 collision checks, versioned JSON roundtrip, malformed-file preservation, concurrent saves, legacy migration and URL validation.
