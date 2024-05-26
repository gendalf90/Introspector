namespace Introspector;

internal class Case : Comment
{
    private const string Key = "case";

    private readonly string name;
    private readonly string text;

    private Case(string text, string name)
    {
        this.text = text;
        this.name = name;
    }

    public void FillCases(List<CaseDto> cases)
    {
        cases.RemoveAll(dto => string.Equals(dto.Name, name, StringComparison.OrdinalIgnoreCase));
        cases.Add(new CaseDto(name, text));
    }

    public bool HasName(string caseName)
    {
        return string.Equals(caseName, name, StringComparison.OrdinalIgnoreCase);
    }

    public override void Accept(ICommentVisitor visitor)
    {
        visitor.Visit(this);
    }

    public static bool TryCreate(ParsedComment value, out Case result)
    {
        result = null;
        
        if (!string.Equals(value.Is, Key, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrEmpty(value.Name))
        {
            return false;
        }

        result = new Case(value.Text, value.Name);

        return true;
    }
}