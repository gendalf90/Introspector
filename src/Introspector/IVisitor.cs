namespace Introspector;

internal interface IVisitor
{
    void Visit(Case value);

    void Visit(Component value);

    void Visit(Call value);

    void Visit(Comment value);
}
