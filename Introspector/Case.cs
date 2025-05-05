using System.Text;
using System.Text.Json;
using System.Xml;

namespace Introspector;

internal sealed class Case : Element
{
    private readonly string key;
    private readonly string name;
    private readonly string text;

    private Case(string key, string text, string name)
    {
        this.key = key;
        this.text = text;
        this.name = name;
    }

    public void AddToCall(Call call, float? order)
    {
        call.AddCase(key, name, order);
    }

    public void AddToComment(Comment comment, float? order)
    {
        comment.AddCase(key, name, order);
    }

    public bool HasName(string value)
    {
        return name == value;
    }

    public bool HasKey(string value)
    {
        return key == value;
    }

    public void WriteJson(StringBuilder builder)
    {
        builder.Append(JsonSerializer.Serialize(new
        {
            Name = name,
            Text = text
        }));
    }

    public void WriteSequenceTitle(StringBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }
        
        builder.AppendLine("title");
        builder.AppendLine(text);
        builder.AppendLine("end title");
    }

    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    public static void Create(List<Element> elements, string name)
    {
        elements.Add(new Case(null, null, name));
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
            var cases = member.SelectNodes("case");

            if (cases == null)
            {
                continue;
            }

            foreach (XmlNode @case in cases)
            {
                if (TryParse(member, @case, out var result))
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

    private static bool TryParse(XmlNode member, XmlNode node, out Case result)
    {
        result = null;

        var key = member.SelectSingleNode("@name")?.Value;
        var name = node.SelectSingleNode("@name")?.Value;
        var text = node.SelectSingleNode("text()")?.Value;

        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        result = new Case(key, text, name.ToLower());

        return true;
    }

    private class Deduplicator : Visitor
    {
        private readonly HashSet<string> names = new();
        private readonly List<Case> toRemove = new();
        private readonly List<Element> elements;

        public Deduplicator(List<Element> elements)
        {
            this.elements = elements;
        }

        public override void Visit(Case value)
        {
            if (!names.Add(value.name))
            {
                toRemove.Add(value);
            }
        }

        public void Remove()
        {
            foreach (var @case in toRemove)
            {
                elements.Remove(@case);
            }
        }
    }
}