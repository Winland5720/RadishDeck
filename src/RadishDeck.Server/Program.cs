using RadishDeck.Core;
using RadishDeck.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var deckStore = new DeckJsonStore();

app.MapGet("/status", () => new ServerStatus(
    ServerStatus.ServerName, AppVersion.Current, ServerStatus.RunningStatus, Environment.ProcessId));

app.MapGet("/deck", async (CancellationToken cancellationToken) =>
{
    var deck = await deckStore.LoadAsync(cancellationToken);
    return Results.Ok(new { version = "deck.v1", name = deck.Name, pages = deck.Pages });
});

app.Run();
