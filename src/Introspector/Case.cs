using System.Text;

namespace Introspector;

internal class Case : Element
{
    private readonly string name;
    private readonly string text;

    private Case(string name, string text)
    {
        this.name = name;
        this.text = text;
    }

    public void AddToCall(Call call, float? order)
    {
        call.AddCase(name, order);
    }

    public void AddToComment(Comment comment, float? order)
    {
        comment.AddCase(name, order);
    }

    public bool HasName(string value)
    {
        return name == value;
    }

    public void WriteUseCase(StringBuilder builder)
    {
        builder.AppendLine(@$"usecase ""{name}""");

        if (!string.IsNullOrWhiteSpace(text))
        {
            builder.AppendLine(@$"note right of ""{name}""");
            builder.AppendLine(text);
            builder.AppendLine("end note");
        }
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

    public static bool TryCreate(string name, string text, out Case result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        result = new Case(name, text);

        return true;
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

    private class Deduplicator : IVisitor
    {
        private readonly HashSet<string> names = new();
        private readonly List<Case> toRemove = new();
        private readonly List<Element> elements;

        public Deduplicator(List<Element> elements)
        {
            this.elements = elements;
        }

        public void Visit(Case value)
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

        public void Visit(Component value)
        {
        }

        public void Visit(Call value)
        {
        }

        public void Visit(Comment value)
        {
        }
    }
}