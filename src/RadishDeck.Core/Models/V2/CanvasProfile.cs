namespace RadishDeck.Core.Models.V2;

/// <summary>Describes the drawable Canvas area used by a project or page.</summary>
public sealed class CanvasProfile
{
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public CanvasOrientation Orientation { get; set; } = CanvasOrientation.Landscape;
    public string? Background { get; set; }
    public List<string> Assets { get; set; } = new();
}

/// <summary>Supported Canvas orientations.</summary>
public enum CanvasOrientation { Landscape, Portrait }
