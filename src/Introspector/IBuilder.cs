namespace Introspector;

public interface IBuilder
{
    IBuilder AddCase(string name, string text);

    IBuilder AddComponent(string name, string type, string text);

    IBuilder AddCall(Action<ICallBuilder> configure);

    IBuilder AddComment(Action<ICommentBuilder> configure);
}

public interface ICallBuilder
{
    ICallBuilder AddCase(string name, float? order);

    ICallBuilder AddTo(string name);

    ICallBuilder AddFrom(string name);

    ICallBuilder SetText(string text);
}

public interface ICommentBuilder
{
    ICommentBuilder AddCase(string name, float? order);

    ICommentBuilder AddOver(string name);

    ICommentBuilder SetText(string text);
}