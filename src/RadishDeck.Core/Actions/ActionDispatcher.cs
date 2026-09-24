using RadishDeck.Core.Models;
using RadishDeck.Core.Execution;

namespace RadishDeck.Core.Actions;

public sealed class ActionDispatcher
{
    private readonly ActionRegistry _registry;
    public ActionDispatcher(ActionRegistry registry) => _registry = registry;

    public Task<State> ExecuteAsync(Element element, Models.Action action, CancellationToken cancellationToken = default)
    {
        if (!_registry.TryGet(action.Type, out _, out var executor) || executor is null)
            return Task.FromResult(new State { Status = "Error", Message = $"Unknown action: {action.Type}" });
        return ExecuteCoreAsync(executor, element, action, cancellationToken);
    }

    private static async Task<State> ExecuteCoreAsync(IActionExecutor executor, Element element, Models.Action action, CancellationToken cancellationToken)
    {
        try { return await executor.ExecuteAsync(element, action, cancellationToken).ConfigureAwait(false); }
        catch (Exception exception) { return new State { Status = "Error", Message = exception.Message }; }
    }
}
