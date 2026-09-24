namespace RadishDeck.Core.Models;

// Shared editing rules, independent of WPF and persistence.
public static class DeckEditor
{
    public const int GridSize = 3;

    public static Page AddPage(Deck deck)
    {
        var page = new Page { Name = "Page " + (deck.Pages.Count + 1) };
        deck.Pages.Add(page);
        return page;
    }

    public static void RenamePage(Page page, string name) => page.Name = RequiredName(name);

    public static void DeletePage(Deck deck, Page page)
    {
        if (deck.Pages.Count <= 1) throw new InvalidOperationException("Cannot delete the last page");
        if (!deck.Pages.Remove(page)) throw new InvalidOperationException("Page not found");
    }

    public static Element AddButton(Page page)
    {
        for (var y = 0; y < GridSize; y++)
        for (var x = 0; x < GridSize; x++)
        {
            if (!Fits(page, null, x, y, 1, 1)) continue;
            var element = new Element { Name = "Button", Type = "Button", ActionId = "server.start", GridX = x, GridY = y };
            page.Elements.Add(element);
            return element;
        }
        throw new InvalidOperationException("Page is full; delete or resize an element first");
    }

    public static void UpdateElement(Page page, Element element, string name, string actionId, string url,
        int x, int y, int width, int height)
    {
        if (!page.Elements.Contains(element)) throw new InvalidOperationException("Element not found on this page");
        name = RequiredName(name);
        if (!Fits(page, element, x, y, width, height))
            throw new InvalidOperationException("Position/size is outside the 3x3 grid or overlaps another element");
        element.Name = name;
        element.ActionId = actionId;
        element.Url = url;
        element.GridX = x;
        element.GridY = y;
        element.GridWidth = width;
        element.GridHeight = height;
    }

    public static bool Fits(Page page, Element? excluded, int x, int y, int width, int height) =>
        x >= 0 && y >= 0 && width >= 1 && height >= 1 && width <= GridSize && height <= GridSize &&
        x <= GridSize - width && y <= GridSize - height &&
        !page.Elements.Any(e => e != excluded && x < e.GridX + e.GridWidth && x + width > e.GridX &&
                               y < e.GridY + e.GridHeight && y + height > e.GridY);

    private static string RequiredName(string name) =>
        string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Name is required") : name.Trim();
}
