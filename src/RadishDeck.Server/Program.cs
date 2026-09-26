using RadishDeck.Core;
using RadishDeck.Core.Actions;
using RadishDeck.Core.Models;
using RadishDeck.Infrastructure;
using RadishDeck.Infrastructure.Actions;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

var deckStore = new DeckJsonStore();

var registry = ActionCatalog.Create();

var dispatcher = new ActionDispatcher(registry);

app.MapGet("/status", () => new ServerStatus(
    ServerStatus.ServerName, AppVersion.Current, ServerStatus.RunningStatus, Environment.ProcessId));

app.MapGet("/deck", async (CancellationToken cancellationToken) =>
{
    var deck = await deckStore.LoadAsync(cancellationToken);
    return Results.Ok(new { version = "deck.v1", name = deck.Name, canvas = new { width = 1920, height = 1080 }, pages = deck.Pages });
});

app.MapPost("/execute", async (ExecuteRequest request, CancellationToken cancellationToken) =>
{
    var deck = await deckStore.LoadAsync(cancellationToken);
    var element = deck.Pages.SelectMany(x => x.Elements)
        .FirstOrDefault(x => x.Id == request.ElementId);

    if (element is null)
        return Results.NotFound();

    var action = new RadishDeck.Core.Models.Action
    {
        Type = element.ActionId,
        Name = element.Name
    };

    var result = await dispatcher.ExecuteAsync(element, action, cancellationToken);
    return Results.Ok(result);
});

app.Run();

public record ExecuteRequest(Guid ElementId);
