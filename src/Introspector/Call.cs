namespace Introspector;

public sealed class Call : Element
{
    internal Call(Case @case, Component from, Component to, string text, Comparer order)
    {
        Case = @case ?? throw new ArgumentNullException(nameof(@case));
        From = from ?? throw new ArgumentNullException(nameof(from));
        To = to ?? throw new ArgumentNullException(nameof(to));
        Order = order ?? throw new ArgumentNullException(nameof(order));
        Text = text;
    }
    
    public Case Case { get; }

    public Component From { get; }

    public Component To { get; }

    public string Text { get; }

    public Comparer Order { get; }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}