using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Introspector.WebApi;

public static class Extensions
{
    public static IServiceCollection AddIntrospector(this IServiceCollection services, string package, Action<IBuilder> configure)
    {
        return services.AddSingleton(Factory.Create(package, configure));
    }
    
    public static IApplicationBuilder UseIntrospector(this IApplicationBuilder appBuilder, Action<IntrospectorOptions> configure = null)
    {
        var options = new IntrospectorOptions();

        configure?.Invoke(options);

        appBuilder.Map($"{options.BasePath}/cases", builder =>
        {
            builder.Run(async context =>
            {
                var factory = context.RequestServices.GetService<IFactory>();

                if (factory == null)
                {
                    context.Response.StatusCode = 204;

                    return;
                }

                context.Response.ContentType = "text/plain; charset=utf-8";

                await context.Response.WriteAsync(factory.CreateUseCases());
            });
        });

        appBuilder.Map($"{options.BasePath}/sequence", builder =>
        {
            builder.Run(async context =>
            {
                var factory = context.RequestServices.GetService<IFactory>();

                if (factory == null)
                {
                    context.Response.StatusCode = 204;

                    return;
                }

                var caseName = context.Request.Query["case"].FirstOrDefault();

                if (string.IsNullOrWhiteSpace(caseName))
                {
                    context.Response.StatusCode = 404;

                    return;
                }

                var sequence = factory.CreateSequence(caseName);

                if (string.IsNullOrWhiteSpace(sequence))
                {
                    context.Response.StatusCode = 404;

                    return;
                }

                context.Response.ContentType = "text/plain; charset=utf-8";

                await context.Response.WriteAsync(sequence);
            });
        });

        appBuilder.Map($"{options.BasePath}/components", builder =>
        {
            builder.Run(async context =>
            {
                var factory = context.RequestServices.GetService<IFactory>();

                if (factory == null)
                {
                    context.Response.StatusCode = 204;

                    return;
                }

                var caseName = context.Request.Query["case"].FirstOrDefault();
                var components = string.Empty;

                if (string.IsNullOrWhiteSpace(caseName))
                {
                    components = factory.CreateAllComponents();
                }
                else
                {
                    components = factory.CreateComponents(caseName);
                }

                if (string.IsNullOrWhiteSpace(components))
                {
                    context.Response.StatusCode = 404;

                    return;
                }

                context.Response.ContentType = "text/plain; charset=utf-8";

                await context.Response.WriteAsync(components);
            });
        });

        return appBuilder;
    }
}