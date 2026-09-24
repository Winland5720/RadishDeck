namespace RadishDeck.Core.Models;

public sealed class Deck
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "Radish Deck";
    public List<Page> Pages { get; set; } = new();
}
