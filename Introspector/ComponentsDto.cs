namespace Introspector;

internal class ComponentsDto
{
    public List<ParticipantDto> Participants { get; set; } = new();
    
    public List<LinkDto> Links { get; set; } = new();

    public string Case { get; set; }

    public float? Scale { get; set; }
    
    public class LinkDto
    {
        public string From { get; set; }

        public string To { get; set;}
    }

    public class ParticipantDto
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public float? Scale { get; set; }

        public bool Used { get; set; }

        public List<string> Cases { get; set; } = new();
    }
}