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

## Stage 5 — Application Shell and WebView2
- Added local WebView2 shell, navigation, Canvas prototype, and C#↔JavaScript JSON bridge.
- Desktop profile uses 1920×1080 logical coordinates; zoom is visual and drag coordinates are clamped.

## Stage 5 — Application Shell + WebView2 Foundation (completed)

ADDED:
- WebView2 Desktop UI foundation and local Web assets.
- Application shell/navigation and structured C#↔JavaScript JSON bridge.
- Web-based Canvas Designer foundation with Button creation, selection and drag.
- Existing server lifecycle controls integrated into the new shell.

CHANGED:
- WPF MainWindow is now the WebView2 host; WPF remains the platform layer.
- Designer drag uses local JavaScript movement with final C# synchronization.
- Desktop Web UI is documented separately from the browser-based Web Runtime.

FIXED:
- Double JSON parsing and nested Element V2 layout contract mismatches.
- Undefined/NaN Designer values and ElementMove deserialization failures.
- DOM re-render interruption during pointer drag.
- Designer errors appearing as Server errors.
- Default WebView2 browser context menu exposure.

VALIDATED:
- Restore/build/tests, server lifecycle, bind address and port configuration.
- Web Runtime opening, Designer create/select/drag/clamp and persistence after restart.
- WebView2 context-menu suppression; external browser behavior remains unchanged.

Future Designer work includes resize, richer Properties/Pages/Layers, multi-selection, grouping, alignment, undo/redo, custom context actions, responsive profiles, widgets, plugins, QR pairing and production shared rendering.

## Stage 6.1 — Shared Web Renderer Foundation

- Added canonical vanilla shared renderer assets linked into Desktop and Server outputs.
- Desktop Designer now uses the shared visual renderer while preserving selection, drag, clamp and persistence adapters.
- Browser Runtime now uses logical absolute Canvas positioning, first-page rendering and proportional viewport scaling.
- Existing `/deck`, `/execute`, ActionDispatcher and `deck.v1` compatibility remain unchanged.
