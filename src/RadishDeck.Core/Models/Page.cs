namespace RadishDeck.Core.Models;

public sealed class Page
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "Main";
    public List<Element> Elements { get; set; } = new();
}
