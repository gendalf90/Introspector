using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Introspector.Generator;

internal class CommentsSyntaxWalker : CSharpSyntaxWalker
{
    private readonly List<string> comments;

    public CommentsSyntaxWalker(List<string> comments) : base(SyntaxWalkerDepth.Trivia)
    {
        this.comments = comments;
    }

    public override void VisitTrivia(SyntaxTrivia trivia)
    {
        if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
        {
            var comment = trivia.ToString();
            
            comments.Add(ToBase64(comment.Substring(2)));
        }

        if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
        {
            var comment = trivia.ToString();

            comments.Add(ToBase64(comment.Substring(2, comment.Length - 4)));
        }

        base.VisitTrivia(trivia);
    }

    private string ToBase64(string str)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
    }
}
