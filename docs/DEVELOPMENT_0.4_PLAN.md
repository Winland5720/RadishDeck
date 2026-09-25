# RadishDeck 0.4.0-alpha — план перехода к Canvas Designer

Статус: планирование, исходный код не меняется.

Целевое направление задают `docs/VISION.md` (Vision v3),
`docs/ARCHITECTURE.md` (Architecture v3), `docs/DESIGNER_SPEC.md` и
`docs/ELEMENT_MODEL.md`. Версия 0.4.0-alpha переводит прототип редактора от
Grid-only Button модели к Canvas-based Designer Preview.

## 1. Что есть сейчас

### Element

`src/RadishDeck.Core/Models/Element.cs` содержит плоскую модель:

- `Id`, `Name`, `Type`;
- `GridX`, `GridY`, `GridWidth`, `GridHeight`;
- `ActionId`;
- `Url`, `Color`.

Модель фактически ориентирована на `Button`. Вложенных Identity, Layout,
Content, Appearance и Behavior нет.

### Page и Deck

`Page` содержит имя, идентификатор и `List<Element>`. `Deck` содержит имя,
идентификатор и список страниц. Ни Deck, ни Page пока не имеют Canvas Profile,
Device Profile или ресурсов страницы.

### DeckJsonStore

`src/RadishDeck.Infrastructure/DeckJsonStore.cs` хранит файл
`deck.v1.json` в оболочке `{ schemaVersion: 1, deck: ... }`. Он также читает
старые документы без оболочки версии.

Валидация принимает только `Type == "Button"`, требует `ActionId` и проверяет
границы и пересечения в сетке 3×3. Сохранение сериализует текущую модель и
перезаписывает документ атомарно.

### Renderer

Отдельного общего Renderer в исходном коде пока нет. Desktop Preview
реализован в `MainWindow.RenderDeck()` через WPF `Grid`, `RowDefinitions`,
`ColumnDefinitions` и `Grid.SetColumn/Row`. Это текущий прототип отображения,
а не целевой Renderer из Architecture v3.

Web Runtime также не использует общий Renderer: `src/RadishDeck.Server/wwwroot/app.js`
строит обычные HTML-кнопки, а `style.css` задаёт фиксированную CSS-сетку.

### Desktop UI

`MainWindow.xaml` содержит список страниц, центральный `DeckGrid` и панель
свойств Button. `MainWindow.xaml.cs` связывает их с Grid-полями Element и
`DeckEditor`. Добавление, перемещение и изменение размера ограничены сеткой
3×3; Drag & Drop, Canvas Profile, Edit/Preview Mode и библиотека элементов
пока отсутствуют.

## 2. Цели 0.4.0-alpha

В рамках версии нужно подготовить основу, а не сразу реализовывать весь набор
будущих Widgets и Plugins:

1. Ввести Canvas Profile и Device Profile как понятия Deck/страницы.
2. Определить Element v2 с Identity, Layout, Content, Appearance, Behavior и
   Action.
3. Выделить общий Renderer contract для Desktop Preview и Web Runtime.
4. Сделать Designer Preview Canvas-based, сохранив редактирование страниц и
   действий.
5. Читать старые `deck.v1.json` и не терять их при ошибке миграции.

## 3. Предлагаемая модель Canvas Profile

Canvas Profile следует сделать частью проекта или страницы согласно принятому
решению о наследовании. Минимальный контракт:

- `Name`;
- `Width`, `Height`;
- `Resolution`;
- `Orientation` (`Landscape` или `Portrait`);
- `Background`;
- `Assets`;
- выбранный `DeviceProfile` и его Layout Rules.

Стандартные профили: Desktop, Tablet, Mobile и Custom. Единицы координат
должны быть определены один раз и использоваться одинаково Preview и Web
Runtime. Grid может остаться вспомогательным snap/guide-инструментом, но не
ограничением модели.

## 4. Предлагаемая Element v2

Element v2 должна быть вложенной платформенно независимой моделью:

```text
Element
├── Identity
├── Layout       (x, y, width, height, layer, alignment)
├── Content      (text, image, icon, logo, media)
├── Appearance   (background, color, border, radius, shadow, font, opacity)
├── Behavior     (hover, click, animation, states)
└── Action       (optional binding)
```

Первый набор типов: Button, Text, Image, Container. Action не должен быть
обязательным для Text, Image и Container. `ActionId` переходит в новую
структуру Action без изменения смысла действия.

## 5. План Designer Preview

1. Ввести единый `IRenderer`/renderer contract в Core или отдельном общем
   проекте без зависимости от WPF и DOM.
2. Реализовать Canvas renderer для Desktop Preview; WPF должен отображать
   координаты и размеры Element v2, а не вызывать `Grid.SetColumn/Row`.
3. Выделить edit operations: select, move, resize, layer order, copy, delete.
4. Добавить Elements Library для Button, Text, Image и Container.
5. Разделить Edit Mode и Preview Mode.
6. Перевести Web Runtime на тот же семантический контракт Renderer. Нельзя
   поддерживать независимую трактовку Layout в `app.js` и Desktop.

На первом шаге допустим адаптер существующего Button Preview, но он должен
быть временным и покрываться теми же Renderer contract checks.

## 6. Обратная совместимость

Переход не должен молча ломать текущие документы.

