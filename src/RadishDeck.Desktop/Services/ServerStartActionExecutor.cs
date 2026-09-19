using RadishDeck.Core.Execution;
using RadishDeck.Core.Models;
using CoreAction = RadishDeck.Core.Models.Action;

namespace RadishDeck.Desktop.Services;

public sealed class ServerStartActionExecutor : IActionExecutor
{
    private readonly ServerLauncherService _serverLauncherService;
    private readonly Func<(string IpAddress, int Port)> _configuration;

    public ServerStartActionExecutor(ServerLauncherService serverLauncherService, Func<(string IpAddress, int Port)> configuration)
    {
        _serverLauncherService = serverLauncherService;
        _configuration = configuration;
    }

    public async Task<State> ExecuteAsync(Element element, CoreAction action, CancellationToken cancellationToken = default)
    {
        if (action.Type != "server.start")
        {
            return new State { Status = "Error", Message = $"Unsupported action: {action.Type}" };
        }

        try
        {
            var (ipAddress, port) = _configuration();
            var started = await _serverLauncherService.StartAsync(ipAddress, port, cancellationToken);
            return started
                ? new State { Status = "Running", Message = $"{element.Name} completed" }
                : new State { Status = "Error", Message = "Server did not become available" };
        }
        catch (Exception exception)
        {
            return new State { Status = "Error", Message = exception.Message };
        }
    }
}
