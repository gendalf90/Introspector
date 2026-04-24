using System.Text.Json;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Introspector.Tests.Mcp;

public class McpTests : IClassFixture<McpServerFixture>
{
    private readonly HttpClient client;

    public McpTests(McpServerFixture fixture)
    {
        client = fixture.GetClient();
    }

    [Fact]
    public async Task GetTools_ResultsAreExpected()
    {
        // Arrange
        var options = new HttpClientTransportOptions
        {
            Endpoint = client.BaseAddress
        };
        
        var transport = new HttpClientTransport(options, client);
        
        await using var mcpClient = await McpClient.CreateAsync(transport);

        //Act
        var tools = await mcpClient.ListToolsAsync();

        //Assert
        Assert.Equal(3, tools.Count);
        Assert.Contains(tools, value => value.Name == "get_all_cases");
        Assert.Contains(tools, value => value.Name == "get_all_components");
        Assert.Contains(tools, value => value.Name == "get_case_sequence");
    }

    [Fact]
    public async Task GetAllCases_ResultsAreExpected()
    {
        // Arrange
        var options = new HttpClientTransportOptions
        {
            Endpoint = client.BaseAddress
        };
        
        var transport = new HttpClientTransport(options, client);
        
        await using var mcpClient = await McpClient.CreateAsync(transport);

        //Act
        var cases = await mcpClient.CallToolAsync("get_all_cases");

        //Assert
        var content = Assert.Single(cases.Content);
        var block = Assert.IsType<TextContentBlock>(content);
        var parsed = JsonSerializer.Deserialize<List<CaseOrComponentResult>>(block.Text, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var @case = Assert.Single(parsed);

        Assert.True(@case.Key == "case_1" && @case.Description == "description_1\ndescription_1");
    }

    [Fact]
    public async Task GetAllComponents_ResultsAreExpected()
    {
        // Arrange
        var options = new HttpClientTransportOptions
        {
            Endpoint = client.BaseAddress
        };
        
        var transport = new HttpClientTransport(options, client);
        
        await using var mcpClient = await McpClient.CreateAsync(transport);

        //Act
        var cases = await mcpClient.CallToolAsync("get_all_components");

        //Assert
        var content = Assert.Single(cases.Content);
        var block = Assert.IsType<TextContentBlock>(content);
        var parsed = JsonSerializer.Deserialize<List<CaseOrComponentResult>>(block.Text, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        Assert.Equal(2, parsed.Count);
        Assert.Contains(parsed, value => value.Key == "component_1" && value.Description == "description_2");
        Assert.Contains(parsed, value => value.Key == "component_2" && value.Description == null);
    }

    public record CaseOrComponentResult(string Key, string Description);

    [Fact]
    public async Task GetCaseSequence_ResultIsExpected()
    {
        // Arrange
        var options = new HttpClientTransportOptions
        {
            Endpoint = client.BaseAddress
        };
        
        var transport = new HttpClientTransport(options, client);
        
        await using var mcpClient = await McpClient.CreateAsync(transport);

        //Act
        var cases = await mcpClient.CallToolAsync("get_case_sequence", new Dictionary<string, object>
        {
            ["key"] = "case_1"
        });

        //Assert
        var content = Assert.Single(cases.Content);
        var block = Assert.IsType<TextContentBlock>(content);
        
        Assert.Equal("""
        sequenceDiagram
        	participant component_1
        	participant component_2
        	component_1->>component_2: description_3<br>description_3
        	note over component_2: description_4
        """, block.Text);
    }
}