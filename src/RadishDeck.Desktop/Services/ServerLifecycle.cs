namespace RadishDeck.Desktop.Services;

public enum ServerLifecycle { Stopped, Starting, Running, Stopping, Error }

public sealed record ServerLifecycleSnapshot(
    ServerLifecycle Status, string Message, ServerConfiguration? Configuration, string? Version,
    bool OwnsProcess)
{
    public bool CanEditConfiguration => !OwnsProcess && Status is ServerLifecycle.Stopped or ServerLifecycle.Error;
    public bool CanStop => Status is ServerLifecycle.Starting or ServerLifecycle.Running ||
                           (Status == ServerLifecycle.Error && OwnsProcess);
}
