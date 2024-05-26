namespace Introspector;

internal class SequenceDto
{
    public List<ParticipantDto> Participants { get; set; } = new();

    public List<RecordDto> Records { get; set; } = new();

    public string Case { get; set; }

    public float? Scale { get; set; }
    
    public class ParticipantDto
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public float? Order { get; set; }

        public float? Scale { get; set; }

        public bool Used { get; set; }
    }

    public class RecordDto
    {
        public bool IsMessage { get; set; }

        public bool IsNote { get; set; }
        
        public string From { get; set; }

        public string To { get; set; }

        public string Text { get; set; }

        public string Type { get; set; }

        public string Over { get; set; }

        public float? Order { get; set; }
    }
}