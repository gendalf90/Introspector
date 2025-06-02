using Introspector.WebApi;
using Introspector.Xml;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIntrospector("sample", builder => builder.ParseXmlDocFile());

var app = builder.Build();

app.UseIntrospector();
app.Run("http://localhost:3000");
