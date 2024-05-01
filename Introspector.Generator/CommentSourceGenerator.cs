using System.Text;
using Microsoft.CodeAnalysis;

namespace Introspector.Generator;

[Generator]
internal class CommentSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, (ctx, source) =>
        {
            var comments = new List<string>();
            var walker = new CommentsSyntaxWalker(comments);

            foreach (var tree in source.SyntaxTrees)
            {
                walker.Visit(tree.GetRoot());
            }

            ctx.AddSource("CommentSource.g.cs", @$"
using System.Text;
using System;
using System.Collections.Generic;

namespace Introspector_{Guid.NewGuid():N}
{{
    public static class GeneratedCommentSource
    {{
        public static IEnumerable<string> GetList()
        {{
            {GenerateCommentsList(comments)}
        }}
    }}
}}
            ");
        });
    }

    private string GenerateCommentsList(List<string> comments)
    {
        var result = new StringBuilder();

        foreach (var comment in comments)
        {
            result.AppendLine(@$"yield return Encoding.UTF8.GetString(Convert.FromBase64String(""{comment}""));");
        }

        return result.AppendLine("yield break;").ToString();
    }
}
