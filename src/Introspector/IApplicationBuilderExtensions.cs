using System.Diagnostics;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Introspector;

public static class IApplicationBuilderExtensions
{
    public static IApplicationBuilder UseIntrospector(this IApplicationBuilder appBuilder, Action<IntrospectorOptions> options = null)
    {
        var opt = new IntrospectorOptions();

        options?.Invoke(opt);

        var elements = LoadElements(opt);

        appBuilder.Map($"{opt.BasePath}/cases", builder =>
        {
            builder.Run(async context =>
            {
                var casesPresenter = CasesPresenter.Create(elements);

                await casesPresenter.Write(context);
            });
        });

        appBuilder.Map($"{opt.BasePath}/sequence", builder =>
        {
            builder.Run(async context =>
            {
                var caseName = GetStringQueryParam(context, "case");
                var scale = GetFloatQueryParam(context, "scale");
                var presenter = SequencePresenter.Create(caseName, scale, elements);

                await presenter.Write(context);
            });
        });

        appBuilder.Map($"{opt.BasePath}/components", builder =>
        {
            builder.Run(async context =>
            {
                var caseName = GetStringQueryParam(context, "case");
                var scale = GetFloatQueryParam(context, "scale");
                var presenter = ComponentsPresenter.Create(caseName, scale, elements);

                await presenter.Write(context);
            });
        });

        return appBuilder;
    }

    private static IEnumerable<Element> LoadElements(IntrospectorOptions options)
    {
        if (options?.XmlFilePaths == null)
        {
            return Enumerable.Empty<Element>();
        }

        var results = new List<Element>();

        foreach (var path in options.XmlFilePaths)
        {
            var document = new XmlDocument();

            document.Load(path);

            Case.Parse(document, results);
            Component.Parse(document, results);
            Call.Parse(document, results);
            Comment.Parse(document, results);
        }

        Call.AddReferencedDependencies(results);
        Comment.AddReferencedDependencies(results);
        Call.CreateNotListedDependecies(results);
        Comment.CreateNotListedDependecies(results);
        Call.DeduplicateAndCleanDependecies(results);
        Comment.DeduplicateAndCleanDependecies(results);
        Case.Deduplicate(results);
        Component.Deduplicate(results);

        return results;
    }

    private static float? GetFloatQueryParam(HttpContext context, string name)
    {
        var value = context.Request.Query[name].FirstOrDefault();

        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        if (float.TryParse(value, out var result))
        {
            return result;
        }

        return null;
    }

    private static string GetStringQueryParam(HttpContext context, string name)
    {
        return context.Request.Query[name].FirstOrDefault();
    }

    private class CasesPresenter : IVisitor
    {
        private readonly List<Case> cases = new();

        private CasesPresenter()
        {    
        }

        public void Visit(Case value)
        {
            cases.Add(value);
        }

        public void Visit(Component value)
        {
        }

        public void Visit(Call value)
        {
        }

        public void Visit(Comment value)
        {
        }

        public async Task Write(HttpContext context)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var builder = new StringBuilder();

            builder.Append('[');

            WriteCases(builder);

            builder.AppendLine();
            builder.Append(']');

            await context.Response.WriteAsync(builder.ToString());
        }

        private void WriteCases(StringBuilder builder)
        {
            foreach (var first in cases.Take(1))
            {
                builder.AppendLine();
                first.WriteJson(builder);
            }

            foreach (var other in cases.Skip(1))
            {
                builder.Append(',');
                builder.AppendLine();
                other.WriteJson(builder);
            }
        }

