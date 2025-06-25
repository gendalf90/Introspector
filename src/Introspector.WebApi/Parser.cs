using System.Text.RegularExpressions;

namespace Introspector.WebApi;

internal static class Parser
{
    private const string Pattern = "usecase\\s*\"(?<useCaseName>[^\"]+)\"";

    public static IEnumerable<string> GetUseCaseNames(string useCases)
    {
        foreach (Match match in Regex.Matches(useCases, Pattern))
        {
            yield return match.Groups["useCaseName"].Value;
        }
    }
}