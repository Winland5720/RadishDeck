namespace RadishDeck.Core.Models.V2;

/// <summary>Optional future action binding for an Element v2.</summary>
public sealed class ElementAction
{
    public string? ActionId { get; set; }
    public string? Type { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
}