### Чтение

- `DeckJsonStore` продолжает читать `schemaVersion: 1`;
- продолжает читать прежний unversioned документ;
- v1 Element преобразуется во внутренний Element v2 adapter:
  `GridX/GridY` → координаты Canvas, `GridWidth/GridHeight` → размеры,
  `ActionId` → Action binding;
- старый `Color` переносится в Appearance color/background по утверждённому
  правилу;
- неизвестные будущие поля не должны ломать чтение, если документ валиден.

### Запись

До согласования новой схемы нельзя менять смысл `deck.v1.json`. Возможны два
явно выбранных режима:

1. сохранить v1 без потери данных для старого редактора;
2. записывать новую версию отдельным форматом после явной миграции и backup.

Автоматическое повышение версии при обычном сохранении не допускается. Нужно
зафиксировать целевой `schemaVersion` и стратегию rollback отдельной задачей.

### Миграция координат

Миграция должна знать Canvas Profile. Если старый Grid 3×3 не имеет
абсолютного размера Canvas, выбрать профиль и правило масштабирования нужно
до написания кода. Преобразование должно быть детерминированным и проверять
пересечения уже в Canvas units.

## 7. Файлы, которые потребуют изменения

### Core

- `src/RadishDeck.Core/Models/Element.cs` — Element v2 или переходный DTO;
- новый `src/RadishDeck.Core/Models/CanvasProfile.cs`;
- новый `src/RadishDeck.Core/Models/DeviceProfile.cs`;
- новый `src/RadishDeck.Core/Models/ElementLayout.cs`;
- новые Content/Appearance/Behavior/Action binding модели;
- `src/RadishDeck.Core/Models/Page.cs` — Canvas Profile и page-level settings;
- `src/RadishDeck.Core/Models/Deck.cs` — project profiles/assets, если они
  будут принадлежать Deck;
- `src/RadishDeck.Core/Models/DeckEditor.cs` — Canvas coordinate operations,
  resize, layer and overlap rules.

### Persistence

- `src/RadishDeck.Infrastructure/DeckJsonStore.cs` — version detection,
  v1 adapter, validation, backup/migration policy;
- новый migration/compatibility service, если адаптер нельзя оставить внутри
  store;
- tests in `tests/RadishDeck.Lifecycle.Tests/DeckChecks.cs` и отдельные Core
  model tests.

### Renderer и Server Runtime

- новый общий Renderer contract/project;
- `src/RadishDeck.Desktop/MainWindow.xaml` — Canvas, library, properties,
  modes;
- `src/RadishDeck.Desktop/MainWindow.xaml.cs` — preview/edit operations;
- `src/RadishDeck.Server/wwwroot/app.js` — render Element v2;
- `src/RadishDeck.Server/wwwroot/style.css` — Canvas/device layout;
- `src/RadishDeck.Server/Program.cs` — only if API response needs explicit
  schema/profile metadata.

### Documentation and build

- `docs/ELEMENT_MODEL.md` — уточнить принятые единицы и версию схемы;
- `docs/DESIGNER_SPEC.md` — связать Preview operations с реализацией;
- `docs/ARCHITECTURE.md` — зафиксировать Renderer contract;
- `docs/BUILD.md`, `docs/TESTING.md`, `CHANGELOG.md`;
- соответствующие `.csproj`, если появится общий Renderer project.

## 8. Порядок маленьких шагов

1. Утвердить Canvas Profile, Device Profile, единицы и правила scaling.
2. Добавить чистые Core DTO без изменения старого `Element`.
3. Реализовать read-only v1 adapter и тесты старых JSON.
4. Ввести Renderer contract и эталонный snapshot/contract test.
5. Перевести Desktop Preview с Grid на Canvas adapter.
6. Добавить Edit Mode operations и Elements Library.
7. Перевести Web Runtime на тот же contract.
8. Только после этого выбрать формат записи Element v2 и стратегию миграции.

После каждого шага обязательны `dotnet build RadishDeck.sln`, regression tests
для v1 и проверка, что старые Deck открываются без потери данных.

## 9. Что не входит автоматически

План не подразумевает автоматическое добавление WebSocket, SQLite, Plugins,
новых Actions, мобильного клиента или полной системы Themes/Animations.
Это отдельные этапы и могут быть подключены только после согласования их
контрактов с Canvas и Renderer.

## 10. Критерии готовности 0.4.0-alpha

- Canvas Profile и Device Profile имеют согласованный контракт;
- Element v2 описан и доступен Renderer без WPF/DOM-зависимостей;
- Desktop Preview больше не зависит от Grid-only размещения;
- Web Runtime использует ту же семантику Layout;
- Button, Text, Image и Container имеют минимальный Preview path;
- старый `deck.v1.json` загружается и сохраняется по выбранной политике;
- тесты миграции, рендеринга и обратной совместимости проходят;
- документация и changelog обновлены.

## Stage 5 completion

Stage 5 is complete: the Desktop WebView2 shell, local Web UI assets, navigation, structured C#↔JavaScript bridge, Canvas/Button Designer foundation, server lifecycle controls, persistence round trip and host-level context-menu suppression are implemented and manually validated. Resize, full Properties/Pages/Layers, multi-selection, undo/redo, widgets, QR pairing, production shared rendering and full Logs/Clients/Settings remain future work.
