var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/status", () => new
{
    name = "Radish Deck Server",
    version = "0.1.0",
    status = "running"
});

app.Run();
