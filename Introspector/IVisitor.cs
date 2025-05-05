namespace Introspector;

internal interface IVisitor
{
    void Visit(Case value);

    void Visit(Component value);

    void Visit(Call value);

    void Visit(Comment value);
}

internal abstract class Visitor : IVisitor
{
    public virtual void Visit(Case value) {}

    public virtual void Visit(Component value) {}

    public virtual void Visit(Call value) {}

    public virtual void Visit(Comment value) {}
}
