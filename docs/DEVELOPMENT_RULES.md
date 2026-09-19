# Radish Deck Development Rules

## Правила разработки проекта Radish Deck

------------------------------------------------------------------------

## Documentation First

Каждая задача разработки должна завершаться обновлением документации.

Every development task must include documentation updates.

Перед завершением задачи необходимо проверить:

Before completing a task:

1.  Код реализован.
2.  Проект успешно собирается.
3.  Документация обновлена.
4.  CHANGELOG.md обновлён.
5.  Изменения подготовлены для Git commit.

------------------------------------------------------------------------

## Task Completion Criteria

## Критерии завершения задачи

Задача считается завершённой только если:

-   функциональность реализована;
-   выполнена проверка сборки;
-   обновлены необходимые документы;
-   добавлена запись в CHANGELOG.md;
-   подготовлено описание изменений.

A task is considered completed only when:

-   functionality is implemented;
-   build verification is completed;
-   required documentation is updated;
-   CHANGELOG.md is updated;
-   changes are described.

------------------------------------------------------------------------

## Documentation Updates

## Обновление документации

При необходимости обновляются:

Update when required:

### CHANGELOG.md

История изменений проекта.

Project change history.

### README.md

Основная информация о проекте.

Main project information.

### docs/BUILD.md

Инструкции сборки и запуска.

Build and run instructions.

### docs/ARCHITECTURE.md

Архитектура системы.

System architecture.

### docs/PROJECT_STRUCTURE.md

Структура проекта.

Project structure.

### docs/DEVELOPMENT_PLAN.md

План развития.

Development roadmap.

------------------------------------------------------------------------

## Codex Workflow Rules

## Правила работы Codex

Перед выполнением задачи:

1.  Изучить документацию проекта.
2.  Проверить текущую версию проекта.
3.  Проверить существующую архитектуру.
4.  Не менять архитектуру без необходимости.

Before starting a task:

1.  Read project documentation.
2.  Check current project version.
3.  Check existing architecture.
4.  Do not change architecture without necessity.

------------------------------------------------------------------------

После выполнения задачи:

1.  Выполнить сборку проекта.

Example:

``` bash
dotnet build RadishDeck.sln
```

2.  Проверить работоспособность.

3.  Обновить документацию.

4.  Обновить CHANGELOG.md.

5.  Подготовить Git commit message.

------------------------------------------------------------------------

## Versioning

Версии проекта используют:

MAJOR.MINOR.PATCH

Примеры:

    0.1.0-alpha
    0.1.1-alpha
    0.1.2-alpha
    0.2.0-alpha
    0.1.0-beta
    0.1.2-beta
    0.1.3-beta
    1.0.0-beta
    1.1.0-beta
    1.0.0-release

Каждая значимая функция должна быть привязана к версии.

------------------------------------------------------------------------

## Development Philosophy

Главный принцип Radish Deck:

Сначала рабочая функциональность.

Потом улучшение интерфейса.

Не создавать сложный дизайн до подтверждения работы системы.

Build functionality first.

Improve interface later.

Do not create complex design before validating system functionality.
