using System.Net;
using System.Net.Sockets;

namespace RadishDeck.Desktop.Services;

public sealed record ServerConfiguration
{
    public string IpAddress { get; }
    public int Port { get; }
    public Uri Address => new UriBuilder("http", IpAddress, Port).Uri;

    private ServerConfiguration(string ipAddress, int port) => (IpAddress, Port) = (ipAddress, port);

    public static ServerConfiguration Create(string ipAddress, string port)
    {
        if (!IPAddress.TryParse(ipAddress.Trim(), out var address) ||
            address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any) ||
            address.Equals(IPAddress.Broadcast) || address.IsIPv6Multicast ||
            (address.AddressFamily == AddressFamily.InterNetwork && address.GetAddressBytes()[0] >= 224))
        {
            throw new ArgumentException("Invalid IP address");
        }

        if (!int.TryParse(port, out var number) || number is < 1 or > 65535)
        {
            throw new ArgumentException("Invalid port");
        }

        return new ServerConfiguration(address.ToString(), number);
    }
}
