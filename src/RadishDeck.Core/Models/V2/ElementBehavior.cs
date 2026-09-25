namespace RadishDeck.Core.Models.V2;

/// <summary>Interaction and state metadata for an element.</summary>
public sealed class ElementBehavior
{
    public string? Hover { get; set; }
    public string? Click { get; set; }
    public string? Animation { get; set; }
    public string? State { get; set; }
}
