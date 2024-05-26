namespace Introspector;

internal class Participant : Comment
{
    private const string Key = "participant";
    private const string ComponentType = "component";
    private const int DefaultTypeIndex = 0;

    private static string[] ValidTypes = ["participant", "actor", "database", "queue", "boundary", "control", "entity", "collections"];

    private readonly string name;
    private readonly string type;
    private readonly float? scale;

    private Participant(string name, string type, float? scale)
    {
        this.name = name;
        this.type = type;
        this.scale = scale;
    }

    public void FillComponents(ComponentsDto components)
    {
        var participant = components.Participants.FirstOrDefault(p => string.Equals(name, p.Name, StringComparison.OrdinalIgnoreCase));

        if (participant == null)
        {
            components.Participants.Add(new ComponentsDto.ParticipantDto
            {
                Name = name,
                Scale = scale,
                Type = GetComponentType()
            });
        }
        else
        {
            participant.Scale = scale;
            participant.Type = GetComponentType();
        }
    }

    private string GetComponentType()
    {
        return string.IsNullOrEmpty(type) || string.Equals(type, ValidTypes[DefaultTypeIndex], StringComparison.OrdinalIgnoreCase)
            ? ComponentType
            : type;
    }

    public void FillSequence(SequenceDto sequence)
    {
        var participant = sequence.Participants.FirstOrDefault(p => string.Equals(name, p.Name, StringComparison.OrdinalIgnoreCase));

        if (participant == null)
        {
            sequence.Participants.Add(new SequenceDto.ParticipantDto
            {
                Name = name,
                Type = GetSequenceType(),
                Scale = scale
            });
        }
        else
        {
            participant.Type = GetSequenceType();
            participant.Scale = scale;
        }
    }

    private string GetSequenceType()
    {
        return string.IsNullOrEmpty(type)
            ? ValidTypes[DefaultTypeIndex]
            : type;
    }

    public override void Accept(ICommentVisitor visitor)
    {
        visitor.Visit(this);
    }

    public static bool TryCreate(ParsedComment value, out Participant result)
    {
        result = null;

        if (!string.Equals(value.Is, Key, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrEmpty(value.Name))
        {
            return false;
        }

        if (value.Type != null && !ValidTypes.Contains(value.Type, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        result = new Participant(value.Name, value.Type, value.Scale);

        return true;
    }
}