        public static CasesPresenter Create(IEnumerable<Element> elements)
        {
            var result = new CasesPresenter();

            foreach (var element in elements)
            {
                element.Accept(result);
            }

            return result;
        }
    }

    private class SequencePresenter : IVisitor
    {
        private readonly string caseName;
        private readonly float? scale;

        private Case currentCase;
        private readonly List<Component> components = new();
        private readonly List<Call> calls = new();
        private readonly List<Comment> comments = new();

        private SequencePresenter(string caseName, float? scale)
        {
            this.caseName = caseName;
            this.scale = scale;
        }

        public void Visit(Case value)
        {
            if (value.HasName(caseName))
            {
                currentCase = value;
            }
        }

        public void Visit(Component value)
        {
            if (!value.IsFiltered(scale))
            {
                components.Add(value);
            }
        }

        public void Visit(Call value)
        {
            if (value.HasCase(caseName))
            {
                calls.Add(value);
            }
        }

        public void Visit(Comment value)
        {
            if (value.HasCase(caseName))
            {
                comments.Add(value);
            }
        }

        public async Task Write(HttpContext context)
        {
            if (currentCase == null)
            {
                context.Response.StatusCode = 404;

                return;
            }

            context.Response.ContentType = "text/plain; charset=utf-8";

            var builder = new StringBuilder().AppendLine("@startuml");

            currentCase.WriteSequenceTitle(builder);

            WriteComponents(builder);
            FilterCallsAndCommentsByComponents();
            WriteMessages(builder);

            await context.Response.WriteAsync(builder.AppendLine("@enduml").ToString());
        }

        private void FilterCallsAndCommentsByComponents()
        {
            calls.RemoveAll(call => !components.Any(component => call.ContainsTo(component)));
            calls.RemoveAll(call => !components.Any(component => call.ContainsFrom(component)));
            comments.RemoveAll(comment => !components.Any(component => comment.ContainsOver(component)));
        }

        private void WriteComponents(StringBuilder builder)
        {
            var written = new HashSet<Component>();
            var orderedCalls = calls
                .OrderBy(call => call.GetCaseOrder(caseName))
                .ToList();

            foreach (var call in orderedCalls)
            {
                var toWrite = components.Find(call.ContainsFrom);

                if (toWrite != null && written.Add(toWrite))
                {
                    toWrite.WriteToSequence(builder);
                }
            }

            foreach (var call in orderedCalls)
            {
                var toWrite = components.Find(call.ContainsTo);

                if (toWrite != null && written.Add(toWrite))
                {
                    toWrite.WriteToSequence(builder);
                }
            }

            foreach (var comment in comments)
            {
                var toWrite = components.Find(comment.ContainsOver);

                if (toWrite != null && written.Add(toWrite))
                {
                    toWrite.WriteToSequence(builder);
                }
            }
        }

        private void WriteMessages(StringBuilder builder)
        {
            var messages = new List<(Func<string, float?> GetOrder, Action<StringBuilder> Write)>();

            foreach (var call in calls)
            {
                messages.Add((call.GetCaseOrder, call.WriteToSequence));
            }

            foreach (var comment in comments)
            {
                messages.Add((comment.GetCaseOrder, comment.WriteToSequence));
            }

            foreach (var message in messages.OrderBy(message => message.GetOrder(caseName)))
            {
                message.Write(builder);
            }
        }

        public static SequencePresenter Create(string caseName, float? scale, IEnumerable<Element> elements)
        {
            var result = new SequencePresenter(caseName, scale);

            foreach (var element in elements)
            {
                element.Accept(result);
            }

            return result;
        }
    }

    private class ComponentsPresenter : IVisitor
    {
        private readonly string caseName;
        private readonly float? scale;

        private readonly List<Case> cases = new();
        private readonly List<Component> components = new();
        private readonly List<Call> calls = new();
        private readonly List<Comment> comments = new();

        private ComponentsPresenter(string caseName, float? scale)
        {
            this.caseName = caseName;
            this.scale = scale;
        }

        public void Visit(Case value)
        {
            if (!string.IsNullOrWhiteSpace(caseName) && !value.HasName(caseName))
            {
                return;
            }

            cases.Add(value);
        }

        public void Visit(Component value)
        {
            if (!value.IsFiltered(scale))
            {
                components.Add(value);
            }
        }

        public void Visit(Call value)
        {
            if (!string.IsNullOrWhiteSpace(caseName) && !value.HasCase(caseName))
            {
                return;
            }

            calls.Add(value);
        }

        public void Visit(Comment value)
        {
            if (!string.IsNullOrWhiteSpace(caseName) && !value.HasCase(caseName))
            {
                return;
            }

            comments.Add(value);
        }

        public async Task Write(HttpContext context)
        {
            if (cases.Count == 0)
            {
                context.Response.StatusCode = 404;

                return;
            }

            context.Response.ContentType = "text/plain; charset=utf-8";

            var builder = new StringBuilder().AppendLine("@startuml");

            if (IsWholeMap)
            {
                WriteAllComponents(builder);
            }
            else
            {
                WriteCaseComponents(builder);
            }

            FilterCallsAndCommentsByComponents();
            WriteCalls(builder);
            WriteComments(builder);

            await context.Response.WriteAsync(builder.AppendLine("@enduml").ToString());
        }

        private void WriteCaseComponents(StringBuilder builder)
        {
            var written = new HashSet<Component>();

            foreach (var call in calls)
            {
                var toWrite = components.Find(call.ContainsFrom);

                if (toWrite != null && written.Add(toWrite))
                {
                    toWrite.WriteToComponents(builder);
                }
            }

            foreach (var call in calls)
            {
                var toWrite = components.Find(call.ContainsTo);

                if (toWrite != null && written.Add(toWrite))
                {
                    toWrite.WriteToComponents(builder);
                }
            }

            foreach (var comment in comments)
            {
                var toWrite = components.Find(comment.ContainsOver);

                if (toWrite != null && written.Add(toWrite))
                {
                    toWrite.WriteToComponents(builder);
                }
            }
        }

        private void FilterCallsAndCommentsByComponents()
        {
            calls.RemoveAll(call => !components.Any(component => call.ContainsTo(component)));
            calls.RemoveAll(call => !components.Any(component => call.ContainsFrom(component)));
            comments.RemoveAll(comment => !components.Any(component => comment.ContainsOver(component)));
        }

        private void WriteCalls(StringBuilder builder)
        {
            foreach (var call in calls)
            {
                call.WriteToComponents(builder);
            }
        }

        private void WriteAllComponents(StringBuilder builder)
        {
            foreach (var component in components)
            {
                component.WriteToComponents(builder);
            }
        }

        private void WriteComments(StringBuilder builder)
        {
            foreach (var comment in comments)
            {
                comment.WriteToComponents(builder);
            }
        }

        private bool IsWholeMap => cases.Count > 1;

        public static ComponentsPresenter Create(string caseName, float? scale, IEnumerable<Element> elements)
        {
            var result = new ComponentsPresenter(caseName, scale);

            foreach (var element in elements)
            {
                element.Accept(result);
            }

            return result;
        }
    }
}