using Introspector;
using Introspector.Sample;

var app = WebApplication.CreateBuilder(args).Build();

app.UseIntrospector();

app.MapGet("/test", () =>
{
    var results = new object[2];
    
    var serviceOne = new ServiceOne();

    results[0] = serviceOne.GetResult();

    var serviceTwo = new ServiceTwo();

    results[1] = serviceTwo.GetResult();
    
    return results;
});

app.Run("http://localhost:51000");
