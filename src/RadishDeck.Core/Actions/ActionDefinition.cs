namespace RadishDeck.Core.Actions;

public sealed record ActionDefinition(string Id, string Name, string Category, string Description = "");
