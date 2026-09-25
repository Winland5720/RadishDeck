using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using RadishDeck.Core.Models;

namespace RadishDeck.Desktop.Services;

public sealed class ServerLauncherService : IAsyncDisposable
{
    private readonly string _executablePath;
    private readonly TimeSpan _startupTimeout;
    private readonly object _sync = new();
    private readonly SemaphoreSlim _operation = new(1, 1);
    private Process? _process;
    private CancellationTokenSource? _startupCancellation;
    private ServerDiagnostics _diagnostics = new();
    private Task _drain = Task.CompletedTask;
    private Task<State>? _stopTask;
    private bool _disposed;
    private ServerLifecycleSnapshot _snapshot = new(ServerLifecycle.Stopped, "", null, null, false);
    private ServerConfiguration? _lastConfiguration;

    public ServerLauncherService(string? executablePath = null, TimeSpan? startupTimeout = null)
    {
        _executablePath = executablePath ?? Path.Combine(AppContext.BaseDirectory, "server", "RadishDeck.Server.exe");
        _startupTimeout = startupTimeout ?? TimeSpan.FromSeconds(15);
    }

    public event System.Action? StateChanged;
    public ServerLifecycleSnapshot Snapshot { get { lock (_sync) return _snapshot; } }
    public string Diagnostics { get { lock (_sync) return _diagnostics.Tail; } }

