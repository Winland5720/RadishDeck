namespace RadishDeck.Desktop.Services;

public sealed class ServerSettings
{
    public string IpAddress { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 5187;
    public string Name { get; set; } = "RadishDeck";

    public static ServerSettings Defaults => new();
}
