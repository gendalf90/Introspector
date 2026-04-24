using System.ComponentModel;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace Introspector.Mcp;

internal class ElementTools(List<Element> elements)
{
    private readonly JsonSerializerOptions options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    [McpServerTool]
    [Description("Returns all available use cases list with description.")]
    public string GetAllCases()
    {
        var cases = new List<Case>();
        var visitor = Visitor.Create(onCase: cases.Add);

        elements.ForEach(value => value.Accept(visitor));

        return JsonSerializer.Serialize(cases.Select(value => new
        {
            value.Key,
            value.Description
        }), options);
    }

    [McpServerTool]
    [Description("Returns all available components (participants) list with description.")]
    public string GetAllComponents()
    {
        var components = new List<Component>();
        var visitor = Visitor.Create(onComponent: components.Add);

        elements.ForEach(value => value.Accept(visitor));

        return JsonSerializer.Serialize(components.Select(value => new
        {
            value.Key,
            value.Description
        }), options);
    }

    [McpServerTool] 
    [Description("Returns a sequence diagram of a specific use case by key.")]
    public string GetCaseSequence(string key)
    {
        var builder = new StringBuilder();
        var results = new List<(Component component, Comparer order, Call call, Comment comment)>();
        var visitor = Visitor.Create(
            onCall: value =>
            {
                if (string.Equals(value.Case.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add((value.From, value.Order, value, null));
                }
            },
            onComment: value =>
            {
                if (string.Equals(value.Case.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add((value.Over, value.Order, null, value));
                }
            }
        );

        elements.ForEach(value => value.Accept(visitor));

        if (!results.Any())
        {
            return null;
        }

        builder.AppendLine("sequenceDiagram");

        var components = results
            .Where(e => e.component != null)
            .OrderBy(e => e.order)
            .Select(e => e.component)
            .Distinct()
            .ToList();

        foreach (var component in components)
        {
            builder.AppendLine($"\tparticipant {component.Key}");
        }

        var callsAndComments = results
            .OrderBy(e => e.order)
            .Select(e => (e.call, e.comment))
            .ToList();

        var calls = callsAndComments
            .Where(e => e.call != null)
            .Select(e => e.call);

        foreach (var call in calls)
        {
            builder.AppendLine($"\t{call.From.Key}->>{call.To.Key}: {ReplaceNewLines(call.Text)}");
        }

        var comments = callsAndComments
            .Where(e => e.comment != null)
            .Select(e => e.comment);

        if (components.Any())
        {
            foreach (var comment in comments)
            {
                if (comment.Over != null)
                {
                    builder.AppendLine($"\tnote over {comment.Over.Key}: {ReplaceNewLines(comment.Text)}");
                }
                else
                {
                    builder.AppendLine($"\tnote over {components[0].Key}, {components[^1].Key}: {ReplaceNewLines(comment.Text)}");
                }
            }
        }
        else
        {
            builder.AppendLine($"\tparticipant _ as");

            foreach (var comment in comments)
            {
                builder.AppendLine($"\tnote over _: {ReplaceNewLines(comment.Text)}");
            }
        }

        return builder.ToString().Trim();
    }

    private string ReplaceNewLines(string value)
    {
        return value?.Trim().Replace(Environment.NewLine, "<br>");
    }
}
