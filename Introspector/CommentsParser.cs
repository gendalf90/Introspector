using System.Text.Json;

namespace Introspector;

internal static class CommentsParser
{
    public static IEnumerable<ParsedComment> Parse(IEnumerable<string> comments)
    {
        foreach (var comment in comments)
        {
            if (TryParse(comment, out var result) && result.IsValidComment())
            {
                yield return result;
            }
        }
    }

    private static bool TryParse(string value, out ParsedComment result)
    {
        result = null;
        
        try
        {
            result = JsonSerializer.Deserialize<ParsedComment>(value);

            return true;
        }
        catch
        {
            return false;
        }
    }
}