using Introspector.Files;

namespace Introspector.Tests.Files;

public class FilesTests
{
    private readonly List<Case> cases = [];
    private readonly List<Component> components = [];
    private readonly List<Call> calls = [];
    private readonly List<Comment> comments = [];
    private readonly IVisitor reader;

    public FilesTests()
    {
        reader = Visitor.Create(cases.Add, components.Add, calls.Add, comments.Add);
    }
    
    [Fact]
    public void Load_JustCollectElements_ElementsAreExpected()
    {
        // Arrange
        // Act
        var results = Builder.Build(builder =>
        {
            builder.LoadFiles(filePattern: "test_1.txt");
        });

        foreach (var result in results)
        {
            result.Accept(reader);
        }

        // Assert
        var @case = Assert.Single(cases);

        Assert.True(@case.Key == "case_1" && @case.Description == "description_1\ndescription_1");
        Assert.Equal(2, components.Count);
        Assert.Contains(components, value => value.Key == "component_1" && value.Description == "description_2");
        Assert.Contains(components, value => value.Key == "component_2" && value.Description == null);

        var call = Assert.Single(calls);

        Assert.True(call.Case.Key == "case_1"
            && call.From.Key == "component_1"
            && call.To.Key == "component_2"
            && call.Text == "description_3\ndescription_3");

        var comment = Assert.Single(comments);

        Assert.True(comment.Case.Key == "case_1"
            && comment.Over.Key == "component_2"
            && comment.Text == "description_4");
    }
}
