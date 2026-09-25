using System.Text.Json.Serialization;
using RadishDeck.Core.Models.V2;

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

    /// <summary>Optional Canvas geometry; does not replace legacy Grid fields yet.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ElementLayout? Layout { get; set; }

    /// <summary>Optional visual content for the future Designer.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ElementContent? Content { get; set; }

    /// <summary>Optional platform-independent appearance.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ElementAppearance? Appearance { get; set; }

    /// <summary>Optional interaction metadata.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ElementBehavior? Behavior { get; set; }

    /// <summary>Optional future binding; execution still uses legacy ActionId.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ElementAction? Action { get; set; }
}
