namespace Introspector;

internal class ParsedComment
{
    public string Is { get; set; }

    public string Of { get; set; }

    public ParsedComment[] OfList { get; set; }

    public string Name { get; set; }

    public string Type { get; set; }

    public string From { get; set; }

    public string To { get; set; }

    public string Text { get; set; }

    public string Over { get; set; }

    public float? Order { get; set; }

    public float? Scale { get; set; }
}