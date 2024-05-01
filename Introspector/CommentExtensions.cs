namespace Introspector;

internal static class CommentExtensions
{
    private static string[] ValidParticipantTypes = ["participant", "actor", "database", "queue"];
    private static string ParticipantDefaultType = "participant";


    public static bool IsParticipant(this ParsedComment comment)
    {
        return IsEqual(comment.Is, "participant");
    }

    public static bool IsMessage(this ParsedComment comment)
    {
        return IsEqual(comment.Is, "message");
    }

    public static bool IsCase(this ParsedComment comment)
    {
        return IsEqual(comment.Is, "case");
    }

    public static string GetParticipantTypeOrDefault(this ParsedComment comment)
    {
        return string.IsNullOrEmpty(comment.Type) ? ParticipantDefaultType : comment.Type;
    }

    public static bool BelongsToCase(this ParsedComment comment, string caseName)
    {
        return string.Equals(comment.Of, caseName, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsValidParticipantType(this ParsedComment comment)
    {
        return ValidParticipantTypes.Contains(comment.GetParticipantTypeOrDefault(), StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsValidParticipant(this ParsedComment comment)
    {
        return comment.IsParticipant() && comment.IsValidParticipantType();
    }

    public static bool IsValidCase(this ParsedComment comment)
    {
        return comment.IsCase();
    }

    public static bool IsValidMessage(this ParsedComment comment)
    {
        return comment.IsMessage() 
            && IsFilled(comment.Of)
            && IsFilled(comment.From)
            && IsFilled(comment.To);
    }

    public static bool IsValidComment(this ParsedComment comment)
    {
        return comment.IsValidParticipant() || comment.IsValidCase() || comment.IsValidMessage();
    }

    private static bool IsEqual(string first, string second)
    {
        return string.Equals(first, second, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFilled(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
}