using System.Text.Json;
using RadishDeck.Core.Actions;
using RadishDeck.Core.Execution;
using RadishDeck.Core.Models;
using RadishDeck.Desktop.Services;
using RadishDeck.Desktop.Rendering;
using RadishDeck.Infrastructure;
using RadishDeck.Infrastructure.Actions.Executors;
using RadishDeck.Core.Models.V2;
using RadishDeck.Core.Rendering;
using CoreAction = RadishDeck.Core.Models.Action;

internal static class DeckChecks
{
    public static async Task RunAsync(Func<string, Func<Task>, Task> test)
    {
        await test("Renderer contract maps Element V2 without UI dependencies", () =>
        {
            var element = new Element
            {
                Id = Guid.NewGuid(), Type = "Button",
                Layout = new ElementLayout { X = 10, Y = 20, Width = 300, Height = 100 },
                Content = new ElementContent { Text = "Restart", Image = "restart.png" },
                Appearance = new ElementAppearance { Background = "#E11D2E" }
            };
            var rendered = RenderElement.From(element);
            Check(rendered.Id == element.Id && rendered.Type == "Button");
            Check(rendered.Layout.X == 10 && rendered.Layout.Y == 20 && rendered.Layout.Width == 300 && rendered.Layout.Height == 100);
            Check(rendered.Content?.Text == "Restart" && rendered.Content.Image == "restart.png");
            Check(rendered.Appearance?.Background == "#E11D2E");
            Check(typeof(IRenderer).GetMethod(nameof(IRenderer.Render)) is not null &&
                  typeof(IRenderContext).GetMethod(nameof(IRenderContext.Draw)) is not null);
            return Task.CompletedTask;
        });
        await test("WPF Renderer receives RenderElement through the contract", () =>
        {
            var rendered = RenderElement.From(new Element { Type = "Button", Layout = new() { Width = 1, Height = 1 } });
            var context = new RecordingRenderContext();
            new WpfCanvasRenderer().Render(new[] { rendered }, context);
            Check(context.Drawn.Count == 1 && context.Drawn[0] == rendered);
            return Task.CompletedTask;
        });
        await test("Renderer contract rejects legacy Element without V2 layout", () =>
        {
            try { RenderElement.From(new Element { Type = "Button" }); }
            catch (InvalidOperationException) { return Task.CompletedTask; }
            throw new InvalidOperationException("Expected V2 layout requirement");
        });

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        await test("Legacy Element has no V2 sections", () =>
        {
            var element = new Element { Name = "Legacy", Type = "Button", ActionId = "url.open" };
            Check(element.Layout is null && element.Content is null && element.Appearance is null &&
                  element.Behavior is null && element.Action is null);
            using var json = JsonDocument.Parse(JsonSerializer.Serialize(element, options));
            foreach (var section in new[] { "layout", "content", "appearance", "behavior", "action" })
                Check(!json.RootElement.TryGetProperty(section, out _));
            Check(element.GridWidth == 1 && element.GridHeight == 1);
            return Task.CompletedTask;
        });
        await test("Element Canvas layout leaves legacy geometry intact", () =>
        {
            var element = new Element { GridX = 2, Layout = new() { X = 100.5, Y = 200, Width = 300, Height = 100 } };
            Check(element.Layout.X == 100.5 && element.GridX == 2 && element.GridWidth == 1);
            return Task.CompletedTask;
        });
        await test("Element content and appearance are independent of legacy fields", () =>
        {
            var element = new Element { Name = "Internal", Color = "#123456",
                Content = new() { Text = "Visible", Image = "image.png" },
                Appearance = new() { Background = "#E11D2E", Radius = 20, Opacity = 0.5 } };
            Check(element.Content.Text == "Visible" && element.Appearance.Radius == 20 &&
                  element.Name == "Internal" && element.Color == "#123456" && element.Action is null);
            return Task.CompletedTask;
        });
        await test("Element V2 roundtrip preserves all sections and legacy binding", () =>
        {
            var element = new Element { Name = "Old", Type = "Button", GridX = 1, ActionId = "url.open",
                Url = "https://example.com/", Color = "#123456",
                Layout = new() { X = 12.5, Y = 30, Width = 200, Height = 80, Layer = 2, Alignment = "center" },
                Content = new() { Text = "New", Image = "image.png", Icon = "icon", Logo = "logo.png", Media = "media" },
                Appearance = new() { Background = "#E11D2E", Color = "#FFFFFF", Border = "border", Radius = 20,
                    Shadow = "shadow", Font = "font", Opacity = 0.75 },
                Behavior = new() { Hover = "hover", Click = "click", Animation = "fade", State = "Normal" },
                Action = new() { ActionId = "future", Type = "process.start", Parameters = new() { ["path"] = "app.exe" } } };
            var json = JsonSerializer.Serialize(element, options);
            var restored = JsonSerializer.Deserialize<Element>(json, options)!;
            Check(restored.Id == element.Id && restored.ActionId == "url.open" && restored.GridX == 1 &&
                  restored.Url == element.Url && restored.Color == element.Color);
            Check(restored.Layout?.X == 12.5 && restored.Content?.Logo == "logo.png" &&
                  restored.Appearance?.Opacity == 0.75 && restored.Behavior?.Animation == "fade" &&
                  restored.Action?.Parameters["path"] == "app.exe");
            Check(JsonSerializer.Serialize(restored, options) == json);
            return Task.CompletedTask;
        });
        await test("Fixed legacy Element JSON retains its fields on roundtrip", () =>
        {
            const string json = """
                {"id":"ed3ec153-aaea-41bf-8103-f71d30555fa9","name":"Legacy","type":"Button",
                 "gridX":1,"gridY":2,"gridWidth":2,"gridHeight":1,"actionId":"url.open",
                 "url":"https://example.com/","color":"#112233"}
                """;
            var element = JsonSerializer.Deserialize<Element>(json, options)!;
            Check(element.Layout is null && element.Action is null && element.Content is null &&
                  element.Appearance is null && element.Behavior is null);
            using var expected = JsonDocument.Parse(json);
            using var actual = JsonDocument.Parse(JsonSerializer.Serialize(element, options));
            Check(actual.RootElement.EnumerateObject().Count() == expected.RootElement.EnumerateObject().Count());
            foreach (var property in expected.RootElement.EnumerateObject())
                Check(actual.RootElement.GetProperty(property.Name).ToString() == property.Value.ToString());
            var explicitNull = JsonSerializer.Deserialize<Element>("{\"layout\":null,\"content\":null,\"appearance\":null,\"behavior\":null,\"action\":null}", options)!;
            Check(explicitNull.Layout is null && explicitNull.Action is null);
            return Task.CompletedTask;
        });

        await test("Element v2 models create and roundtrip through JSON", () =>
        {
            var canvas = new CanvasProfile { Name = "Desktop", Width = 1920, Height = 1080,
                Resolution = "1920x1080", Orientation = CanvasOrientation.Landscape,
                Background = "#111111", Assets = new() { "logo.png" } };
            var device = new DeviceProfile { Name = "Mobile", Width = 390, Height = 844, Type = DeviceProfileType.Mobile };
            var layout = new ElementLayout { X = 100, Y = 200, Width = 300, Height = 100, Layer = 2, Alignment = "center" };
            Check(canvas.Orientation == CanvasOrientation.Landscape && device.Type == DeviceProfileType.Mobile && layout.X == 100);
            var json = JsonSerializer.Serialize(new { canvas, device, layout,
                content = new ElementContent { Text = "Restart" },
                appearance = new ElementAppearance { Background = "#E11D2E", Radius = 20 },
                behavior = new ElementBehavior { State = "Normal" },
                action = new ElementAction { Type = "process.start" } },
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
            using var document = JsonDocument.Parse(json);
            Check(document.RootElement.GetProperty("canvas").GetProperty("width").GetInt32() == 1920);
            Check(document.RootElement.GetProperty("action").GetProperty("type").GetString() == "process.start");
            var restored = JsonSerializer.Deserialize<JsonElement>(json);
            var restoredCanvas = JsonSerializer.Deserialize<CanvasProfile>(restored.GetProperty("canvas").GetRawText(), new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var restoredDevice = JsonSerializer.Deserialize<DeviceProfile>(restored.GetProperty("device").GetRawText(), new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var restoredLayout = JsonSerializer.Deserialize<ElementLayout>(restored.GetProperty("layout").GetRawText(), new JsonSerializerOptions(JsonSerializerDefaults.Web));
            Check(restoredCanvas?.Width == 1920 && restoredDevice?.Type == DeviceProfileType.Mobile && restoredLayout?.Height == 100);
            return Task.CompletedTask;
        });

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
            Check(registry.Definitions.Count == 4 && registry.TryGet("server.start", out _, out _));
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

    private sealed class RecordingRenderContext : IRenderContext
    {
        public List<RenderElement> Drawn { get; } = new();
        public void Clear() => Drawn.Clear();
        public void Draw(RenderElement element) => Drawn.Add(element);
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
