using RadishDeck.Core.Models;

namespace RadishDeck.Core.Execution;

public interface IActionExecutor
{
    Task<State> ExecuteAsync(Element element, Models.Action action, CancellationToken cancellationToken = default);
}
