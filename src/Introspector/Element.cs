namespace Introspector;

internal abstract class Element
{
    public abstract void Accept(IVisitor visitor);
}