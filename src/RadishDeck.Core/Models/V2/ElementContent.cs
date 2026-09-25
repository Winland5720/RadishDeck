namespace RadishDeck.Core.Models.V2;

/// <summary>Visual content supplied to an element.</summary>
public sealed class ElementContent
{
    public string? Text { get; set; }
    public string? Image { get; set; }
    public string? Icon { get; set; }
    public string? Logo { get; set; }
    public string? Media { get; set; }
}
