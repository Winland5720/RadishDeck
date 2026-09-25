> Исторические записи по версиям. Они не заменяют актуальные Vision v3, Architecture v3 и Designer Specification и не подтверждают реализацию перечисленных концепций.

# CHANGELOG

## 0.2.1-alpha

- Desktop стал конструктором
- Server стал мозгом системы
- Web/Mobile клиенты
- Единый HTML Renderer

## 0.4.0-alpha

Added:

- Canvas Designer core models.
- Element V2 foundation.
- New layout/content/appearance model.
- Nullable Layout, Content, Appearance, Behavior and Action on legacy Element.
- Element serialization and backward compatibility checks.

Stage 1–2 foundation only: existing UI, rendering, Action System and Deck store
remain unchanged. No migration or new Deck schema is introduced.

### Renderer Contract foundation

- Added platform-independent `IRenderer`, `IRenderContext` and `RenderElement`.
- Added Core checks for Element V2 to render-data mapping.
- Desktop Preview and Web Runtime remain unchanged.

### Desktop Canvas Preview

- Added the first WPF Canvas Renderer implementation.
- Added Canvas Button preview with basic selection and dragging.
- Added Canvas coordinate editing in the Desktop properties panel.
