namespace RadishDeck.Core.Models.V2;

/// <summary>Canvas coordinates, size and stacking information for an element.</summary>
public sealed class ElementLayout
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public int Layer { get; set; }
    public string? Alignment { get; set; }
}
