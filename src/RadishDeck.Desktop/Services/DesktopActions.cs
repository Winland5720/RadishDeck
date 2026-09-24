using RadishDeck.Core.Actions;
using RadishDeck.Infrastructure.Actions;

namespace RadishDeck.Desktop.Services;

public static class DesktopActions
{
    public static ActionRegistry Create(
        ServerLauncherService server,
        Func<ServerConfiguration> configuration)
    {
        // Берём общие действия
        var registry = ActionCatalog.Create();

        // Добавляем только Desktop-специфичные
        registry.Register(
            new("server.start", "Start Server", "Server"),
            new ServerStartActionExecutor(server, configuration));

        registry.Register(
            new("server.stop", "Stop Server", "Server"),
            new ServerStopActionExecutor(server));

        return registry;
    }
}