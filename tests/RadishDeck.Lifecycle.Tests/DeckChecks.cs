using System.Text.Json;
using RadishDeck.Core.Actions;
using RadishDeck.Core.Execution;
using RadishDeck.Core.Models;
using RadishDeck.Desktop.Services;
using RadishDeck.Infrastructure;
using CoreAction = RadishDeck.Core.Models.Action;

internal static class DeckChecks
{
    public static async Task RunAsync(Func<string, Func<Task>, Task> test)
    {
        await test("Registry registration, lookup, duplicate rejection", async () =>
        {
            var registry = new ActionRegistry();
            var executor = new FakeExecutor();
            registry.Register(new("test", "Test", "Tests"), executor);
            Check(registry.TryGet("test", out var definition, out var resolved) && definition!.Name == "Test" && resolved == executor);
            Check(registry.Definitions.Count == 1 && !registry.TryGet("missing", out _, out _));
            await Reject(() => { registry.Register(new("test", "Duplicate", "Tests"), executor); return Task.CompletedTask; });
        });
        await test("Dispatcher routes correct executor and passes request", async () =>
        {
            var first = new FakeExecutor();
            var second = new FakeExecutor();
            var registry = new ActionRegistry();
            registry.Register(new("first", "First", "Tests"), first);
            registry.Register(new("second", "Second", "Tests"), second);
            var element = new Element();
            var action = new CoreAction { Type = "second" };
            var state = await new ActionDispatcher(registry).ExecuteAsync(element, action);
            Check(state.Status == "Success" && first.Calls == 0 && second.Calls == 1 && second.Element == element && second.Action == action);
        });
        await test("Dispatcher unknown Action and executor failure", async () =>
        {
            var registry = new ActionRegistry();
            registry.Register(new("fail", "Fail", "Tests"), new FakeExecutor { Fail = true });
            var dispatcher = new ActionDispatcher(registry);
            Check((await dispatcher.ExecuteAsync(new(), new CoreAction { Type = "missing" })).Status == "Error");
            var error = await dispatcher.ExecuteAsync(new(), new CoreAction { Type = "fail" });
            Check(error.Status == "Error" && error.Message == "Test failure");
        });
        await test("Pages create, rename, delete, final page protection", async () =>
        {
            var deck = DeckJsonStore.CreateDefault();
            var page = DeckEditor.AddPage(deck);
            DeckEditor.RenamePage(page, " Work ");
            Check(page.Name == "Work" && deck.Pages.Count == 2);
            DeckEditor.DeletePage(deck, page);
            await Reject(() => { DeckEditor.DeletePage(deck, deck.Pages[0]); return Task.CompletedTask; });
            Check(deck.Pages.Count == 1);
        });
        await test("Elements create, rename, move, resize, delete and collision validation", async () =>
        {
            var page = new Page();
            var element = DeckEditor.AddButton(page);
            DeckEditor.UpdateElement(page, element, "Run", "url.open", "https://example.com/", 1, 1, 2, 2);
            Check(element.GridX == 1 && element.GridWidth == 2 && element.Name == "Run" && element.ActionId == "url.open");
            var second = DeckEditor.AddButton(page);
            await Reject(() => { DeckEditor.UpdateElement(page, second, "Overlap", "server.start", "", 1, 1, 1, 1); return Task.CompletedTask; });
            await Reject(() => { DeckEditor.UpdateElement(page, element, "Outside", "server.start", "", 2, 2, 2, 2); return Task.CompletedTask; });
            Check(element.Name == "Run");
            Check(page.Elements.Remove(element) && page.Elements.Count == 1);
        });
        await test("Full grid rejects tenth element", async () =>
        {
            var page = new Page();
            for (var i = 0; i < 9; i++) DeckEditor.AddButton(page);
            await Reject(() => { DeckEditor.AddButton(page); return Task.CompletedTask; });
            Check(page.Elements.Count == 9);
        });
        await test("Versioned JSON roundtrip restores identities, bindings, geometry and URL", async () =>
        {
            await WithStore(async (store, path) =>
            {
                var deck = DeckJsonStore.CreateDefault();
                var page = DeckEditor.AddPage(deck);
                DeckEditor.RenamePage(page, "Saved Page");
                var element = DeckEditor.AddButton(page);
                DeckEditor.UpdateElement(page, element, "Saved Button", "url.open", "https://example.com/", 1, 1, 2, 2);
                await store.SaveAsync(deck);
                var restored = await new DeckJsonStore(path).LoadAsync();
                var actual = restored.Pages[1].Elements[0];
                Check(restored.Id == deck.Id && restored.Pages[1].Name == "Saved Page" && actual.Id == element.Id);
                Check(actual.Name == "Saved Button" && actual.ActionId == "url.open" && actual.GridWidth == 2 && actual.Url == "https://example.com/");
                using var document = JsonDocument.Parse(await File.ReadAllTextAsync(path));
                Check(document.RootElement.GetProperty("schemaVersion").GetInt32() == 1);
            });
        });
        await test("Concurrent saves use snapshots and preserve latest edit", async () =>
        {
            await WithStore(async (store, path) =>
            {
                var deck = DeckJsonStore.CreateDefault();
                var writes = new List<Task>();
                for (var i = 0; i < 25; i++) { deck.Name = "Deck " + i; writes.Add(store.SaveAsync(deck)); }
                await Task.WhenAll(writes);
                Check((await new DeckJsonStore(path).LoadAsync()).Name == "Deck 24");
            });
        });
        await test("Malformed/unsupported JSON is preserved and writes are blocked", async () =>
        {
            foreach (var json in new[] { "{broken", "null", "{}", "{\"schemaVersion\":99}", "{\"pages\":[]}" })
                await WithStore(async (store, path) =>
                {
                    await File.WriteAllTextAsync(path, json);
                    await Reject(() => store.LoadAsync());
                    await Reject(() => store.SaveAsync(DeckJsonStore.CreateDefault()));
                    Check(await File.ReadAllTextAsync(path) == json);
                });
        });
        await test("Legacy Deck migrates on next save", async () =>
        {
            await WithStore(async (store, path) =>
            {
                var deck = DeckJsonStore.CreateDefault();
                await File.WriteAllTextAsync(path, JsonSerializer.Serialize(deck, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
                var loaded = await store.LoadAsync();
                Check(loaded.Id == deck.Id);
                await store.SaveAsync(loaded);
                Check((await new DeckJsonStore(path).LoadAsync()).Id == deck.Id);
            });
        });
        await test("url.open uses configured URL and rejects shell/file schemes", async () =>
        {
            Uri? opened = null;
            var executor = new UrlOpenActionExecutor(uri => opened = uri);
            Check((await executor.ExecuteAsync(new Element { Url = "https://example.com/path" }, new CoreAction())).Status == "Success");
            Check(opened!.AbsoluteUri == "https://example.com/path");
            foreach (var url in new[] { "file:///C:/Windows/notepad.exe", "javascript:alert(1)", "not a url" })
                Check((await executor.ExecuteAsync(new Element { Url = url }, new CoreAction())).Status == "Error");
        });
        await test("Desktop registry includes Start before first click, shared service and current config", async () =>
        {
            await using var server = new ServerLauncherService(Path.Combine(Path.GetTempPath(), "RadishDeck-no-executable-" + Guid.NewGuid() + ".exe"));
            var reads = 0;
            var config = ServerConfiguration.Create("127.0.0.1", "5187");
            var registry = DesktopActions.Create(server, () => { reads++; return config; });
            Check(registry.Definitions.Count == 3 && registry.TryGet("server.start", out _, out _));
            var dispatcher = new ActionDispatcher(registry);
            var start = await dispatcher.ExecuteAsync(new(), new CoreAction { Type = "server.start" });
            Check(start.Status == "Error" && server.Snapshot.Status == ServerLifecycle.Error && reads == 1);
            Check((await dispatcher.ExecuteAsync(new(), new CoreAction { Type = "server.stop" })).Status == "Stopped");
            Check(server.Snapshot.Status == ServerLifecycle.Stopped);
        });
    }

    private static void Check(bool condition)
    {
        if (!condition) throw new InvalidOperationException("Deck assertion failed");
    }
    private static async Task Reject(Func<Task> operation)
    {
        try { await operation(); }
        catch (Exception) { return; }
        throw new InvalidOperationException("Expected rejection");
    }
    private static async Task WithStore(Func<DeckJsonStore, string, Task> run)
    {
        var directory = Path.Combine(Path.GetTempPath(), "RadishDeck-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "deck.json");
        try { await run(new DeckJsonStore(path), path); }
        finally
        {
            foreach (var file in Directory.GetFiles(directory)) File.Delete(file);
            Directory.Delete(directory);
        }
    }
    private sealed class FakeExecutor : IActionExecutor
    {
        public int Calls { get; private set; }
        public Element? Element { get; private set; }
        public CoreAction? Action { get; private set; }
        public bool Fail { get; init; }
        public Task<State> ExecuteAsync(Element element, CoreAction action, CancellationToken cancellationToken = default)
        {
            Calls++; Element = element; Action = action;
            if (Fail) throw new InvalidOperationException("Test failure");
            return Task.FromResult(new State { Status = "Success", Message = "Executed" });
        }
    }
}
