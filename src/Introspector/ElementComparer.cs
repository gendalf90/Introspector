namespace Introspector;

public sealed class ElementComparer : IComparer<Element>
{
    public static IComparer<Element> Default { get; } = new ElementComparer();

    private static readonly ThreadLocal<OrderVisitor> visitor = new(() => new OrderVisitor());
    
    public int Compare(Element x, Element y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);

        x.Accept(visitor.Value);

        var firstOrder = visitor.Value.GetOrder();

        y.Accept(visitor.Value);

        var secondOrder = visitor.Value.GetOrder();

        return Comparer<Comparer>.Default.Compare(firstOrder, secondOrder);
    }

    private class OrderVisitor : IVisitor
    {
        private Comparer order;

        public void Visit(Case value)
        {
            order = null;
        }

        public void Visit(Component value)
        {
            order = null;
        }

        public void Visit(Call value)
        {
            order = value.Order;
        }

        public void Visit(Comment value)
        {
            order = value.Order;
        }

        public Comparer GetOrder()
        {
            return order;
        }
    }
}