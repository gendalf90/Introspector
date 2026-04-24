using Introspector.Files;
using Introspector.Mcp;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(opt =>
{
    opt.SingleLine = true;
    opt.UseUtcTimestamp = true;
    opt.IncludeScopes = true;
    opt.TimestampFormat = "[yyyy-MM-dd HH:mm:ss.fff] ";
});
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .WithIntrospector(b =>
    {
        b.LoadFiles
        (
            builder.Configuration.GetValue<string>("RootDirectory"),
            builder.Configuration.GetValue<string>("FilePattern"),
            builder.Configuration.GetValue<string>("StringPrefix")
        );
    });

var app = builder.Build();

app.MapMcp();
app.Run();
