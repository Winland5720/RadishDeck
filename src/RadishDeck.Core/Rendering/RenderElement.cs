using RadishDeck.Core.Models;
using RadishDeck.Core.Models.V2;

namespace RadishDeck.Core.Rendering;

/// <summary>Platform-independent data required to render one Element.</summary>
public sealed class RenderElement
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public ElementLayout Layout { get; init; } = new();
    public ElementContent? Content { get; init; }
    public ElementAppearance? Appearance { get; init; }

    /// <summary>Creates render data from an Element carrying V2 layout data.</summary>
    public static RenderElement From(Element element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return new RenderElement
        {
            Id = element.Id,
            Type = element.Type,
            Layout = element.Layout ?? throw new InvalidOperationException("Element V2 Layout is required for rendering"),
            Content = element.Content,
            Appearance = element.Appearance
        };
    }
}
