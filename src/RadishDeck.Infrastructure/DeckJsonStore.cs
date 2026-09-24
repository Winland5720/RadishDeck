using System.Text.Json;
using RadishDeck.Core.Models;

namespace RadishDeck.Infrastructure;

public sealed class DeckJsonStore
{
    private readonly string _path;
    private readonly SemaphoreSlim _writes = new(1, 1);
    private bool _loadFailed;
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    public string FilePath => _path;
    public sealed record Document(int SchemaVersion, Deck Deck);

    public DeckJsonStore(string? path = null) => _path = Path.GetFullPath(path ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RadishDeck", "deck.v1.json"));

    public async Task<Deck> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await File.ReadAllTextAsync(_path, cancellationToken);
            using var parsed = JsonDocument.Parse(json);
            Deck? deck;
            if (parsed.RootElement.TryGetProperty("schemaVersion", out var version))
            {
                if (version.GetInt32() != 1) throw new InvalidDataException("Unsupported Deck format version");
                deck = JsonSerializer.Deserialize<Document>(json, Options)?.Deck;
            }
            else
            {
                // Read the unversioned file produced by the interrupted implementation.
                deck = JsonSerializer.Deserialize<Deck>(json, Options);
            }
            Validate(deck);
            return deck!;
        }
        catch (FileNotFoundException) { return CreateDefault(); }
        catch (DirectoryNotFoundException) { return CreateDefault(); }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _loadFailed = true;
            throw new InvalidDataException($"Deck could not be loaded. Original file preserved: {_path}. {exception.Message}", exception);
        }
    }

    public Task SaveAsync(Deck deck, CancellationToken cancellationToken = default)
    {
        if (_loadFailed) throw new InvalidOperationException("Saving is disabled because Deck loading failed; repair the file and restart");
        Validate(deck);
        // Capture the model before the first await: later edits cannot alter an in-flight save.
        var bytes = JsonSerializer.SerializeToUtf8Bytes(new Document(1, deck), Options);
        return WriteAsync(bytes, cancellationToken);
    }

    private async Task WriteAsync(byte[] bytes, CancellationToken cancellationToken)
    {
        await _writes.WaitAsync(cancellationToken);
        var temporary = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            await File.WriteAllBytesAsync(temporary, bytes, cancellationToken);
            File.Move(temporary, _path, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
            _writes.Release();
        }
    }

    private static void Validate(Deck? deck)
    {
        if (deck is null || deck.Pages is null || deck.Pages.Count == 0 || string.IsNullOrWhiteSpace(deck.Name))
            throw new InvalidDataException("Deck must contain at least one page and a name");
        var ids = new HashSet<Guid> { deck.Id };
        foreach (var page in deck.Pages)
        {
            if (page is null || !ids.Add(page.Id) || string.IsNullOrWhiteSpace(page.Name) || page.Elements is null)
                throw new InvalidDataException("Invalid page or duplicate ID");
            foreach (var element in page.Elements)
            {
                if (element is null || !ids.Add(element.Id) || string.IsNullOrWhiteSpace(element.Name) ||
                    element.Type != "Button" || string.IsNullOrWhiteSpace(element.ActionId) ||
                    !DeckEditor.Fits(page, element, element.GridX, element.GridY, element.GridWidth, element.GridHeight))
                    throw new InvalidDataException("Invalid element, duplicate ID or overlapping layout");
            }
        }
    }

    public static Deck CreateDefault() => new()
    {
        Pages = new()
        {
            new Page
            {
                Name = "Main",
                Elements = new()
                {
                    new Element { Name = "Start Server", Type = "Button", ActionId = "server.start", GridX = 0 },
                    new Element { Name = "Stop Server", Type = "Button", ActionId = "server.stop", GridX = 1 },
                    new Element { Name = "Open Radish Deck", Type = "Button", ActionId = "url.open", GridX = 2 }
                }
            }
        }
    };
}
