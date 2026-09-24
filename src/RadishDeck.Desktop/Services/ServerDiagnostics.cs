using System.IO;
using System.Text;

namespace RadishDeck.Desktop.Services;

internal sealed class ServerDiagnostics
{
    internal const int Capacity = 16 * 1024;
    private readonly StringBuilder _tail = new();
    private readonly object _sync = new();

    public string Tail { get { lock (_sync) return _tail.ToString(); } }

    public void Append(string text)
    {
        lock (_sync)
        {
            _tail.Append(text);
            if (_tail.Length > Capacity) _tail.Remove(0, _tail.Length - Capacity);
        }
    }

    public async Task DrainAsync(StreamReader reader)
    {
        // Read bounded chunks: even a child writing without newlines cannot grow memory indefinitely.
        var buffer = new char[1024];
        try
        {
            int count;
            while ((count = await reader.ReadAsync(buffer).ConfigureAwait(false)) > 0)
                Append(new string(buffer, 0, count));
        }
        catch (IOException exception) { Append(exception.Message); }
        catch (ObjectDisposedException) { /* Process cleanup closed the pipe. */ }
    }
}
