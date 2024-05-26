namespace Introspector;

internal static class CommentFactory
{
    public static bool TryCreate(string value, out Comment comment)
    {
        comment = null;

        if (!CommentParser.TryParse(value, out var parsed))
        {
            return false;
        }

        if (Case.TryCreate(parsed, out var resultCase))
        {
            comment = resultCase;

            return true;
        }

        if (Message.TryCreate(parsed, out var resultMessage))
        {
            comment = resultMessage;

            return true;
        }

        if (Participant.TryCreate(parsed, out var resultParticipant))
        {
            comment = resultParticipant;

            return true;
        }

        return false;
    }
}