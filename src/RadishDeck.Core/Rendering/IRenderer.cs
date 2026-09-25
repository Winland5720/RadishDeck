namespace RadishDeck.Core.Rendering;

/// <summary>Common rendering contract shared by Desktop Preview and Web Runtime.</summary>
public interface IRenderer
{
    void Render(IReadOnlyCollection<RenderElement> elements, IRenderContext context);
}
