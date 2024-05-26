namespace Introspector;

internal interface ICommentVisitor
{
    void Visit(Case value);

    void Visit(Message value);

    void Visit(Participant value);
}

internal class CommentVisitor : ICommentVisitor
{
    public virtual void Visit(Case value)
    {
    }

    public virtual void Visit(Message value)
    {
    }

    public virtual void Visit(Participant value)
    {
    }
}
