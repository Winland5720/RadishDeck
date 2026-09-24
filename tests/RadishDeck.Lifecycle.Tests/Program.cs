using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using RadishDeck.Core;
using RadishDeck.Core.Models;
using RadishDeck.Desktop.Services;

if (args.Length != 3)
{
    Console.Error.WriteLine("Usage: <published Server.exe> <TestServer.exe> <local interface IP>");
    return 2;
}
var serverPath = Path.GetFullPath(args[0]);
var fixturePath = Path.GetFullPath(args[1]);
var ip = IPAddress.Parse(args[2]);
var passed = 0;
var failed = 0;
using var client = new HttpClient(new HttpClientHandler { UseProxy = false });
var previousMode = Environment.GetEnvironmentVariable("RADISHDECK_TEST_MODE");
try
{
    await Test("configuration validation and IPv6 URI", () =>
    {
        foreach (var invalid in new[] { "", "not-an-ip", "0.0.0.0", "::", "255.255.255.255", "224.0.0.1" })
            Throws(() => ServerConfiguration.Create(invalid, "5187"));
        foreach (var invalid in new[] { "", "text", "0", "65536", "-1" })
            Throws(() => ServerConfiguration.Create(ip.ToString(), invalid));
        Assert(ServerConfiguration.Create("::1", "5187").Address.AbsoluteUri == "http://[::1]:5187/", "IPv6 must be bracketed");
        return Task.CompletedTask;
    });

    await Test("published server: pipeline, status, duplicate Start, Stop, restart", async () =>
    {
        await using var launcher = new ServerLauncherService(serverPath);
        var configuration = Configuration();
        var executor = new ServerStartActionExecutor(launcher, configuration);
        var rejected = await executor.ExecuteAsync(new Element(), new RadishDeck.Core.Models.Action { Type = "unsupported" });
        Assert(rejected.Status == "Error" && launcher.Snapshot.Status == ServerLifecycle.Stopped, "unsupported action changed lifecycle");
        var start = executor.ExecuteAsync(new Element(), new RadishDeck.Core.Models.Action { Type = "server.start" });
        Assert(launcher.Snapshot.Status == ServerLifecycle.Starting && !launcher.Snapshot.CanEditConfiguration, "Starting must lock config");
        var duplicate = await launcher.StartAsync(Configuration());
        Assert(duplicate.Status == "Starting" && launcher.Snapshot.Configuration == configuration, "duplicate Start replaced snapshot");
        Assert((await start).Status == "Running", launcher.Diagnostics);
        Assert(!launcher.Snapshot.CanEditConfiguration && launcher.Snapshot.Version == AppVersion.Current, "Running snapshot invalid");
        var status = await client.GetFromJsonAsync<ServerStatus>(new Uri(configuration.Address, "status"));
        Assert(status is not null && status.IsExpected(status.ProcessId), "invalid actual /status");
        Assert(Process.GetProcessById(status!.ProcessId).MainModule!.FileName == serverPath, "server came from outside publish");
        var stop = launcher.StopAsync();
        Assert(launcher.Snapshot.Status == ServerLifecycle.Stopping && !launcher.Snapshot.CanEditConfiguration, "Stopping must lock config");
        await stop;
        Assert(launcher.Snapshot.Status == ServerLifecycle.Stopped && launcher.Snapshot.CanEditConfiguration, "Stop did not unlock config");
        Assert(IsExited(status.ProcessId), "child survived Stop");
        Assert((await launcher.StartAsync(configuration)).Status == "Running", launcher.Diagnostics);
    });

    await Test("immediate Start/Stop x30 and overlapping Stop", async () =>
    {
        await using var launcher = new ServerLauncherService(serverPath);
        for (var i = 0; i < 30; i++)
        {
            var start = launcher.StartAsync(Configuration());
            var stop = launcher.StopAsync();
            var secondStop = launcher.StopAsync();
            await Task.WhenAll(start, stop, secondStop);
            Assert(launcher.Snapshot.Status == ServerLifecycle.Stopped && !launcher.Snapshot.OwnsProcess, "stale Start overwrote Stop");
        }
    });

    await Test("missing executable", async () =>
    {
        await using var launcher = new ServerLauncherService(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".exe"));
        var result = await launcher.StartAsync(Configuration());
        Assert(result.Status == "Error" && result.Message == "Server executable not found", "missing exe diagnostic");
        Assert(launcher.Snapshot.CanEditConfiguration, "error must unlock config after cleanup");
    });

    await Test("occupied port", async () =>
    {
        var listener = new TcpListener(ip, 0);
        listener.Server.ExclusiveAddressUse = true;
        listener.Start();
        try
        {
            await using var launcher = new ServerLauncherService(serverPath);
            var result = await launcher.StartAsync(ServerConfiguration.Create(ip.ToString(), ((IPEndPoint)listener.LocalEndpoint).Port.ToString()));
            Assert(result.Status == "Error" && result.Message.Contains("already in use"), result.Message);
            Assert(launcher.Snapshot.CanEditConfiguration, "occupied port leaked process");
        }
        finally { listener.Stop(); }
    });

    await Test("unexpected exit and recovery", async () =>
    {
        await using var launcher = new ServerLauncherService(serverPath);
        var configuration = Configuration();
        Assert((await launcher.StartAsync(configuration)).Status == "Running", launcher.Diagnostics);
        var status = (await client.GetFromJsonAsync<ServerStatus>(new Uri(configuration.Address, "status")))!;
        using (var process = Process.GetProcessById(status.ProcessId)) process.Kill();
        await Until(() => launcher.Snapshot.Status == ServerLifecycle.Error);
        Assert(launcher.Snapshot.CanEditConfiguration && launcher.Snapshot.Message.Contains("unexpectedly"), "exit not reflected");
        Assert((await launcher.StartAsync(configuration)).Status == "Running", launcher.Diagnostics);
    });

    await Test("Stop during delayed startup", async () =>
    {
        Environment.SetEnvironmentVariable("RADISHDECK_TEST_MODE", "delay");
        await using var launcher = new ServerLauncherService(fixturePath);
        var start = launcher.StartAsync(Configuration());
        await Until(() => launcher.Snapshot.OwnsProcess);
        var stop = launcher.StopAsync();
        await Task.WhenAll(start, stop);
        Assert(launcher.Snapshot.Status == ServerLifecycle.Stopped && launcher.Snapshot.CanEditConfiguration, "delayed startup not stopped");
    });

    await Test("Dispose during startup", async () =>
    {
        Environment.SetEnvironmentVariable("RADISHDECK_TEST_MODE", "delay");
        var launcher = new ServerLauncherService(fixturePath);
        var start = launcher.StartAsync(Configuration());
        await Until(() => launcher.Snapshot.OwnsProcess);
        await launcher.DisposeAsync();
        await start;
        Assert(launcher.Snapshot.Status == ServerLifecycle.Stopped && !launcher.Snapshot.OwnsProcess, "dispose leaked child");
    });

    await Test("external cancellation", async () =>
    {
        Environment.SetEnvironmentVariable("RADISHDECK_TEST_MODE", "delay");
        await using var launcher = new ServerLauncherService(fixturePath);
        using var cancellation = new CancellationTokenSource();
        var start = launcher.StartAsync(Configuration(), cancellation.Token);
        await Until(() => launcher.Snapshot.OwnsProcess);
        cancellation.Cancel();
        Assert((await start).Status == "Error" && launcher.Snapshot.CanEditConfiguration, "cancelled start leaked child");
    });

    foreach (var mode in new[] { "delay", "exit", "wrong-name", "wrong-version", "wrong-status", "wrong-pid", "malformed", "missing", "http-error", "oversized" })
    {
        await Test("reject/clean up fixture: " + mode, async () =>
        {
            Environment.SetEnvironmentVariable("RADISHDECK_TEST_MODE", mode);
            await using var launcher = new ServerLauncherService(fixturePath, TimeSpan.FromSeconds(2));
            var state = await launcher.StartAsync(Configuration());
            Assert(state.Status == "Error" && launcher.Snapshot.CanEditConfiguration && !launcher.Snapshot.OwnsProcess, "invalid server was accepted or leaked");
            Assert(launcher.Diagnostics.Length > 0, "diagnostics missing");
        });
    }

    await Test("stdout/stderr flood without newlines is drained and bounded", async () =>
    {
        Environment.SetEnvironmentVariable("RADISHDECK_TEST_MODE", "flood");
        await using var launcher = new ServerLauncherService(fixturePath);
        Assert((await launcher.StartAsync(Configuration())).Status == "Running", launcher.Diagnostics);
        Assert(launcher.Diagnostics.Length <= 16 * 1024, "unbounded diagnostic tail");
        Assert(launcher.Diagnostics.Contains("stderr drained"), "stderr was not drained");
    });
}
finally { Environment.SetEnvironmentVariable("RADISHDECK_TEST_MODE", previousMode); }
await DeckChecks.RunAsync(Test);
Console.WriteLine($"RESULT: {passed} passed, {failed} failed");
return failed == 0 ? 0 : 1;

ServerConfiguration Configuration()
{
    var listener = new TcpListener(ip, 0);
    listener.Start();
    var port = ((IPEndPoint)listener.LocalEndpoint).Port;
    listener.Stop();
    return ServerConfiguration.Create(ip.ToString(), port.ToString());
}
async Task Test(string name, Func<Task> run)
{
    try { await run(); passed++; Console.WriteLine("PASS " + name); }
    catch (Exception exception) { failed++; Console.WriteLine("FAIL " + name + ": " + exception); }
}
static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
static void Throws(System.Action action)
{
    try { action(); }
    catch (ArgumentException) { return; }
    throw new InvalidOperationException("Expected validation failure");
}
static bool IsExited(int pid)
{
    try { using var process = Process.GetProcessById(pid); return process.HasExited; }
    catch (ArgumentException) { return true; }
}
static async Task Until(Func<bool> condition)
{
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    while (!condition()) await Task.Delay(20, timeout.Token);
}

