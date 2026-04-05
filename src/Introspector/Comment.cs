namespace Introspector;

public sealed class Comment : Element
{
    internal Comment(Case @case, Component over, string text, Comparer order)
    {
        Case = @case ?? throw new ArgumentNullException(nameof(@case));
        Order = order ?? throw new ArgumentNullException(nameof(order));
        Text = text;
        Over = over;
    }
    
    public Case Case { get; }

    public Component Over { get; }

    public string Text { get; }

    public Comparer Order { get; }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}