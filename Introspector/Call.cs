using System.Text;
using System.Xml;

namespace Introspector;

internal sealed class Call : Element
{
    private record InnerCase(string Key, string Name, float? Order);
    private record InnerComponent(string Key, string Name);

    private readonly List<InnerCase> cases;
    private readonly List<InnerComponent> from;
    private readonly List<InnerComponent> to;
    private readonly string text;

    private Call(List<InnerCase> cases, List<InnerComponent> from, List<InnerComponent> to, string text)
    {
        this.cases = cases;
        this.from = from;
        this.to = to;
        this.text = text;
    }

    public void AddCase(string key, string name, float? order)
    {
        cases.Add(new InnerCase(key, name, order));
    }

    public void AddTo(string key, string name)
    {
        to.Add(new InnerComponent(key, name));
    }

    public void AddFrom(string key, string name)
    {
        from.Add(new InnerComponent(key, name));
    }

    public bool HasCase(string name)
    {
        return cases.Any(@case => @case.Name == name);
    }

    public bool ContainsTo(Component component)
    {
        return to.Any(inner => component.HasName(inner.Name));
    }

    public bool ContainsFrom(Component component)
    {
        return from.Any(inner => component.HasName(inner.Name));
    }

    public float? GetCaseOrder(string name)
    {
        return cases.Find(inner => inner.Name == name)?.Order;
    }

    public void WriteToSequence(StringBuilder builder)
    {
        foreach (var innerFrom in from)
        {
            foreach (var innerTo in to)
            {
                builder.AppendLine($@"""{innerFrom.Name}"" -> ""{innerTo.Name}"" : ""{text}""");
            }
        }
    }

    public void WriteToComponents(StringBuilder builder)
    {
        foreach (var innerFrom in from)
        {
            foreach (var innerTo in to)
            {
                builder.AppendLine($@"[""{innerFrom.Name}""] --> [""{innerTo.Name}""] : ""{text}""");
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

    public static void DeduplicateAndCleanDependecies(List<Element> elements)
    {
        var deduplicatorAndCleaner = new DependenciesDeduplicatorAndCleaner();

        foreach (var element in elements)
        {
            element.Accept(deduplicatorAndCleaner);
        }
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

    public static void Parse(XmlDocument document, List<Element> elements)
    {
        var members = document.SelectNodes("/doc/members/member");

        if (members == null)
        {
            return;
        }

        foreach (XmlNode member in members)
        {
            var calls = member.SelectNodes("call");

            if (calls == null)
            {
                continue;
            }

            foreach (XmlNode call in calls)
            {
                if (TryParse(call, out var result))
                {
                    elements.Add(result);
                }
            }
        }
    }

    private static bool TryParse(XmlNode node, out Call result)
    {
        result = null;

        var cases = ParseCases(node.SelectNodes("case"));

        if (cases.Count == 0)
        {
            return false;
        }

        var from = ParseComponents(node.SelectNodes("from"));

        if (from.Count == 0)
        {
            return false;
        }

        var to = ParseComponents(node.SelectNodes("to"));

        if (to.Count == 0)
        {
            return false;
        }

        var text = node.SelectSingleNode("text/text()")?.Value;

        result = new Call(cases, from, to, text);

        return true;
    }

    private static List<InnerCase> ParseCases(XmlNodeList nodes)
    {
        if (nodes == null)
        {
            return null;
        }

        var results = new List<InnerCase>();

        foreach (XmlNode @case in nodes)
        {
            var key = @case.SelectSingleNode("@cref")?.Value;
            var name = @case.SelectSingleNode("@name")?.Value;

            if (string.IsNullOrEmpty(key) && string.IsNullOrEmpty(name))
            {
                continue;
            }

            float? order = float.TryParse(@case.SelectSingleNode("@order")?.Value, out var parsedOrder)
                ? parsedOrder
                : null;

            results.Add(new InnerCase(key, name?.ToLower(), order));
        }

        return results;
    }

    private static List<InnerComponent> ParseComponents(XmlNodeList nodes)
    {
        if (nodes == null)
        {
            return null;
        }

        var results = new List<InnerComponent>();

        foreach (XmlNode component in nodes)
        {
            var key = component.SelectSingleNode("@cref")?.Value;
            var name = component.SelectSingleNode("@name")?.Value;

            if (string.IsNullOrEmpty(key) && string.IsNullOrEmpty(name))
            {
                continue;
            }

            results.Add(new InnerComponent(key, name?.ToLower()));
        }

        return results;
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
                .SelectMany(call => call.from.Select(component => component.Name))
                .Where(name => !string.IsNullOrWhiteSpace(name));
            var dependentComponentToNames = calls
                .SelectMany(call => call.to.Select(component => component.Name))
                .Where(name => !string.IsNullOrWhiteSpace(name));

            foreach (var caseName in dependentCaseNames.Distinct())
            {
                if (!cases.Any(@case => @case.HasName(caseName)))
                {
                    Case.Create(elements, caseName);
                }
            }

            foreach (var componentName in dependentComponentFromNames.Concat(dependentComponentToNames).Distinct())
            {
                if (!components.Any(component => component.HasName(componentName)))
                {
                    Component.Create(elements, componentName);
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
                cases.Find(@case => @case.HasKey(inner.Key))?.AddToCall(call, inner.Order);
            }
        }

        private void MatchComponentsTo(Call call)
        {
            foreach (var inner in call.to.ToList())
            {
                components.Find(component => component.HasKey(inner.Key))?.AddToCall(call);
            }
        }

        private void MatchComponentsFrom(Call call)
        {
            foreach (var inner in call.from.ToList())
            {
                components.Find(component => component.HasKey(inner.Key))?.AddFromCall(call);
            }
        }
    }

    private class DependenciesDeduplicatorAndCleaner : IVisitor
    {
        private readonly HashSet<string> names = new();

        public void Visit(Call value)
        {
            names.Clear();
            value.cases.RemoveAll(IsRemoved);
            names.Clear();
            value.to.RemoveAll(IsRemoved);
            names.Clear();
            value.from.RemoveAll(IsRemoved);
        }

        public void Visit(Case value)
        {
        }

        public void Visit(Component value)
        {
        }

        public void Visit(Comment value)
        {
        }

        private bool IsRemoved(InnerCase inner)
        {
            return string.IsNullOrWhiteSpace(inner.Name) || !names.Add(inner.Name);
        }

        private bool IsRemoved(InnerComponent inner)
        {
            return string.IsNullOrWhiteSpace(inner.Name) || !names.Add(inner.Name);
        }
    }
}