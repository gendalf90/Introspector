using System.Text;
using System.Xml;

namespace Introspector;

internal sealed class Component : Element, IEquatable<Component>
{
    private const int DefaultTypeIndex = 0;
    private static string[] ValidTypes = ["participant", "actor", "database", "queue", "boundary", "control", "entity", "collections"];

    private readonly string key;
    private readonly string name;
    private readonly string type;
    private readonly string text;

    private Component(string key, string name, string type, string text)
    {
        this.key = key;
        this.name = name;
        this.type = type;
        this.text = text;
    }

    public void AddToCall(Call call, float? order)
    {
        call.AddCase(key, name, order);
    }

    public void AddToComment(Comment comment)
    {
        comment.AddOver(key, name);
    }

    public void AddToCall(Call call)
    {
        call.AddTo(key, name);
    }

    public void AddFromCall(Call call)
    {
        call.AddFrom(key, name);
    }

    public bool HasKey(string value)
    {
        return key == value;
    }

    public bool HasName(string value)
    {
        return name == value;
    }

    public void WriteToSequence(StringBuilder builder)
    {
        builder.AppendLine($@"{GetValidType()} ""{name}""");

        if (!string.IsNullOrWhiteSpace(text))
        {
            builder.AppendLine($@"/ note over ""{name}""");
            builder.AppendLine($@"""{text}""");
            builder.AppendLine("end note");
        }
    }

    public void WriteToComponents(StringBuilder builder, IEnumerable<Comment> comments)
    {
        builder.AppendLine($@"[""{name}""]");

        var componentComments = new List<Action>();

        if (!string.IsNullOrWhiteSpace(text))
        {
            componentComments.Add(() => builder.AppendLine($@"""{text}"""));
        }

        componentComments.AddRange(comments
            .Where(comment => comment.ContainsOver(this))
            .Select<Comment, Action>(comment => () => comment.WriteToComponent(builder, this)));

        if (componentComments.Count == 0)
        {
            return;
        }

        builder.AppendLine($@"note right of [""{name}""]");

        for (int i = 0; i < componentComments.Count; i++)
        {
            if (i != 0)
            {
                builder.AppendLine("----");
            }

            componentComments[i].Invoke();
        }

        builder.AppendLine("end note");
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    private string GetValidType()
    {
        if (string.IsNullOrEmpty(type))
        {
            return ValidTypes[DefaultTypeIndex];
        }

        if (!ValidTypes.Contains(type))
        {
            return ValidTypes[DefaultTypeIndex];
        }

        return type;
    }

    private string GetComponentsDescription()
    {
        return string.IsNullOrWhiteSpace(text)
        ? string.Empty
        : @$"[{name}\n----\n{text}] as";
    }

    public static void Create(List<Element> elements, string name)
    {
        elements.Add(new Component(null, name, null, null));
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
            var components = member.SelectNodes("component");

            if (components == null)
            {
                continue;
            }

            foreach (XmlNode component in components)
            {
                if (TryParse(member, component, out var result))
                {
                    elements.Add(result);
                }
            }
        }
    }

    public static void Deduplicate(List<Element> elements)
    {
        var deduplicator = new Deduplicator(elements);

        foreach (var element in elements)
        {
            element.Accept(deduplicator);
        }

        deduplicator.Remove();
    }

    private static bool TryParse(XmlNode member, XmlNode node, out Component result)
    {
        result = null;

        var key = member.SelectSingleNode("@name")?.Value;
        var name = node.SelectSingleNode("@name")?.Value;
        var type = node.SelectSingleNode("@type")?.Value;
        var text = node.SelectSingleNode("text()")?.Value;

        name = string.IsNullOrEmpty(name)
            ? GetNameFromKey(key)
            : name;

        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        result = new Component(key, name, type?.ToLower(), TrimText(text));

        return true;
    }

    private static string TrimText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        return string.Join('\n', text.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
    }

    private static string GetNameFromKey(string key)
    {
        return key.Split(':', '.').LastOrDefault();
    }

    public bool Equals(Component other)
    {
        return name == other.name;
    }

    private class Deduplicator : IVisitor
    {
        private readonly HashSet<string> names = new();
        private readonly List<Component> toRemove = new();
        private readonly List<Element> elements;

        public Deduplicator(List<Element> elements)
        {
            this.elements = elements;
        }

        public void Visit(Component value)
        {
            if (!names.Add(value.name))
            {
                toRemove.Add(value);
            }
        }

        public void Visit(Case value)
        {
        }

        public void Visit(Call value)
        {
        }

        public void Visit(Comment value)
        {
        }

        public void Remove()
        {
            foreach (var component in toRemove)
            {
                elements.Remove(component);
            }
        }
    }
}