using System.Text;
using System.Text.RegularExpressions;

namespace Introspector.Files;

internal class Cursor(IBuilder builder) : IDisposable
{
    protected readonly StringBuilder buffer = new();

    private IElement element;
    
    public void Append(string value)
    {
        if (TryCreate(value, out var result))
        {
            Flush();

            element = result;
            
            return;
        }
        
        if (element == null)
        {
            return;
        }

        buffer.AppendLine(value);
    }

    private bool TryCreate(string value, out IElement result)
    {
        result = null;

        if (ElementCall.TryCreate(value, out var call))
        {
            result = call;

            return true;
        }

        if (ElementCase.TryCreate(value, out var @case))
        {
            result = @case;

            return true;
        }

        if (ElementComponent.TryCreate(value, out var component))
        {
            result = component;

            return true;
        }

        if (ElementComment.TryCreate(value, out var comment))
        {
            result = comment;

            return true;
        }

        return false;
    }

    public void Flush()
    {
        if (element == null)
        {
            return;
        }

        var text = buffer.ToString().Trim();

        element.Build(builder, text);
        buffer.Clear();

        element = null;
    }
    
    public void Dispose()
    {
        Flush();
    }

    private static IEnumerable<(string name, float order)> ParseCases(string value)
    {
        foreach (var pair in value.Split(','))
        {
            var values = pair.Trim().Split(':');

            yield return (values[0], float.Parse(values[1]));
        }
    }

    private interface IElement
    {
        public abstract void Build(IBuilder builder, string text);
    }

    private class ElementCall : IElement
    {
        private static readonly Regex Pattern = new(@"call\((?<pairs>(?:\w+:\d+(?:\.\d+)?(?:,\s*)?)+)\):\s*(?<from>\w+)\s*->\s*(?<to>\w+)", RegexOptions.Compiled);

        private IEnumerable<(string name, float order)> cases = [];
        private string from;
        private string to;

        public void Build(IBuilder builder, string text)
        {
            foreach (var (name, order) in cases)
            {
                builder.AddCall(name, from, to, text, order);
            }
        }

        public static bool TryCreate(string value, out ElementCall result)
        {
            result = null;
            
            var match = Pattern.Match(value);

            if (!match.Success)
            {
                return false;
            }

            result = new ElementCall
            {
                cases = ParseCases(match.Groups["pairs"].Value),
                from = match.Groups["from"].Value,
                to = match.Groups["to"].Value
            };

            return true;
        }
    }

    private class ElementCase : IElement
    {
        private static readonly Regex Pattern = new(@"case:\s+(\w+)\s*$", RegexOptions.Compiled);

        private string key;

        public void Build(IBuilder builder, string text)
        {
            builder.AddCase(key, text);
        }

        public static bool TryCreate(string value, out ElementCase result)
        {
            result = null;
            
            var match = Pattern.Match(value);

            if (!match.Success)
            {
                return false;
            }

            result = new ElementCase
            {
                key = match.Groups[1].Value
            };

            return true;
        }
    }

    private class ElementComponent : IElement
    {
        private static readonly Regex Pattern = new(@"component:\s+(\w+)\s*$", RegexOptions.Compiled);

        private string name;

        public void Build(IBuilder builder, string text)
        {
            builder.AddComponent(name, text);
        }

        public static bool TryCreate(string value, out ElementComponent result)
        {
            result = null;
            
            var match = Pattern.Match(value);

            if (!match.Success)
            {
                return false;
            }

            result = new ElementComponent
            {
                name = match.Groups[1].Value
            };

            return true;
        }
    }

    private class ElementComment : IElement
    {
        private static readonly Regex Pattern = new(@"comment\((?<cases>(?:[^:]+:[^,\s]+(?:,\s*)?)+)\):\s*(?<component>\S*)\s*$", RegexOptions.Compiled);

        private IEnumerable<(string name, float order)> cases = [];
        private string over;

        public void Build(IBuilder builder, string text)
        {
            foreach (var (name, order) in cases)
            {
                builder.AddComment(name, text, order, over);
            }
        }

        public static bool TryCreate(string value, out ElementComment result)
        {
            result = null;
            
            var match = Pattern.Match(value);

            if (!match.Success)
            {
                return false;
            }

            result = new ElementComment
            {
                cases = ParseCases(match.Groups["cases"].Value),
                over = match.Groups["component"].Value
            };

            return true;
        }
    }
}
