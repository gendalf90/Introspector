using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Introspector;

internal static class CommentParser
{
    private static readonly IDeserializer deserializer = new DeserializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public static bool TryParse(string value, out ParsedComment result)
    {
        result = null;

        try
        {
            result = deserializer.Deserialize<ParsedComment>(value);

            return result != null;
        }
        catch
        {
            return false;
        }
    }
}