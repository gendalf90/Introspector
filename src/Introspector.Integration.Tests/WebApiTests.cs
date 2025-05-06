using System.Net;
using System.Net.Http.Json;

namespace Introspector.Integration.Tests;

public class CaseDto
{
    public string Name { get; set; }

    public string Text { get; set; }
}

public class WebApiTests : IClassFixture<WebFixture>
{
    private readonly HttpClient client;

    public WebApiTests(WebFixture fixture)
    {
        client = fixture.Client;
    }

    [Fact]
    public async Task CheckAllCases()
    {
        var results = await client.GetFromJsonAsync<CaseDto[]>("/introspector/cases");

        Assert.Equal(2, results.Length);
        Assert.Contains(results, @case => @case.Name == "use case one" && @case.Text == "info about case one");
        Assert.Contains(results, @case => @case.Name == "use case two" && @case.Text == null);
    }

    [Fact]
    public async Task CheckSequenceOfUnknownCase()
    {
        var result = await client.GetAsync("/introspector/sequence?case=unknown");

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task CheckSequenceOfCaseOne()
    {
        var result = await client.GetStringAsync("/introspector/sequence?case=use%20case%20one");

        Assert.Equal("""
            @startuml
            title
            "info about case one"
            end title
            participant "service one"
            participant "service three"
            database "database"
            "service one" -> "service three" : "call service three"
            "service three" -> "database" : "call to database"
            note over "database" : "processing request to database"
            "database" -> "service three" : "result from database"
            "service three" -> "service one" : "result of the call"
            @enduml
            """, result.Trim('\n'));
    }

    [Fact]
    public async Task CheckSequenceOfCaseOneWithScale2()
    {
        var result = await client.GetStringAsync("/introspector/sequence?case=use%20case%20one&scale=2.0");

        Assert.Equal("""
            @startuml
            title
            "info about case one"
            end title
            participant "service one"
            participant "service three"
            "service one" -> "service three" : "call service three"
            "service three" -> "service one" : "result of the call"
            @enduml
            """, result.Trim('\n'));
    }

    [Fact]
    public async Task CheckSequenceOfCaseOneWithScale1()
    {
        var result = await client.GetStringAsync("/introspector/sequence?case=use%20case%20one&scale=1.0");

        Assert.Equal("""
            @startuml
            title
            "info about case one"
            end title
            participant "service one"
            @enduml
            """, result.Trim('\n'));
    }

    [Fact]
    public async Task CheckSequenceOfCaseTwo()
    {
        var result = await client.GetStringAsync("/introspector/sequence?case=use%20case%20two");

        Assert.Equal("""
            @startuml
            participant "service two"
            participant "service three"
            database "database"
            "service two" -> "service three" : "call service three"
            "service three" -> "database" : "call to database"
            note over "database" : "processing request to database"
            "database" -> "service three" : "result from database"
            "service three" -> "service two" : "result of the call"
            @enduml
            """, result.Trim('\n'));
    }

    [Fact]
    public async Task CheckAllComponents()
    {
        var result = await client.GetStringAsync("/introspector/components");

        Assert.Equal("""
            @startuml
            ["service one"]
            ["service three"]
            ["database"]
            ["not called service"]
            ["service two"]
            ["service three"] --> ["service one"] : "result of the call"
            ["service one"] --> ["service three"] : "call service three"
            ["service three"] --> ["service two"] : "result of the call"
            ["service two"] --> ["service three"] : "call service three"
            ["service three"] --> ["database"] : "call to database"
            ["database"] --> ["service three"] : "result from database"
            note right of ["database"] : "processing request to database"
            @enduml
            """, result.Trim('\n'));
    }

    [Fact]
    public async Task CheckAllComponentsWithScale2()
    {
        var result = await client.GetStringAsync("/introspector/components?scale=2.0");

        Assert.Equal("""
            @startuml
            ["service one"]
            ["service three"]
            ["not called service"]
            ["service two"]
            ["service three"] --> ["service one"] : "result of the call"
            ["service one"] --> ["service three"] : "call service three"
            ["service three"] --> ["service two"] : "result of the call"
            ["service two"] --> ["service three"] : "call service three"
            @enduml
            """, result.Trim('\n'));
    }

    [Fact]
    public async Task CheckAllComponentsWithScale1()
    {
        var result = await client.GetStringAsync("/introspector/components?scale=1.0");

        Assert.Equal("""
            @startuml
            ["service one"]
            ["not called service"]
            ["service two"]
            @enduml
            """, result.Trim('\n'));
    }

    [Fact]
    public async Task CheckComponentsOfUnknownCase()
    {
        var result = await client.GetAsync("/introspector/components?case=unknown");

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task CheckComponentsOfCaseOne()
    {
        var result = await client.GetStringAsync("/introspector/components?case=use%20case%20one");

        Assert.Equal("""
            @startuml
            ["service three"]
            ["service one"]
            ["database"]
            ["service three"] --> ["service one"] : "result of the call"
            ["service one"] --> ["service three"] : "call service three"
            ["service three"] --> ["database"] : "call to database"
            ["database"] --> ["service three"] : "result from database"
            note right of ["database"] : "processing request to database"
            @enduml
            """, result.Trim('\n'));
    }

    [Fact]
    public async Task CheckComponentsOfCaseOneWithScale2()
    {
        var result = await client.GetStringAsync("/introspector/components?case=use%20case%20one&scale=2.0");

        Assert.Equal("""
            @startuml
            ["service three"]
            ["service one"]
            ["service three"] --> ["service one"] : "result of the call"
            ["service one"] --> ["service three"] : "call service three"
            @enduml
            """, result.Trim('\n'));
    }

    [Fact]
    public async Task CheckComponentsOfCaseOneWithScale1()
    {
        var result = await client.GetStringAsync("/introspector/components?case=use%20case%20one&scale=1.0");

        Assert.Equal("""
            @startuml
            ["service one"]
            @enduml
            """, result.Trim('\n'));
    }

    [Fact]
    public async Task CheckComponentsOfCaseTwo()
    {
        var result = await client.GetStringAsync("/introspector/components?case=use%20case%20two");

        Assert.Equal("""
            @startuml
            ["service three"]
            ["service two"]
            ["database"]
            ["service three"] --> ["service two"] : "result of the call"
            ["service two"] --> ["service three"] : "call service three"
            ["service three"] --> ["database"] : "call to database"
            ["database"] --> ["service three"] : "result from database"
            note right of ["database"] : "processing request to database"
            @enduml
            """, result.Trim('\n'));
    }
}