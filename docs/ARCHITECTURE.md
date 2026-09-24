# RADISH DECK Architecture
Version: 0.2.1-alpha

User
 |
RadishDeck.Desktop Designer
 |
RadishDeck.Server
 |
Web / Mobile / Tablet

Server является единственным исполнителем действий.

В версии 0.2.2-alpha Server получает текущую колоду через существующий
`DeckJsonStore` и предоставляет только read-only endpoint `GET /deck`.
Формат хранения остаётся `deck.v1.json`; Desktop пока продолжает загружать
и сохранять Deck локально.

Element -> Action -> Executor -> Result -> State Update

Используется единый HTML Renderer через WebView2.
