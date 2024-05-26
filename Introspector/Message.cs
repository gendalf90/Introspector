namespace Introspector;

internal class Message : Comment
{
    private const string Key = "message";
    
    private readonly OfItem[] of;
    private readonly string from;
    private readonly string to;
    private readonly string over;
    private readonly string text;
    private readonly bool isNote;
    private readonly bool isMessage;

    private Message(OfItem[] of, string from, string to, string over, string text, bool isNote, bool isMessage)
    {
        this.of = of;
        this.from = from;
        this.to = to;
        this.over = over;
        this.text = text;
        this.isNote = isNote;
        this.isMessage = isMessage;
    }

    public void FillCases(List<CaseDto> cases)
    {
        foreach (var item in of)
        {
            if (!cases.Any(dto => string.Equals(dto.Name, item.Of, StringComparison.OrdinalIgnoreCase)))
            {
                cases.Add(new CaseDto(item.Of, null));
            }
        }
    }

    public void FillComponents(ComponentsDto components)
    {
        if (!string.IsNullOrEmpty(components.Case) && !of.Any(item => string.Equals(components.Case, item.Of, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }
        
        if (isMessage)
        {
            TryAddLink(components);
            AddOrUpdateParticipant(components, from);
            AddOrUpdateParticipant(components, to);
        }
        else if (isNote)
        {
            AddOrUpdateParticipant(components, over);
        }
    }

    private void TryAddLink(ComponentsDto components)
    {
        if (!components.Links.Any(IsCurrentLink))
        {
            components.Links.Add(new ComponentsDto.LinkDto
            {
                From = from,
                To = to
            });
        }
    }

    private bool IsCurrentLink(ComponentsDto.LinkDto link)
    {
        var linkComponents = new[] { link.From, link.To };

        return linkComponents.Contains(from, StringComparer.OrdinalIgnoreCase) && linkComponents.Contains(to, StringComparer.OrdinalIgnoreCase);
    }

    private void AddOrUpdateParticipant(ComponentsDto components, string name)
    {
        var participant = components.Participants.FirstOrDefault(p => string.Equals(name, p.Name, StringComparison.OrdinalIgnoreCase));

        if (participant == null)
        {
            components.Participants.Add(new ComponentsDto.ParticipantDto
            {
                Name = name,
                Used = true,
                Cases = of.Select(item => item.Of).ToList()
            });
        }
        else
        {
            foreach (var ofItem in of)
            {
                if (!participant.Cases.Any(c => string.Equals(c, ofItem.Of, StringComparison.OrdinalIgnoreCase)))
                {
                    participant.Cases.Add(ofItem.Of);
                }
            }

            participant.Used = true;
        }
    }

    public void FillSequence(SequenceDto sequence)
    {
        var ofItem = of.FirstOrDefault(item => string.Equals(sequence.Case, item.Of, StringComparison.OrdinalIgnoreCase));

        if (ofItem == null)
        {
            return;
        }

        if (isMessage)
        {
            AddMessage(sequence, ofItem);
            AddOrUpdateParticipant(sequence, from);
            AddOrUpdateParticipant(sequence, to);
        }
        else if (isNote)
        {
            AddNote(sequence, ofItem);
            AddOrUpdateParticipant(sequence, over);
        }
    }

    private void AddMessage(SequenceDto sequence, OfItem ofItem)
    {
        sequence.Records.Add(new SequenceDto.RecordDto
        {
            IsMessage = true,
            From = from,
            To = to,
            Text = text,
            Order = ofItem.Order
        });
    }

    private void AddNote(SequenceDto sequence, OfItem ofItem)
    {
        sequence.Records.Add(new SequenceDto.RecordDto
        {
            IsNote = true,
            Over = over,
            Text = text,
            Order = ofItem.Order
        });
    }

    private void AddOrUpdateParticipant(SequenceDto sequence, string name)
    {
        var participant = sequence.Participants.FirstOrDefault(p => string.Equals(name, p.Name, StringComparison.OrdinalIgnoreCase));

        if (participant == null)
        {
            sequence.Participants.Add(new SequenceDto.ParticipantDto
            {
                Name = name,
                Used = true
            });
        }
        else
        {
            participant.Used = true;
        }
    }

    public override void Accept(ICommentVisitor visitor)
    {
        visitor.Visit(this);
    }

    public static bool TryCreate(ParsedComment value, out Message result)
    {
        result = null;

        if (!string.Equals(value.Is, Key, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!TryCreateOfList(value, out var ofList))
        {
            return false;
        }

        var isNote = !string.IsNullOrEmpty(value.Over) && !string.IsNullOrEmpty(value.Text);
        var isMessage = !string.IsNullOrEmpty(value.From) && !string.IsNullOrEmpty(value.To);

        if (!isNote && !isMessage)
        {
            return false;
        }

        result = new Message(ofList, value.From, value.To, value.Over, value.Text, isNote, isMessage);

        return true;
    }

    private static bool TryCreateOfList(ParsedComment value, out OfItem[] result)
    {
        result = null;
        
        var list = new List<OfItem>()
        {
            new OfItem(value.Of, value.Order)
        };

        if (value.OfList != null)
        {
            foreach (var item in value.OfList)
            {
                list.Add(new OfItem(item.Of, item.Order ?? value.Order));
            }
        }

        result = list
            .Where(item => !string.IsNullOrEmpty(item.Of) && item.Order.HasValue)
            .DistinctBy(item => item.Of)
            .ToArray();

        return result.Any();
    }

    private record OfItem(string Of, float? Order);
}