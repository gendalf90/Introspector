using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Introspector;

public static class IApplicationBuilderExtensions
{
    public static IApplicationBuilder UseIntrospector(this IApplicationBuilder builder, Action<IntrospectorOptions> options = null)
    {
        var opt = new IntrospectorOptions();

        options?.Invoke(opt);

        var comments = CommentsCollector.Collect().ToList();
        var parsed = CommentsParser.Parse(comments).ToList();
        var cases = GetCases(parsed);
        var serializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        builder.Map($"{opt.BasePath}/cases", casesBuilder =>
        {
            casesBuilder.Run(async context =>
            {
                var caseName = context.Request.Query["name"].FirstOrDefault();

                if (string.IsNullOrEmpty(caseName))
                {
                    await context.Response.WriteAsJsonAsync(cases.Values, serializerOptions);
                }
                else if (cases.ContainsKey(caseName))
                {
                    await WriteCase(context, parsed, caseName);
                }
                else
                {
                    context.Response.StatusCode = 404;
                }
            });
        });

        return builder;
    }

    private static IDictionary<string, CaseDto> GetCases(IEnumerable<ParsedComment> comments)
    {
        var results = new Dictionary<string, CaseDto>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var comment in comments)
        {
            if (comment.IsCase())
            {
                results[comment.Name] = new CaseDto(comment.Name, comment.Note);
            }

            if (comment.IsMessage())
            {
                results.TryAdd(comment.Of, new CaseDto(comment.Of, null));
            }
        }

        return results;
    }

    private static async Task WriteCase(HttpContext context, IEnumerable<ParsedComment> comments, string caseName)
    {
        context.Response.ContentType = "text/plain; charset=utf-8";

        await context.Response.WriteAsync(CreateCase(comments, caseName));
    }

    private static string CreateCase(IEnumerable<ParsedComment> comments, string caseName)
    {
        var result = new StringBuilder().AppendLine("@startuml");

        var caseMassages = GetMessagesOfCase(comments, caseName).OrderBy(comment => comment.Order).ToList();
        var caseParticipants = GetParticipantsOfCase(comments, caseMassages).ToList();

        foreach (var comment in caseParticipants)
        {
            result.AppendLine($"{comment.GetParticipantTypeOrDefault()} {comment.Name}");
        }

        foreach (var comment in caseMassages)
        {
            result.AppendLine($"{comment.From} -> {comment.To} : {comment.Note}");
        }

        return result.AppendLine("@enduml").ToString();
    }

    private static IEnumerable<ParsedComment> GetParticipantsOfCase(
        IEnumerable<ParsedComment> allComments,
        IEnumerable<ParsedComment> caseMessages)
    {
        var results = new Dictionary<string, ParsedComment>(StringComparer.OrdinalIgnoreCase);
        var order = 0;

        foreach (var comment in caseMessages)
        {
            results.TryAdd(comment.From, new ParsedComment(Is: "participant", Name: comment.From, Order: order++));
            results.TryAdd(comment.To, new ParsedComment(Is: "participant", Name: comment.To, Order: order++));
        }

        foreach (var comment in allComments)
        {
            if (comment.IsParticipant() && results.TryGetValue(comment.Name, out var participant))
            {
                results[comment.Name] = participant with { Type = comment.GetParticipantTypeOrDefault() };
            }
        }

        return results.Values.OrderBy(comment => comment.Order);
    }

    private static IEnumerable<ParsedComment> GetMessagesOfCase(IEnumerable<ParsedComment> comments, string caseName)
    {
        foreach (var comment in comments)
        {
            if (comment.IsMessage() && comment.BelongsToCase(caseName))
            {
                yield return comment;
            }
        }
    }
}