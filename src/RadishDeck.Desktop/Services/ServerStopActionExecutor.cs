using RadishDeck.Core.Execution;
using RadishDeck.Core.Models;
using CoreAction = RadishDeck.Core.Models.Action;

namespace RadishDeck.Desktop.Services;

public sealed class ServerStopActionExecutor : IActionExecutor
{
    private readonly ServerLauncherService _launcher;
    public ServerStopActionExecutor(ServerLauncherService launcher) => _launcher = launcher;
    public Task<State> ExecuteAsync(Element element, CoreAction action, CancellationToken cancellationToken = default) =>
        _launcher.StopAsync();
}
