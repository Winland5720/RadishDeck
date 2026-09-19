using System.Net.Http;
using System.Net.Http.Json;

namespace RadishDeck.Desktop.Services;

public sealed class ServerStatusService : IDisposable
{
    private readonly HttpClient _httpClient;

    public ServerStatusService(Uri serverAddress)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = serverAddress,
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    public Task<ServerStatus?> GetStatusAsync(CancellationToken cancellationToken = default) =>
        _httpClient.GetFromJsonAsync<ServerStatus>("status", cancellationToken);

    public void Dispose() => _httpClient.Dispose();
}
