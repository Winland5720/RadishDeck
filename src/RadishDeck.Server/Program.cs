using RadishDeck.Core;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/status", () => new
{
    name = "Radish Deck Server",
    version = AppVersion.Current,
    status = "running"
});

app.Run();
