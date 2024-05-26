using Introspector;
using Introspector.Sample;

var app = WebApplication.CreateBuilder(args).Build();

app.UseIntrospector();

app.MapGet("/test", () =>
{
    var results = new object[2];
    
    /*
        is: case
        name: use case 1
        text: info about case 1
    */
    var serviceOne = new ServiceOne();

    results[0] = serviceOne.GetResult();

    // { "is": "case", "name": "use case 2", "text": "info about case 2" }
    var serviceTwo = new ServiceTwo();

    results[1] = serviceTwo.GetResult();
    
    return results;
});

app.Run("http://localhost:51000");
