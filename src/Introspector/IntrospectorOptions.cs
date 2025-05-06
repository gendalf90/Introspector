using System.Reflection;

namespace Introspector;

public class IntrospectorOptions
{
    public string BasePath { get; set; } = "/introspector";

    public string[] XmlFilePaths { get; set; } = [Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetEntryAssembly().GetName().Name}.xml")];
}