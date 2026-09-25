using System.Text.Json;
using System.IO;

namespace RadishDeck.Desktop.Services;

public sealed class ServerSettingsStore
{
    private readonly string _path;
    private readonly SemaphoreSlim _writes = new(1, 1);
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public string FilePath => _path;

    public ServerSettingsStore(string? path = null) => _path = Path.GetFullPath(path ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RadishDeck", "server.settings.json"));

    public async Task<ServerSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await File.ReadAllTextAsync(_path, cancellationToken);
            return JsonSerializer.Deserialize<ServerSettings>(json, Options)
                ?? throw new InvalidDataException("Server settings are empty");
        }
        catch (FileNotFoundException) { return ServerSettings.Defaults; }
        catch (DirectoryNotFoundException) { return ServerSettings.Defaults; }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new InvalidDataException($"Server settings could not be loaded. Original file preserved: {_path}. {exception.Message}", exception);
        }
    }

    public Task SaveAsync(ServerSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(settings, Options);
        return WriteAsync(bytes, cancellationToken);
    }

    private async Task WriteAsync(byte[] bytes, CancellationToken cancellationToken)
    {
        await _writes.WaitAsync(cancellationToken);
        var temporary = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            await File.WriteAllBytesAsync(temporary, bytes, cancellationToken);
            File.Move(temporary, _path, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
            _writes.Release();
        }
    }
}
