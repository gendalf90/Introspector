namespace Introspector;

internal static class Extensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
    {
        return list == null || !list.Any();
    }

    public static IEnumerable<string> NotEmpty(this IEnumerable<string> list)
    {
        return list.Where(str => !string.IsNullOrWhiteSpace(str));
    }

    public static string JoinLines(this string text)
    {
        return string.Join("\\n", text.Split(Environment.NewLine, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
    }
}