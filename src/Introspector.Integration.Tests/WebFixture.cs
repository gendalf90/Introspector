
using Microsoft.AspNetCore.Builder;
using Introspector.WebApi;
using Introspector.Xml;

namespace Introspector.Integration.Tests;

public class WebFixture : IDisposable
{
    private const string FileName = "Introspector.Integration.Tests.xml";
    private const string BaseAddress = "http://localhost:51000/";

    private readonly WebApplication application;
    
    public WebFixture()
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri(BaseAddress)
        };

        var appBuilder = WebApplication.CreateBuilder();

        appBuilder.Services.AddIntrospector("tests", builder => builder.ParseXmlDocFile(FileName));

        application = appBuilder.Build();
        
        application.UseIntrospector();
        application.Urls.Add(BaseAddress);
        application.StartAsync().Wait();
    }

    public HttpClient Client { get; private set; }

    public void Dispose()
    {
        application.StopAsync().Wait();
    }
}