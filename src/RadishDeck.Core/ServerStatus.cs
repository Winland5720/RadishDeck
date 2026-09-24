namespace RadishDeck.Core;

public sealed record ServerStatus(string Name, string Version, string Status, int ProcessId)
{
    public const string ServerName = "Radish Deck Server";
    public const string RunningStatus = "running";

    public bool IsExpected(int processId) =>
        Name == ServerName && Version == AppVersion.Current &&
        Status == RunningStatus && ProcessId == processId;
}
