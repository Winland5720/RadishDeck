# RadishDeck Development Plan

Источник целевого направления: [Vision v3](VISION.md),
[Architecture v3](ARCHITECTURE.md), [Designer Specification](DESIGNER_SPEC.md).
Это план, а не перечень уже реализованных функций.

## Текущая основа

Core содержит Deck, Page, Element и Action System.
Desktop редактирует локальный Deck и управляет отдельным процессом Server.
Server предоставляет GET /deck и выполнение действий; Web — прототип runtime.
Текущее JSON-хранение не меняется автоматически из-за обновления концепции.

## 0.4 — основа Designer

- Canvas и профили устройств.
- Elements Library: Button, Text, Image, Container.
- Properties: Layout, Content, Appearance, Behavior, Action.
- Preview через общий Renderer для Desktop и Web.

## 0.5 — визуальное редактирование

- Drag & Drop.
- Animations.
- Themes.
- Templates.

## 0.6 — расширения

- Widgets.
- Monitoring.
- Plugins.

## Дальнейшие возможности

REST API, синхронизация через WebSocket, мобильные клиенты, pairing,
права доступа и развитие хранения реализуются отдельными согласованными
задачами. Старый план «Server owns Deck» не является требованием перенести
Designer или авторство проекта на Server.

Каждый этап сохраняет работоспособность существующих функций, сопровождается
проверками, сборкой и документацией. Конкретные ограничения задачи определяют
объём работ; дорожная карта сама по себе не требует миграции Deck.
