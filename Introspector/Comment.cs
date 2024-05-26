namespace Introspector;

internal abstract class Comment
{
    public abstract void Accept(ICommentVisitor visitor);
}