using RadishDeck.Core;

// Fault-injection executable for integration tests only; never bundled with Desktop.
var mode = Environment.GetEnvironmentVariable("RADISHDECK_TEST_MODE");
if (mode == "exit") return 42;
if (mode == "delay") await Task.Delay(TimeSpan.FromMinutes(1));
if (mode == "flood")
{
    var chunk = new string('x', 4096);
    for (var i = 0; i < 256; i++)
    {
        await Console.Out.WriteAsync(chunk);
        await Console.Error.WriteAsync(chunk);
    }
    await Console.Out.WriteLineAsync("stdout drained");
    await Console.Error.WriteLineAsync("stderr drained");
}
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/status", () =>
{
    if (mode == "malformed") return Results.Text("not json", "application/json");
    if (mode == "missing") return Results.Json(new { status = "running" });
    if (mode == "http-error") return Results.StatusCode(503);
    if (mode == "oversized") return Results.Text(new string('x', 32768));
    return Results.Json(new ServerStatus(
        mode == "wrong-name" ? "Other Server" : ServerStatus.ServerName,
        mode == "wrong-version" ? "incorrect-version" : AppVersion.Current,
        mode == "wrong-status" ? "stopped" : ServerStatus.RunningStatus,
        mode == "wrong-pid" ? -1 : Environment.ProcessId));
});
await app.RunAsync();
return 0;
