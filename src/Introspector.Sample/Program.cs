using Introspector;

var app = WebApplication.CreateBuilder(args).Build();

app.UseIntrospector();
app.Run("http://localhost:3000");
