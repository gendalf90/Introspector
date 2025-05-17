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

        Assert.Equal(4, results.Length);
        Assert.Contains(results, @case => @case.Name == "use case one" && @case.Text == "info about case one");
        Assert.Contains(results, @case => @case.Name == "use case two" && @case.Text == null);
        Assert.Contains(results, @case => @case.Name == "ServiceOne" && @case.Text == "info about case of service one");
        Assert.Contains(results, @case => @case.Name == "use case three" && @case.Text == null);
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
            / note over "service one"
            "info about service one"
            end note
            participant "ServiceOne"
            participant "ServiceThree"
            / note over "ServiceThree"
            "info about ServiceThree"
            end note
            database "database"
            "service one" -> "ServiceThree" : "call service three"
            "ServiceOne" -> "ServiceThree" : "call service three"
            "ServiceThree" -> "database" : "call to database"
            note over "database"
            "processing request to database"
            end note
            "database" -> "ServiceThree" : "result from database"
            "ServiceThree" -> "service one" : "result of the call"
            "ServiceThree" -> "ServiceOne" : "result of the call"
            @enduml
            """, result.Trim('\n'));
    }

    [Fact]
    public async Task CheckSequenceOfCaseServiceOne()
    {
        var result = await client.GetStringAsync("/introspector/sequence?case=ServiceOne");

        Assert.Equal("""
            @startuml
            title
            "info about case of service one"
            end title
            participant "service one"
            / note over "service one"
            "info about service one"
            end note
            participant "ServiceOne"
            participant "ServiceThree"
            / note over "ServiceThree"
            "info about ServiceThree"
            end note
            database "database"
            "service one" -> "ServiceThree" : "call service three"
            "ServiceOne" -> "ServiceThree" : "call service three"
            "ServiceThree" -> "database" : "call to database"
            note over "database"
            "processing request to database"
            end note
            "database" -> "ServiceThree" : "result from database"
            "ServiceThree" -> "service one" : "result of the call"
            "ServiceThree" -> "ServiceOne" : "result of the call"
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
            participant "ServiceThree"
            / note over "ServiceThree"
            "info about ServiceThree"
            end note
            database "database"
            "service two" -> "ServiceThree" : "call service three"
            note over "ServiceThree"
            "processing request to service three"
            end note
            "ServiceThree" -> "database" : "call to database"
            note over "database"
            "processing request to database"
            end note
            "database" -> "ServiceThree" : "result from database"
            "ServiceThree" -> "service two" : "result of the call"
            @enduml
            """, result.Trim('\n'));
    }

    [Fact]
    public async Task CheckSequenceOfCaseThree()
    {
        var result = await client.GetStringAsync("/introspector/sequence?case=use%20case%20three");

        Assert.Equal("""
            @startuml
            participant "ServiceThree"
            / note over "ServiceThree"
            "info about ServiceThree"
            end note
            database "database"
            "ServiceThree" -> "database" : "call to database"
            note over "database"
            "processing request to database"
            end note
            "database" -> "ServiceThree" : "result from database"
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
            note right of ["service one"]
            "info about service one"
            end note
            ["ServiceOne"]
            ["ServiceThree"]
            note right of ["ServiceThree"]
            "info about ServiceThree"
            ----
            "processing request to service three"
            end note
            ["database"]
            note right of ["database"]
            "processing request to database"
            end note
            ["not called service"]
            ["service two"]
            ["ServiceThree"] --> ["service one"] : "result of the call"
            ["ServiceThree"] --> ["ServiceOne"] : "result of the call"
            ["service one"] --> ["ServiceThree"] : "call service three"
            ["ServiceOne"] --> ["ServiceThree"] : "call service three"
            ["ServiceThree"] --> ["service two"] : "result of the call"
            ["service two"] --> ["ServiceThree"] : "call service three"
            ["ServiceThree"] --> ["database"] : "call to database"
            ["database"] --> ["ServiceThree"] : "result from database"
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
            ["ServiceThree"]
            note right of ["ServiceThree"]
            "info about ServiceThree"
            end note
            ["service one"]
            note right of ["service one"]
            "info about service one"
            end note
            ["database"]
            note right of ["database"]
            "processing request to database"
            end note
            ["ServiceThree"] --> ["service one"] : "result of the call"
            ["ServiceThree"] --> ["ServiceOne"] : "result of the call"
            ["service one"] --> ["ServiceThree"] : "call service three"
            ["ServiceOne"] --> ["ServiceThree"] : "call service three"
            ["ServiceThree"] --> ["database"] : "call to database"
            ["database"] --> ["ServiceThree"] : "result from database"
            @enduml
            """, result.Trim('\n'));
    }

    [Fact]
    public async Task CheckComponentsOfCaseServiceOne()
    {
        var result = await client.GetStringAsync("/introspector/components?case=ServiceOne");

        Assert.Equal("""
            @startuml
            ["ServiceThree"]
            note right of ["ServiceThree"]
            "info about ServiceThree"
            end note
            ["service one"]
            note right of ["service one"]
            "info about service one"
            end note
            ["database"]
            note right of ["database"]
            "processing request to database"
            end note
            ["ServiceThree"] --> ["service one"] : "result of the call"
            ["ServiceThree"] --> ["ServiceOne"] : "result of the call"
            ["service one"] --> ["ServiceThree"] : "call service three"
            ["ServiceOne"] --> ["ServiceThree"] : "call service three"
            ["ServiceThree"] --> ["database"] : "call to database"
            ["database"] --> ["ServiceThree"] : "result from database"
            @enduml
            """, result.Trim('\n'));
    }

    [Fact]
    public async Task CheckComponentsOfCaseTwo()
    {
        var result = await client.GetStringAsync("/introspector/components?case=use%20case%20two");

        Assert.Equal("""
            @startuml
            ["ServiceThree"]
            note right of ["ServiceThree"]
            "info about ServiceThree"
            ----
            "processing request to service three"
            end note
            ["service two"]
            ["database"]
            note right of ["database"]
            "processing request to database"
            end note
            ["ServiceThree"] --> ["service two"] : "result of the call"
            ["service two"] --> ["ServiceThree"] : "call service three"
            ["ServiceThree"] --> ["database"] : "call to database"
            ["database"] --> ["ServiceThree"] : "result from database"
            @enduml
            """, result.Trim('\n'));
    }

    [Fact]
    public async Task CheckComponentsOfCaseThree()
    {
        var result = await client.GetStringAsync("/introspector/components?case=use%20case%20three");

        Assert.Equal("""
            @startuml
            ["ServiceThree"]
            note right of ["ServiceThree"]
            "info about ServiceThree"
            end note
            ["database"]
            note right of ["database"]
            "processing request to database"
            end note
            ["ServiceThree"] --> ["database"] : "call to database"
            ["database"] --> ["ServiceThree"] : "result from database"
            @enduml
            """, result.Trim('\n'));
    }
}