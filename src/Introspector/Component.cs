namespace Introspector;

public enum ComponentType
{
    Default = 0,
    Database = 1
}

public sealed class Component : Element, IEquatable<Component>
{
    internal Component(string key, ComponentType type, string description)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(nameof(key));
        }
        
        Key = key;
        Type = type;
        Description = description;
    }
    
    public string Key { get; }

    public ComponentType Type { get; }

    public string Description { get; }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Component);
    }

    public override int GetHashCode()
    {
        return Key.GetHashCode();
    }

    public bool Equals(Component other)
    {
        return other != null && string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase);
    }
}