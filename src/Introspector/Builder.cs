namespace Introspector;

internal class BuilderImpl : IBuilder
{
    private readonly List<Element> elements = new();

    public IBuilder AddCase(string name, string text)
    {
        if (Case.TryCreate(name, text, out var result))
        {
            elements.Add(result);
        }

        return this;
    }

    public IBuilder AddComponent(string name, string type, string text)
    {
        if (Component.TryCreate(name, type, text, out var result))
        {
            elements.Add(result);
        }

        return this;
    }

    public IBuilder AddCall(Action<ICallBuilder> configure)
    {
        var builder = new CallBuilderImpl();

        configure?.Invoke(builder);

        if (builder.TryBuild(out var result))
        {
            elements.Add(result);
        }

        return this;
    }

    public IBuilder AddComment(Action<ICommentBuilder> configure)
    {
        var builder = new CommentBuilderImpl();

        configure?.Invoke(builder);

        if (builder.TryBuild(out var result))
        {
            elements.Add(result);
        }

        return this;
    }

    public IEnumerable<Element> Build()
    {
        Call.AddReferencedDependencies(elements);
        Comment.AddReferencedDependencies(elements);
        Call.CreateNotListedDependecies(elements);
        Comment.CreateNotListedDependecies(elements);
        Case.Deduplicate(elements);
        Component.Deduplicate(elements);

        return elements;
    }
}

internal class CallBuilderImpl : ICallBuilder
{
    private List<(string Name, float? Order)> cases = new();
    private List<string> from = new();
    private List<string> to = new();
    private string text;

    public ICallBuilder AddCase(string name, float? order)
    {
        cases.Add((name, order));

        return this;
    }

    public ICallBuilder AddFrom(string name)
    {
        from.Add(name);

        return this;
    }

    public ICallBuilder AddTo(string name)
    {
        to.Add(name);

        return this;
    }

    public ICallBuilder SetText(string text)
    {
        this.text = text;

        return this;
    }

    public bool TryBuild(out Call result)
    {
        return Call.TryCreate(cases.ToArray(), from.ToArray(), to.ToArray(), text, out result);
    }
}

internal class CommentBuilderImpl : ICommentBuilder
{
    private List<(string Name, float? Order)> cases = new();
    private List<string> over = new();
    private string text;

    public ICommentBuilder AddCase(string name, float? order)
    {
        cases.Add((name, order));

        return this;
    }

    public ICommentBuilder AddOver(string name)
    {
        over.Add(name);

        return this;
    }

    public ICommentBuilder SetText(string text)
    {
        this.text = text;

        return this;
    }

    public bool TryBuild(out Comment result)
    {
        return Comment.TryCreate(cases.ToArray(), over.ToArray(), text, out result);
    }
}