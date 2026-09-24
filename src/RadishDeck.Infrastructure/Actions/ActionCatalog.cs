using RadishDeck.Core.Actions;
using RadishDeck.Infrastructure.Actions.Executors;

namespace RadishDeck.Infrastructure.Actions;

public static class ActionCatalog
{
    public static ActionRegistry Create()
    {
        var registry = new ActionRegistry();

        registry.Register(
            new(
                "process.start",
                "Start Process",
                "Local"
            ),
            new ProcessStartActionExecutor()
        );

        registry.Register(
            new(
                "url.open",
                "Open URL",
                "Local"
            ),
            new UrlOpenActionExecutor()
        );

        return registry;
    }
}