namespace RadishDeck.Core.Models;

public sealed class Action
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}
