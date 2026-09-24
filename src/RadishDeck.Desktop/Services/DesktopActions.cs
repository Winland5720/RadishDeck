using RadishDeck.Core.Actions;
using RadishDeck.Infrastructure.Actions.Executors;

namespace RadishDeck.Desktop.Services;

public static class DesktopActions
{
    public static ActionRegistry Create(ServerLauncherService server, Func<ServerConfiguration> configuration)
    {
        var registry = new ActionRegistry();
        registry.Register(new("server.start", "Start Server", "Server"), new ServerStartActionExecutor(server, configuration));
        registry.Register(new("server.stop", "Stop Server", "Server"), new ServerStopActionExecutor(server));
        registry.Register(new("url.open", "Open URL", "Local"), new UrlOpenActionExecutor());
        registry.Register(new("process.start", "Start Process", "Local"), new ProcessStartActionExecutor());
        return registry;
    }
}