    public Task<State> StartAsync(ServerConfiguration configuration, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            // Reject overlapping starts instead of queuing a surprise restart after Stop.
            if (!_snapshot.CanEditConfiguration) return Task.FromResult(CurrentState());
            _diagnostics = new ServerDiagnostics();
            _lastConfiguration = configuration;
            _startupCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _startupCancellation.CancelAfter(_startupTimeout);
            _snapshot = new(ServerLifecycle.Starting, "Starting server", configuration, null, false);
            StateChanged?.Invoke();
            return StartCoreAsync(configuration, _startupCancellation, _operation.WaitAsync());
        }
    }

    public Task<State> StopAsync()
    {
        lock (_sync)
        {
            if (_snapshot.Status == ServerLifecycle.Stopping) return _stopTask!;
            _startupCancellation?.Cancel();
            SetState(ServerLifecycle.Stopping, "Stopping server");
            _stopTask = StopCoreAsync(_operation.WaitAsync());
            return _stopTask;
        }
    }

    public async Task<State> RestartAsync(CancellationToken cancellationToken = default)
    {
        var configuration = Snapshot.Configuration ?? _lastConfiguration;
        if (configuration is null)
            return new State { Status = "Error", Message = "Server configuration is missing" };

        var stopped = await StopAsync().ConfigureAwait(false);
        if (stopped.Status != ServerLifecycle.Stopped.ToString()) return stopped;
        return await StartAsync(configuration, cancellationToken).ConfigureAwait(false);
    }

    private async Task<State> StartCoreAsync(ServerConfiguration configuration, CancellationTokenSource cancellation, Task admission)
    {
        await Task.Yield();
        await admission.ConfigureAwait(false);
        try
        {
            cancellation.Token.ThrowIfCancellationRequested();
            if (!File.Exists(_executablePath)) throw new FileNotFoundException("Server executable not found");
            CheckPort(configuration);
            var info = new ProcessStartInfo
            {
                FileName = _executablePath,
                WorkingDirectory = Path.GetDirectoryName(_executablePath)!,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            info.ArgumentList.Add("--urls");
            info.ArgumentList.Add(configuration.Address.AbsoluteUri.TrimEnd('/'));
            var process = Process.Start(info) ?? throw new InvalidOperationException("Server could not be started");
            lock (_sync)
            {
                _process = process;
                _snapshot = _snapshot with { OwnsProcess = true };
            }
            _drain = Task.WhenAll(_diagnostics.DrainAsync(process.StandardOutput), _diagnostics.DrainAsync(process.StandardError));
            _ = ObserveExitAsync(process);

            using var statusService = new ServerStatusService(configuration.Address);
            while (!process.HasExited)
            {
                cancellation.Token.ThrowIfCancellationRequested();
                var status = await statusService.GetStatusAsync(process.Id, cancellation.Token).ConfigureAwait(false);
                if (status is not null && !process.HasExited)
                {
                    lock (_sync)
                    {
                        cancellation.Token.ThrowIfCancellationRequested();
                        if (_snapshot.Status == ServerLifecycle.Starting)
                        {
                            _snapshot = _snapshot with { Version = status.Version };
                            SetState(ServerLifecycle.Running, "Server is ready");
                        }
                        return CurrentState();
                    }
                }
                await Task.Delay(100, cancellation.Token).ConfigureAwait(false);
            }
            throw new InvalidOperationException($"Server exited before becoming ready (code {process.ExitCode})");
        }
        catch (Exception exception)
        {
            var message = exception is OperationCanceledException ? "Server did not become ready or startup was cancelled" : exception.Message;
            _diagnostics.Append(Environment.NewLine + message);
            var cleanupError = await CleanupAsync().ConfigureAwait(false);
            lock (_sync)
            {
                // Stop owns the final state once it has been requested.
                if (_snapshot.Status != ServerLifecycle.Stopping)
                    SetState(ServerLifecycle.Error, cleanupError ?? message);
                return CurrentState();
            }
        }
        finally
        {
            lock (_sync)
            {
                if (ReferenceEquals(_startupCancellation, cancellation)) _startupCancellation = null;
                cancellation.Dispose();
            }
            _operation.Release();
        }
    }

    private async Task<State> StopCoreAsync(Task admission)
    {
        await Task.Yield();
        await admission.ConfigureAwait(false);
        try
        {
            var error = await CleanupAsync().ConfigureAwait(false);
            lock (_sync)
            {
                SetState(error is null ? ServerLifecycle.Stopped : ServerLifecycle.Error, error ?? "Server stopped");
                return CurrentState();
            }
        }
        finally { _operation.Release(); }
    }

    private async Task ObserveExitAsync(Process process)
    {
        // Start/Stop may dispose the process before the observer acquires the gate.
        try { await process.WaitForExitAsync().ConfigureAwait(false); }
        catch (InvalidOperationException) { return; }
        await _operation.WaitAsync().ConfigureAwait(false);
        try
        {
            lock (_sync)
            {
                if (!ReferenceEquals(_process, process) || _snapshot.Status != ServerLifecycle.Running) return;
            }
            var message = $"Server exited unexpectedly (code {process.ExitCode})";
            await CleanupAsync().ConfigureAwait(false);
            lock (_sync)
            {
                if (_snapshot.Status == ServerLifecycle.Running) SetState(ServerLifecycle.Error, message);
            }
        }
        finally { _operation.Release(); }
    }

    // The operation gate protects process lifetime. Retain ownership on failure so Stop can retry.
    private async Task<string?> CleanupAsync()
    {
        if (_process is null)
        {
            lock (_sync) _snapshot = _snapshot with { Configuration = null, Version = null, OwnsProcess = false };
            return null;
        }
        try
        {
            if (!_process.HasExited)
            {
                try { _process.Kill(entireProcessTree: true); }
                catch (InvalidOperationException) when (_process.HasExited) { }
            }
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await _process.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
            await _drain.WaitAsync(timeout.Token).ConfigureAwait(false);
            _process.Dispose();
            lock (_sync)
            {
                _process = null;
                _snapshot = _snapshot with { Configuration = null, Version = null, OwnsProcess = false };
            }
            return null;
        }
        catch (Exception exception)
        {
            _diagnostics.Append(Environment.NewLine + exception.Message);
            return "Server could not be stopped; retry Stop";
        }
    }

    private static void CheckPort(ServerConfiguration configuration)
    {
        var listener = new TcpListener(IPAddress.Parse(configuration.IpAddress), configuration.Port);
        listener.Server.ExclusiveAddressUse = true;
        try { listener.Start(); }
        catch (SocketException exception) when (exception.SocketErrorCode == SocketError.AddressAlreadyInUse)
        { throw new InvalidOperationException($"Port {configuration.Port} is already in use", exception); }
        catch (SocketException exception) when (exception.SocketErrorCode == SocketError.AddressNotAvailable)
        { throw new InvalidOperationException("IP address is not available on this computer", exception); }
        finally { listener.Stop(); }
        // Early diagnostic only: /status verifies the child PID as protection against a bind race.
    }

    private State CurrentState() => new() { Status = _snapshot.Status.ToString(), Message = _snapshot.Message };

    private void SetState(ServerLifecycle status, string message)
    {
        _snapshot = _snapshot with { Status = status, Message = message };
        StateChanged?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        lock (_sync) _disposed = true;
        await StopAsync().ConfigureAwait(false);
    }
}
