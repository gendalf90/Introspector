using Introspector.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Introspector.Files;

namespace Introspector.Tests.Mcp;

public class McpServerFixture : IAsyncLifetime
{
    private WebApplication host;
    
    public HttpClient GetClient() => host.GetTestClient();

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Configuration.Sources.Clear();
        builder.Services
            .AddMcpServer()
            .WithHttpTransport(options =>
            {
                options.Stateless = true;
            })
            .WithIntrospector(b =>
            {
                b.LoadFiles(filePattern: "test_2.txt");
            });

        host = builder.Build();

        host.MapMcp();

        await host.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await using (host)
        {
            await host.StopAsync();
        }
    }
}