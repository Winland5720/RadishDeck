using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using RadishDeck.Core;

namespace RadishDeck.Desktop.Services;

public sealed class ServerStatusService : IDisposable
{
    private readonly HttpClient _httpClient;

    public ServerStatusService(Uri serverAddress)
    {
        _httpClient = new HttpClient(new HttpClientHandler { UseProxy = false })
        {
            BaseAddress = serverAddress,
            Timeout = TimeSpan.FromSeconds(1),
            MaxResponseContentBufferSize = 16 * 1024
        };
    }

    public async Task<ServerStatus?> GetStatusAsync(int processId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync("status", cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return null;
            var status = await response.Content.ReadFromJsonAsync<ServerStatus>(cancellationToken).ConfigureAwait(false);
            return status?.IsExpected(processId) == true ? status : null;
        }
        catch (HttpRequestException) { return null; }
        catch (JsonException) { return null; }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested) { return null; }
    }

    public void Dispose() => _httpClient.Dispose();
}
