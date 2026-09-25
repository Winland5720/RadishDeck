namespace RadishDeck.Core.Models.V2;

/// <summary>Platform-independent visual appearance settings.</summary>
public sealed class ElementAppearance
{
    public string? Background { get; set; }
    public string? Color { get; set; }
    public string? Border { get; set; }
    public double? Radius { get; set; }
    public string? Shadow { get; set; }
    public string? Font { get; set; }
    public double? Opacity { get; set; }
}
