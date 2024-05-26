using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Introspector;

public static class IApplicationBuilderExtensions
{
    public static IApplicationBuilder UseIntrospector(this IApplicationBuilder appBuilder, Action<IntrospectorOptions> options = null)
    {
        var opt = new IntrospectorOptions();

        options?.Invoke(opt);

        var comments = GetComments().ToList();
        var casesListPresenter = CasesListPresenter.Create(comments);
        var componentsPresenter = new ComponentsPresenter(comments);
        var sequencePresenter = new SequencePresenter(comments);

        appBuilder.Map($"{opt.BasePath}/cases", builder =>
        {
            builder.Run(async context =>
            {
                await casesListPresenter.Write(context);
            });
        });

        appBuilder.Map($"{opt.BasePath}/sequence", builder =>
        {
            builder.Run(async context =>
            {
                var caseName = GetStringQueryParam(context, "case");
                var scale = GetFloatQueryParam(context, "scale");

                if (FindCase(comments, caseName))
                {
                    await sequencePresenter.Write(context, new SequenceDto
                    {
                        Case = caseName,
                        Scale = scale
                    });
                }
                else
                {
                    context.Response.StatusCode = 404;
                }
            });
        });

        appBuilder.Map($"{opt.BasePath}/components", builder =>
        {
            builder.Run(async context =>
            {
                var caseName = GetStringQueryParam(context, "case");
                var scale = GetFloatQueryParam(context, "scale");

                await componentsPresenter.Write(context, new ComponentsDto
                {
                    Scale = scale,
                    Case = caseName
                });
            });
        });

        return appBuilder;
    }

    private static IEnumerable<Comment> GetComments()
    {
        foreach (var value in CommentsCollector.Collect())
        {
            if (CommentFactory.TryCreate(value, out var result))
            {
                yield return result;
            }
        }
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

    private static bool FindCase(List<Comment> comments, string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }
        
        var visitor = new FindCaseVisitor(name);

        comments.ForEach(c => c.Accept(visitor));

        return visitor.Success;
    }

    private static bool EqualsAny(string value, params string[] values)
    {
        return values.Any(v => string.Equals(v, value, StringComparison.OrdinalIgnoreCase));
    }

    private class CasesListPresenter : CommentVisitor
    {
        private readonly List<CaseDto> cases = new();

        private CasesListPresenter()
        {
        }
        
        public override void Visit(Case value)
        {
            value.FillCases(cases);
        }

        public override void Visit(Message value)
        {
            value.FillCases(cases);
        }

        public async Task Write(HttpContext context)
        {
            await context.Response.WriteAsJsonAsync(cases, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }

        public static CasesListPresenter Create(List<Comment> comments)
        {
            var result = new CasesListPresenter();

            foreach (var comment in comments)
            {
                comment.Accept(result);
            }

            return result;
        }
    }

    private class SequenceCollector : CommentVisitor
    {
        private readonly SequenceDto sequence;

        public SequenceCollector(SequenceDto sequence)
        {
            this.sequence = sequence;
        }

        public override void Visit(Participant value)
        {
            value.FillSequence(sequence);
        }

        public override void Visit(Message value)
        {
            value.FillSequence(sequence);
        }
    }

    private class FindCaseVisitor : CommentVisitor
    {
        private readonly string name;

        public FindCaseVisitor(string name)
        {
            this.name = name;
        }

        public override void Visit(Case value)
        {
            if (value.HasName(name))
            {
                Success = true;
            }
        }

        public bool Success { get; private set;}
    }

    private class SequencePresenter
    {
        private readonly List<Comment> comments;

        public SequencePresenter(List<Comment> comments)
        {
            this.comments = comments;
        }

        public async Task Write(HttpContext context, SequenceDto sequence)
        {
            var collector = new SequenceCollector(sequence);

            comments.ForEach(comment => comment.Accept(collector));

            Filter(sequence);
            Scale(sequence);
            Sort(sequence);

            context.Response.ContentType = "text/plain; charset=utf-8";

            var builder = new StringBuilder().AppendLine("@startuml");

            foreach (var participant in sequence.Participants)
            {
                builder.AppendLine($@"{participant.Type ?? "participant"} ""{participant.Name}""");
            }

            foreach (var record in sequence.Records)
            {
                if (record.IsMessage)
                {
                    builder.AppendLine($@"""{record.From}"" -> ""{record.To}"" : {record.Text}");
                }

                if (record.IsNote)
                {
                    builder.AppendLine($@"note over ""{record.Over}"" : {record.Text}");
                }
            }

            await context.Response.WriteAsync(builder.AppendLine("@enduml").ToString());
        }

        private void Filter(SequenceDto sequence)
        {
            sequence.Participants.RemoveAll(p => !p.Used);
        }

        private void Scale(SequenceDto sequence)
        {
            foreach (var participant in sequence.Participants.Where(p => p.Scale > sequence.Scale))
            {
                sequence.Records.RemoveAll(r => EqualsAny(participant.Name, r.From, r.To, r.Over));
            }

            sequence.Participants.RemoveAll(p => p.Scale > sequence.Scale);
        }

        private void Sort(SequenceDto sequence)
        {
            var participantOrder = 0;

            sequence.Records.Sort((r1, r2) => Comparer<float?>.Default.Compare(r1.Order, r2.Order));

            sequence.Records.ForEach(r =>
            {
                SetParticipantOrder(sequence, r.From, ref participantOrder);
                SetParticipantOrder(sequence, r.To, ref participantOrder);
                SetParticipantOrder(sequence, r.Over, ref participantOrder);
            });

            sequence.Participants.Sort((p1, p2) => Comparer<float?>.Default.Compare(p1.Order, p2.Order));
        }

        private void SetParticipantOrder(SequenceDto sequence, string name, ref int currentOrder)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            var participant = sequence.Participants.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

            if (participant == null)
            {
                return;
            }

            if (!participant.Order.HasValue)
            {
                participant.Order = currentOrder++;
            }
        }
    }

    private class ComponentsCollector : CommentVisitor
    {
        private readonly ComponentsDto components;

        public ComponentsCollector(ComponentsDto components)
        {
            this.components = components;
        }

        public override void Visit(Participant value)
        {
            value.FillComponents(components);
        }

        public override void Visit(Message value)
        {
            value.FillComponents(components);
        }
    }

    private class ComponentsPresenter : CommentVisitor
    {
        private readonly List<Comment> comments;

        public ComponentsPresenter(List<Comment> comments)
        {
            this.comments = comments;
        }

        public async Task Write(HttpContext context, ComponentsDto components)
        {
            var collector = new ComponentsCollector(components);

            comments.ForEach(comment => comment.Accept(collector));

            Filter(components);
            Scale(components);

            context.Response.ContentType = "text/plain; charset=utf-8";

            var builder = new StringBuilder().AppendLine("@startuml");

            foreach (var participant in components.Participants)
            {
                builder.AppendLine($@"{participant.Type ?? "component"} ""{participant.Name}""");
            }

            foreach (var link in components.Links)
            {
                builder.AppendLine($@"""{link.From}"" -- ""{link.To}""");
            }

            await context.Response.WriteAsync(builder.AppendLine("@enduml").ToString());
        }

        private void Filter(ComponentsDto components)
        {
            components.Participants.RemoveAll(p => !p.Used);
        }

        private void Scale(ComponentsDto components)
        {
            foreach (var participant in components.Participants.Where(p => p.Scale > components.Scale))
            {
                components.Links.RemoveAll(r => EqualsAny(participant.Name, r.From, r.To));
            }

            components.Participants.RemoveAll(p => p.Scale > components.Scale);
        }
    }
}