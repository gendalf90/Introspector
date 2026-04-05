namespace Introspector;

public class Comparer : IComparable<Comparer>
{
    private float? order;
    
    internal Comparer(float? order)
    {
        this.order = order;
    }

    internal void Set(float? order)
    {
        this.order = order;
    }
    
    public int CompareTo(Comparer other)
    {
        return Comparer<float?>.Default.Compare(order, other.order);
    }

    public static Comparer Null { get; } = new Comparer(null);
}