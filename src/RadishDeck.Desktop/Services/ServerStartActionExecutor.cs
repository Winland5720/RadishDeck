using RadishDeck.Core.Execution;
using RadishDeck.Core.Models;
using CoreAction = RadishDeck.Core.Models.Action;

namespace RadishDeck.Desktop.Services;

public sealed class ServerStartActionExecutor : IActionExecutor
{
    private readonly ServerLauncherService _serverLauncherService;
    private readonly Func<ServerConfiguration> _configuration;

    public ServerStartActionExecutor(ServerLauncherService serverLauncherService, ServerConfiguration configuration)
    {
        _serverLauncherService = serverLauncherService;
        _configuration = () => configuration;
    }

    public ServerStartActionExecutor(ServerLauncherService serverLauncherService, Func<ServerConfiguration> configuration)
    {
        _serverLauncherService = serverLauncherService;
        _configuration = configuration;
    }

    public Task<State> ExecuteAsync(Element element, CoreAction action, CancellationToken cancellationToken = default) =>
        action.Type == "server.start"
            ? _serverLauncherService.StartAsync(_configuration(), cancellationToken)
            : Task.FromResult(new State { Status = "Error", Message = $"Unsupported action: {action.Type}" });
}
