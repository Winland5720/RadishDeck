using System.Diagnostics;
using System.IO;
using RadishDeck.Core.Execution;
using RadishDeck.Core.Models;
using CoreAction = RadishDeck.Core.Models.Action;

namespace RadishDeck.Infrastructure.Actions.Executors;

public sealed class ProcessStartActionExecutor : IActionExecutor
{
    public Task<State> ExecuteAsync(Element element, CoreAction action, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(action.Type, "process.start", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(Error("Unsupported action: " + action.Type));
        if (string.IsNullOrWhiteSpace(element.Url) || !Path.IsPathFullyQualified(element.Url) ||
            !string.Equals(Path.GetExtension(element.Url), ".exe", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(Error("process.start requires an absolute .exe path"));
        if (!File.Exists(element.Url)) return Task.FromResult(Error("Executable not found: " + element.Url));
        try
        {
            Process.Start(new ProcessStartInfo { FileName = element.Url, UseShellExecute = false });
            return Task.FromResult(new State { Status = "Success", Message = "Application started" });
        }
        catch (Exception exception) { return Task.FromResult(Error(exception.Message)); }
    }

    private static State Error(string message) => new() { Status = "Error", Message = message };
}
