
using Microsoft.AspNetCore.Builder;

namespace Introspector.Integration.Tests;

public class WebFixture : IDisposable
{
    private const string FileName = "Introspector.Integration.Tests.xml";
    private const string BaseAddress = "http://localhost:51000/";

    private readonly WebApplication application = WebApplication.CreateBuilder().Build();
    
    public WebFixture()
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri(BaseAddress)
        };
        
        application.UseIntrospector(opt => opt.XmlFilePaths = [FileName]);
        application.Urls.Add(BaseAddress);
        application.StartAsync().Wait();
    }

    public HttpClient Client { get; private set; }

    public void Dispose()
    {
        application.StopAsync().Wait();
    }
}