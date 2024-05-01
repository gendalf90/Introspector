namespace Introspector;

internal record ParsedComment(
    string Is = null,
    string Of = null,
    string Name = null,
    string Type = null,
    string From = null,
    string To = null,
    string Note = null,
    float? Order = null
);