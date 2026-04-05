namespace Introspector;

public sealed class Case : Element, IEquatable<Case>
{
    internal Case(string key, string description)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(nameof(key));
        }
        
        Key = key;
        Description = description;
    }
    
    public string Key { get; }

    public string Description { get; }
    
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Case);
    }

    public override int GetHashCode()
    {
        return Key.GetHashCode();
    }

    public bool Equals(Case other)
    {
        return other != null && string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase);
    }
}