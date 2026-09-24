namespace RadishDeck.Core.Models;

public sealed class Element
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public int GridX { get; set; }
    public int GridY { get; set; }
    public int GridWidth { get; set; } = 1;
    public int GridHeight { get; set; } = 1;
    public string ActionId { get; set; } = string.Empty;
    public string Url { get; set; } = "https://github.com/Winland5720/RadishDeck";
    public string Color { get; set; } = "#E11D2E";
}
