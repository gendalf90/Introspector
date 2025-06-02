using System.Text;

namespace Introspector;

internal class Component : Element, IEquatable<Component>
{
    private const int DefaultTypeIndex = 0;
    private static string[] ValidTypes = ["participant", "actor", "database", "queue", "boundary", "control", "entity", "collections"];

    private readonly string name;
    private readonly string type;
    private readonly string text;

    private Component(string name, string type, string text)
    {
        this.name = name;
        this.type = type;
        this.text = text;
    }

    public void LinkComment(Comment comment)
    {
        comment.AddOver(name);
    }

    public void LinkTo(Call call)
    {
        call.AddTo(name);
    }

    public void LinkFrom(Call call)
    {
        call.AddFrom(name);
    }

    public bool HasName(string value)
    {
        return name == value;
    }

    public void WriteToSequence(StringBuilder builder)
    {
        builder.AppendLine($@"{type} ""{name}""");

        if (!string.IsNullOrWhiteSpace(text))
        {
            builder.AppendLine($@"/ note over ""{name}""");
            builder.AppendLine(text);
            builder.AppendLine("end note");
        }
    }

    public void WriteToComponents(StringBuilder builder, IEnumerable<Comment> comments)
    {
        builder.AppendLine($@"[""{name}""]");

        var componentComments = new List<Action>();

        if (!string.IsNullOrWhiteSpace(text))
        {
            componentComments.Add(() => builder.AppendLine(text));
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

    public static void Deduplicate(List<Element> elements)
    {
        var deduplicator = new Deduplicator(elements);

        foreach (var element in elements)
        {
            element.Accept(deduplicator);
        }

        deduplicator.Remove();
    }

    public static bool TryCreate(string name, string type, string text, out Component result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var resultType = string.IsNullOrWhiteSpace(type) || !ValidTypes.Contains(type)
            ? ValidTypes[DefaultTypeIndex]
            : type;

        result = new Component(name, resultType, text);

        return true;
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