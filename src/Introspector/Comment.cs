using System.Text;
using System.Xml;

namespace Introspector;

internal sealed class Comment : Element
{
    private record InnerCase(string Key, string Name, float? Order);
    private record InnerComponent(string Key, string Name);

    private readonly List<InnerCase> cases;
    private readonly List<InnerComponent> over;
    private readonly string text;

    private Comment(List<InnerCase> cases, List<InnerComponent> over, string text)
    {
        this.cases = cases;
        this.over = over;
        this.text = text;
    }

    public void AddCase(string key, string name, float? order)
    {
        cases.Add(new InnerCase(key, name, order));
    }

    public void AddOver(string key, string name)
    {
        over.Add(new InnerComponent(key, name));
    }

    public bool HasCase(string name)
    {
        return cases.Any(@case => @case.Name == name);
    }

    public bool ContainsOver(Component component)
    {
        return over.Any(inner => component.HasName(inner.Name));
    }

    public float? GetCaseOrder(string name)
    {
        return cases.Find(inner => inner.Name == name)?.Order;
    }

    public void WriteToSequence(StringBuilder builder)
    {
        var components = over.Select(inner => $@"""{inner.Name}""").ToArray();

        builder.AppendLine($@"note over {string.Join(',', components)}");
        builder.AppendLine($@"""{text}""");
        builder.AppendLine("end note");
    }

    public void WriteToComponent(StringBuilder builder, Component component)
    {
        foreach (var inner in over)
        {
            if (component.HasName(inner.Name))
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
            var comments = member.SelectNodes("comment");

            if (comments == null)
            {
                continue;
            }

            foreach (XmlNode comment in comments)
            {
                if (TryParse(comment, out var result))
                {
                    elements.Add(result);
                }
            }
        }
    }

    private static bool TryParse(XmlNode node, out Comment result)
    {
        result = null;

        var cases = ParseCases(node.SelectNodes("case"));

        if (cases.Count == 0)
        {
            return false;
        }

        var over = ParseComponents(node.SelectNodes("over"));

        if (over.Count == 0)
        {
            return false;
        }

        var text = node.SelectSingleNode("text/text()")?.Value;

        result = new Comment(cases, over, text);

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

            results.Add(new InnerCase(key, name, order));
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

            results.Add(new InnerComponent(key, name));
        }

        return results;
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
                .SelectMany(comment => comment.over.Select(component => component.Name))
                .Where(name => !string.IsNullOrWhiteSpace(name));

            foreach (var caseName in dependentCaseNames.Distinct())
            {
                if (!cases.Any(@case => @case.HasName(caseName)))
                {
                    Case.Create(elements, caseName);
                }
            }

            foreach (var componentName in dependentComponentOverNames.Distinct())
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
                foreach (var found in cases.Where(@case => @case.HasKey(inner.Key)))
                {
                    found.AddToComment(comment, inner.Order);
                }
            }
        }

        private void MatchComponentsOver(Comment comment)
        {
            foreach (var inner in comment.over.ToList())
            {
                foreach (var found in components.Where(component => component.HasKey(inner.Key)))
                {
                    found.AddToComment(comment);
                }
            }
        }
    }

    private class DependenciesDeduplicatorAndCleaner : IVisitor
    {
        private readonly HashSet<string> names = new();

        public void Visit(Comment value)
        {
            names.Clear();
            value.cases.RemoveAll(IsRemoved);
            names.Clear();
            value.over.RemoveAll(IsRemoved);
        }

        public void Visit(Case value)
        {
        }

        public void Visit(Component value)
        {
        }

        public void Visit(Call value)
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