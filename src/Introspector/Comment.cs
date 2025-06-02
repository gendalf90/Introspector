using System.Text;

namespace Introspector;

internal class Comment : Element
{
    private record InnerCase(string Name, float? Order);

    private readonly List<InnerCase> cases = new();
    private readonly HashSet<string> over = new();
    private readonly string text;

    private Comment(string text)
    {
        this.text = text;
    }

    public void AddCase(string name, float? order)
    {
        bool HasNoOrder(InnerCase inner) => inner.Name == name && !inner.Order.HasValue;

        bool HasOrder(InnerCase inner) => inner.Name == name && inner.Order.HasValue;

        bool HasCurrentOrder(InnerCase inner) => inner.Name == name && inner.Order == order;

        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        if (order.HasValue && cases.Any(HasNoOrder))
        {
            cases.RemoveAll(HasNoOrder);
        }

        if (!order.HasValue && cases.Any(HasOrder))
        {
            return;
        }

        if (cases.Any(HasCurrentOrder))
        {
            return;
        }

        cases.Add(new InnerCase(name, order));
    }

    public void AddOver(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        over.Add(name);
    }

    public bool HasCase(string name)
    {
        return cases.Any(@case => @case.Name == name);
    }

    public bool ContainsOver(Component component)
    {
        return over.Any(component.HasName);
    }

    public float?[] GetCaseOrders(Case @case)
    {
        return cases
            .Where(inner => @case.HasName(inner.Name))
            .Select(inner => inner.Order)
            .ToArray();
    }

    public void WriteToSequence(StringBuilder builder)
    {
        var components = over.Select(inner => $@"""{inner}""").ToArray();

        builder.AppendLine($@"note over {string.Join(',', components)}");
        builder.AppendLine($@"""{text}""");
        builder.AppendLine("end note");
    }

    public void WriteToComponent(StringBuilder builder, Component component)
    {
        foreach (var inner in over)
        {
            if (component.HasName(inner))
            {
                builder.AppendLine($@"""{text}""");
            }
        }
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public static void CreateNotListedDependecies(List<Element> elements)
    {
        var creator = new NotListedDependeciesCreator();

        foreach (var element in elements)
        {
            element.Accept(creator);
        }

        creator.Create(elements);
    }

    public static void AddReferencedDependencies(List<Element> elements)
    {
        var matcher = new ReferencedDependenciesCreator();

        foreach (var element in elements)
        {
            element.Accept(matcher);
        }

        matcher.Match();
    }

    public static bool TryCreate(
        (string Name, float? Order)[] cases,
        string[] over,
        string text,
        out Comment result)
    {
        result = null;

        var resultCases = cases?.Where(c => !string.IsNullOrWhiteSpace(c.Name));
        var resultOver = over?.NotEmpty();

        if (resultCases.IsNullOrEmpty() || resultOver.IsNullOrEmpty())
        {
            return false;
        }

        result = new Comment(text);

        foreach (var value in resultCases)
        {
            result.AddCase(value.Name, value.Order);
        }

        foreach (var value in resultOver)
        {
            result.AddOver(value);
        }

        return true;
    }

    private class NotListedDependeciesCreator : IVisitor
    {
        private readonly List<Case> cases = new();
        private readonly List<Component> components = new();
        private readonly List<Comment> comments = new();

        public void Visit(Case value)
        {
            cases.Add(value);
        }

        public void Visit(Component value)
        {
            components.Add(value);
        }

        public void Visit(Comment value)
        {
            comments.Add(value);
        }

        public void Visit(Call value)
        {
        }

        public void Create(List<Element> elements)
        {
            var dependentCaseNames = comments
                .SelectMany(comment => comment.cases.Select(@case => @case.Name))
                .Where(name => !string.IsNullOrWhiteSpace(name));
            var dependentComponentOverNames = comments
                .SelectMany(comment => comment.over.Select(component => component))
                .Where(name => !string.IsNullOrWhiteSpace(name));

            foreach (var caseName in dependentCaseNames.Distinct())
            {
                if (!cases.Any(@case => @case.HasName(caseName)) && Case.TryCreate(caseName, null, out var newCase))
                {
                    elements.Add(newCase);
                }
            }

            foreach (var componentName in dependentComponentOverNames.Distinct())
            {
                if (!components.Any(component => component.HasName(componentName)) && Component.TryCreate(componentName, null, null, out var newComponent))
                {
                    elements.Add(newComponent);
                }
            }
        }
    }

    private class ReferencedDependenciesCreator : IVisitor
    {
        private readonly List<Case> cases = new();
        private readonly List<Component> components = new();
        private readonly List<Comment> comments = new();

        public void Visit(Case value)
        {
            cases.Add(value);
        }

        public void Visit(Component value)
        {
            components.Add(value);
        }

        public void Visit(Comment value)
        {
            comments.Add(value);
        }

        public void Visit(Call value)
        {
        }

        public void Match()
        {
            foreach (var comment in comments)
            {
                MatchCases(comment);
                MatchComponentsOver(comment);
            }
        }

        private void MatchCases(Comment comment)
        {
            foreach (var inner in comment.cases.ToList())
            {
                foreach (var found in cases.Where(@case => @case.HasName(inner.Name)))
                {
                    found.AddToComment(comment, inner.Order);
                }
            }
        }

        private void MatchComponentsOver(Comment comment)
        {
            foreach (var inner in comment.over.ToList())
            {
                foreach (var found in components.Where(component => component.HasName(inner)))
                {
                    found.LinkComment(comment);
                }
            }
        }
    }
}