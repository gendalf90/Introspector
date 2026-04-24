namespace Introspector;

public static class Visitor
{
    public static IVisitor Create(
        Action<Case> onCase = null,
        Action<Component> onComponent = null,
        Action<Call> onCall = null,
        Action<Comment> onComment = null
    )
    {
        return new VisitorDecorator(onCase, onComponent, onCall, onComment);
    }

    private class VisitorDecorator(
        Action<Case> onCase = null,
        Action<Component> onComponent = null,
        Action<Call> onCall = null,
        Action<Comment> onComment = null
    ) : IVisitor
    {
        public void Visit(Case value)
        {
            onCase?.Invoke(value);
        }

        public void Visit(Component value)
        {
            onComponent?.Invoke(value);
        }

        public void Visit(Call value)
        {
            onCall?.Invoke(value);
        }

        public void Visit(Comment value)
        {
            onComment?.Invoke(value);
        }
    }
}
