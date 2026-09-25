namespace RadishDeck.Core.Rendering;

/// <summary>Abstract display commands supplied to an IRenderer.</summary>
public interface IRenderContext
{
    void Clear();
    void Draw(RenderElement element);
}
