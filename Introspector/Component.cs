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
    private readonly float? scale;

    private Component(string key, string name, string type, float? scale)
    {
        this.key = key;
        this.name = name;
        this.type = type;
        this.scale = scale;
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

    public bool IsFiltered(float? scale)
    {
        return this.scale > scale;
    }

    public void WriteToSequence(StringBuilder builder)
    {
        builder.AppendLine($@"{GetValidType()} ""{name}""");
    }

    public void WriteToComponents(StringBuilder builder)
    {
        builder.AppendLine($@"[""{name}""]");
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

        float? scale = float.TryParse(node.SelectSingleNode("@scale")?.Value, out var parsedScale)
            ? parsedScale
            : null;

        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        result = new Component(key, name.ToLower(), type?.ToLower(), scale);

        return true;
    }

    public bool Equals(Component other)
    {
        return name == other.name;
    }

    private class Deduplicator : Visitor
    {
        private readonly HashSet<string> names = new();
        private readonly List<Component> toRemove = new();
        private readonly List<Element> elements;

        public Deduplicator(List<Element> elements)
        {
            this.elements = elements;
        }

        public override void Visit(Component value)
        {
            if (!names.Add(value.name))
            {
                toRemove.Add(value);
            }
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