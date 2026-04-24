namespace Introspector.Tests;

public class BuilderTests
{
    private readonly List<Case> cases = [];
    private readonly List<Component> components = [];
    private readonly List<Call> calls = [];
    private readonly List<Comment> comments = [];
    private readonly IVisitor reader;

    public BuilderTests()
    {
        reader = Visitor.Create(cases.Add, components.Add, calls.Add, comments.Add);
    }
    
    [Fact]
    public void Build_JustAddElements_ElementsAreExpected()
    {
        // Arrange
        // Act
        var results = Builder.Build(builder =>
        {
            builder.AddCase("case_1", "description_1");
            builder.AddCase("case_2");
            builder.AddComponent("component_1", "description_2");
            builder.AddComponent("component_2");
            builder.AddCall("case_1", "component_1", "component_2", "description_3", 1);
            builder.AddCall("case_2", "component_1", "component_2");
            builder.AddComment("case_1", "description_4", 2, "component_2");
            builder.AddComment("case_2");
        });

        foreach (var result in results)
        {
            result.Accept(reader);
        }

        // Assert
        Assert.Equal(2, cases.Count);
        Assert.Equal(2, components.Count);
        Assert.Equal(2, calls.Count);
        Assert.Equal(2, comments.Count);
        Assert.Contains(cases, value => value.Key == "case_1" && value.Description == "description_1");
        Assert.Contains(cases, value => value.Key == "case_2" && value.Description == null);
        Assert.Contains(components, value => value.Key == "component_1" && value.Description == "description_2");
        Assert.Contains(components, value => value.Key == "component_2" && value.Description == null);
        Assert.Contains(calls, value => 
            value.Case.Key == "case_1" 
            && value.From.Key == "component_1"
            && value.To.Key == "component_2"
            && value.Text == "description_3");
        Assert.Contains(calls, value => 
            value.Case.Key == "case_2" 
            && value.From.Key == "component_1"
            && value.To.Key == "component_2"
            && value.Text == null);
        Assert.Contains(comments, value => 
            value.Case.Key == "case_1" 
            && value.Over.Key == "component_2"
            && value.Text == "description_4");
        Assert.Contains(comments, value => 
            value.Case.Key == "case_2" 
            && value.Over == null
            && value.Text == null);
    }

    [Fact]
    public void Build_JustAddOnlyCallsAndComments_CasesAndComponentsAreCreated()
    {
        // Arrange
        // Act
        var results = Builder.Build(builder =>
        {
            builder.AddCall("case_1", "component_1", "component_2", "description_1", 1);
            builder.AddComment("case_2", "description_2", 2, "component_3");
        });

        foreach (var result in results)
        {
            result.Accept(reader);
        }

        // Assert
        Assert.Equal(2, cases.Count);
        Assert.Equal(3, components.Count);
        Assert.Contains(cases, value => value.Key == "case_1" && value.Description == null);
        Assert.Contains(cases, value => value.Key == "case_2" && value.Description == null);
        Assert.Contains(components, value => value.Key == "component_1" && value.Description == null);
        Assert.Contains(components, value => value.Key == "component_2" && value.Description == null);
        Assert.Contains(components, value => value.Key == "component_3" && value.Description == null);
    }
}
