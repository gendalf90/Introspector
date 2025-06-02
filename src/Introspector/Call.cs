using System.Text;

namespace Introspector;

internal class Call : Element
{
    private record InnerCase(string Name, float? Order);

    private readonly List<InnerCase> cases = new();
    private readonly HashSet<string> from = new();
    private readonly HashSet<string> to = new();
    private readonly string text;

    private Call(string text)
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

    public void AddTo(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        to.Add(name);
    }

    public void AddFrom(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        from.Add(name);
    }

    public bool HasCase(string name)
    {
        return cases.Any(@case => @case.Name == name);
    }

    public bool ContainsTo(Component component)
    {
        return to.Any(component.HasName);
    }

    public bool ContainsFrom(Component component)
    {
        return from.Any(component.HasName);
    }

    public float? GetFirstCaseOrder(Case @case)
    {
        return cases
            .Where(inner => @case.HasName(inner.Name))
            .Select(inner => inner.Order)
            .Order()
            .FirstOrDefault();
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
        foreach (var innerFrom in from)
        {
            foreach (var innerTo in to)
            {
                builder.AppendLine($@"""{innerFrom}"" -> ""{innerTo}"" : {text}");
            }
        }
    }

    public void WriteToComponents(StringBuilder builder)
    {
        foreach (var innerFrom in from)
        {
            foreach (var innerTo in to)
            {
                builder.AppendLine($@"[""{innerFrom}""] --> [""{innerTo}""] : {text}");
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
        string[] from,
        string[] to,
        string text,
        out Call result)
    {
        result = null;

        var resultCases = cases?.Where(c => !string.IsNullOrWhiteSpace(c.Name));
        var resultFrom = from?.NotEmpty();
        var resultTo = to?.NotEmpty();

        if (resultCases.IsNullOrEmpty() || resultFrom.IsNullOrEmpty() || resultTo.IsNullOrEmpty())
        {
            return false;
        }

        result = new Call(text?.JoinLines());

        foreach (var value in resultCases)
        {
            result.AddCase(value.Name, value.Order);
        }

        foreach (var value in resultFrom)
        {
            result.AddFrom(value);
        }

        foreach (var value in resultTo)
        {
            result.AddTo(value);
        }

        return true;
    }

    private class NotListedDependeciesCreator : IVisitor
    {
        private readonly List<Case> cases = new();
        private readonly List<Component> components = new();
        private readonly List<Call> calls = new();

        public void Visit(Case value)
        {
            cases.Add(value);
        }

        public void Visit(Component value)
        {
            components.Add(value);
        }

        public void Visit(Call value)
        {
            calls.Add(value);
        }

        public void Visit(Comment value)
        {
        }

        public void Create(List<Element> elements)
        {
            var dependentCaseNames = calls
                .SelectMany(call => call.cases.Select(@case => @case.Name))
                .Where(name => !string.IsNullOrWhiteSpace(name));
            var dependentComponentFromNames = calls
                .SelectMany(call => call.from.Select(component => component))
                .Where(name => !string.IsNullOrWhiteSpace(name));
            var dependentComponentToNames = calls
                .SelectMany(call => call.to.Select(component => component))
                .Where(name => !string.IsNullOrWhiteSpace(name));

            foreach (var caseName in dependentCaseNames.Distinct())
            {
                if (!cases.Any(@case => @case.HasName(caseName)) && Case.TryCreate(caseName, null, out var newCase))
                {
                    elements.Add(newCase);
                }
            }

            foreach (var componentName in dependentComponentFromNames.Concat(dependentComponentToNames).Distinct())
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
        private readonly List<Call> calls = new();

        public void Visit(Case value)
        {
            cases.Add(value);
        }

        public void Visit(Component value)
        {
            components.Add(value);
        }

        public void Visit(Call value)
        {
            calls.Add(value);
        }

        public void Visit(Comment value)
        {
        }

        public void Match()
        {
            foreach (var call in calls)
            {
                MatchCases(call);
                MatchComponentsTo(call);
                MatchComponentsFrom(call);
            }
        }

        private void MatchCases(Call call)
        {
            foreach (var inner in call.cases.ToList())
            {
                foreach (var found in cases.Where(@case => @case.HasName(inner.Name)))
                {
                    found.AddToCall(call, inner.Order);
                }
            }
        }

        private void MatchComponentsTo(Call call)
        {
            foreach (var inner in call.to.ToList())
            {
                foreach (var found in components.Where(component => component.HasName(inner)))
                {
                    found.LinkTo(call);
                }
            }
        }

        private void MatchComponentsFrom(Call call)
        {
            foreach (var inner in call.from.ToList())
            {
                foreach (var found in components.Where(component => component.HasName(inner)))
                {
                    found.LinkFrom(call);
                }
            }
        }
    }
}