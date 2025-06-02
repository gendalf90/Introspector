using System.Reflection;
using System.Xml;

namespace Introspector.Xml;

public static class Extensions
{
    private static readonly string DefaultXmlDocFilePath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetEntryAssembly().GetName().Name}.xml");

    public static IBuilder ParseXml(this IBuilder builder, string xml)
    {
        var document = new XmlDocument();

        document.LoadXml(xml);

        ParseCases(builder, document);
        ParseComponents(builder, document);
        ParseCalls(builder, document);
        ParseComments(builder, document);

        return builder;
    }

    public static IBuilder ParseXmlDocFile(this IBuilder builder, string path = null)
    {
        var filePath = string.IsNullOrWhiteSpace(path)
            ? DefaultXmlDocFilePath
            : path;
        
        var document = new XmlDocument();

        document.Load(filePath);

        ParseMembers(builder, document);

        return builder;
    }

    private static void ParseMembers(IBuilder builder, XmlNode node)
    {
        var values = node.SelectNodes("/doc/members/member");

        if (values == null)
        {
            return;
        }

        foreach (XmlNode value in values)
        {
            ParseCases(builder, value);
            ParseComponents(builder, value);
            ParseCalls(builder, value);
            ParseComments(builder, value);
        }
    }

    private static void ParseCases(IBuilder builder, XmlNode node)
    {
        var values = node.SelectNodes("case");

        if (values == null)
        {
            return;
        }

        foreach (XmlNode value in values)
        {
            var name = value.SelectSingleNode("@name")?.Value;
            var text = value.SelectSingleNode("text()")?.Value;

            builder.AddCase(name, text?.TrimText());
        }
    }

    private static void ParseComponents(IBuilder builder, XmlNode node)
    {
        var values = node.SelectNodes("component");

        if (values == null)
        {
            return;
        }

        foreach (XmlNode value in values)
        {
            var name = value.SelectSingleNode("@name")?.Value;
            var type = value.SelectSingleNode("@type")?.Value;
            var text = value.SelectSingleNode("text()")?.Value;

            builder.AddComponent(name, type, text?.TrimText());
        }
    }

    private static void ParseCalls(IBuilder builder, XmlNode node)
    {
        var values = node.SelectNodes("call");

        if (values == null)
        {
            return;
        }

        foreach (XmlNode value in values)
        {
            builder.AddCall(callBuilder =>
            {
                AddCases(value.SelectNodes("case"), (name, order) => callBuilder.AddCase(name, order));
                AddComponents(value.SelectNodes("from"), name => callBuilder.AddFrom(name));
                AddComponents(value.SelectNodes("to"), name => callBuilder.AddTo(name));

                var text = value.SelectSingleNode("text/text()")?.Value;

                callBuilder.SetText(text?.TrimText());
            });
        }
    }

    private static void ParseComments(IBuilder builder, XmlNode node)
    {
        var values = node.SelectNodes("comment");

        if (values == null)
        {
            return;
        }

        foreach (XmlNode value in values)
        {
            builder.AddComment(commentBuilder =>
            {
                AddCases(value.SelectNodes("case"), (name, order) => commentBuilder.AddCase(name, order));
                AddComponents(value.SelectNodes("over"), name => commentBuilder.AddOver(name));

                var text = value.SelectSingleNode("text/text()")?.Value;

                commentBuilder.SetText(text?.TrimText());
            });
        }
    }

    private static void AddCases(XmlNodeList nodes, Action<string, float?> addCase)
    {
        if (nodes == null)
        {
            return;
        }

        foreach (XmlNode value in nodes)
        {
            var name = value.SelectSingleNode("@name")?.Value;

            float? order = float.TryParse(value.SelectSingleNode("@order")?.Value, out var parsedOrder)
                ? parsedOrder
                : null;

            addCase(name, order);
        }
    }

    private static void AddComponents(XmlNodeList nodes, Action<string> addComponent)
    {
        if (nodes == null)
        {
            return;
        }

        foreach (XmlNode value in nodes)
        {
            var name = value.SelectSingleNode("@name")?.Value;

            addComponent(name);
        }
    }

    public static string TrimText(this string text)
    {
        return string.Join('\n', text.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
    }
}
