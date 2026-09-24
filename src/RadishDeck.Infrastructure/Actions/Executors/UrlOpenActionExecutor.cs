using System.Diagnostics;
using RadishDeck.Core.Execution;
using RadishDeck.Core.Models;
using CoreAction = RadishDeck.Core.Models.Action;

namespace RadishDeck.Infrastructure.Actions.Executors;

public sealed class UrlOpenActionExecutor : IActionExecutor
{
    private readonly Action<Uri> _open;

    public UrlOpenActionExecutor(Action<Uri>? open = null) =>
        _open = open ?? (uri => Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true }));

    public Task<State> ExecuteAsync(Element element, CoreAction action, CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!Uri.TryCreate(element.Url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                return Task.FromResult(new State { Status = "Error", Message = "Only absolute HTTP/HTTPS URLs are allowed" });
            _open(uri);
            return Task.FromResult(new State { Status = "Success", Message = "URL opened" });
        }
        catch (Exception exception) { return Task.FromResult(new State { Status = "Error", Message = exception.Message }); }
    }
}
