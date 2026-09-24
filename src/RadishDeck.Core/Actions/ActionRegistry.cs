using RadishDeck.Core.Execution;
namespace RadishDeck.Core.Actions;

public sealed class ActionRegistry
{
    private readonly Dictionary<string, (ActionDefinition Definition, IActionExecutor Executor)> _items = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<ActionDefinition> Definitions => _items.Values.Select(x => x.Definition).ToArray();

    public void Register(ActionDefinition definition, IActionExecutor executor)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(executor);
        if (string.IsNullOrWhiteSpace(definition.Id)) throw new ArgumentException("Action id is required", nameof(definition));
        if (!_items.TryAdd(definition.Id, (definition, executor)))
            throw new InvalidOperationException($"Action already registered: {definition.Id}");
    }

    public bool TryGet(string id, out ActionDefinition? definition, out IActionExecutor? executor)
    {
        if (!string.IsNullOrWhiteSpace(id) && _items.TryGetValue(id, out var value)) { definition = value.Definition; executor = value.Executor; return true; }
        definition = null; executor = null; return false;
    }
}
