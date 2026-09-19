using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace RadishDeck.Desktop.Services;

public sealed class ServerLauncherService : IDisposable
{
    private readonly string _serverProjectPath;
    private Process? _serverProcess;

    public ServerLauncherService(string? serverProjectPath = null)
    {
        _serverProjectPath = serverProjectPath ?? Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "RadishDeck.Server"));
    }

    public bool IsRunning => _serverProcess is { HasExited: false };

    public async Task<bool> StartAsync(string ipAddress, int port, CancellationToken cancellationToken = default)
    {
        if (IsRunning)
        {
            return await IsAvailableAsync(ipAddress, port, cancellationToken);
        }

        var serverUrl = $"http://{ipAddress}:{port}";
        _serverProcess = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{_serverProjectPath}\" -- --urls \"{serverUrl}\"",
            WorkingDirectory = _serverProjectPath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        });

        if (_serverProcess is null)
        {
            return false;
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        while (!timeout.IsCancellationRequested && !_serverProcess.HasExited)
        {
            if (await IsAvailableAsync(ipAddress, port, timeout.Token))
            {
                return true;
            }

            await Task.Delay(250, timeout.Token);
        }

        return false;
    }

    public async Task<bool> IsAvailableAsync(string ipAddress, int port, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(1) };
            using var response = await client.GetAsync($"http://{ipAddress}:{port}/status", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    public void Stop()
    {
        if (_serverProcess is { HasExited: false })
        {
            _serverProcess.Kill(entireProcessTree: true);
            _serverProcess.WaitForExit(5000);
        }

        _serverProcess?.Dispose();
        _serverProcess = null;
    }

    public void Dispose() => Stop();
}
