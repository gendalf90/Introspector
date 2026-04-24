using Microsoft.Extensions.DependencyInjection;

namespace Introspector.Mcp;

public static class McpExtensions
{
    public static IMcpServerBuilder WithIntrospector(this IMcpServerBuilder builder, Action<IBuilder> configuration)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configuration);

        var elements = Builder.Build(configuration);

        return builder.WithTools(new ElementsTool([.. elements]));
    }
}
