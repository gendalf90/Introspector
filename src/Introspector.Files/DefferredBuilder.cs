using Microsoft.Extensions.Logging;

namespace Introspector.Files;

internal class DefferredBuilder(ILogger logger) : IBuilder
{
    private readonly List<Action<IBuilder>> configurators = [];
    
    public void AddCall(string caseKey, string fromKey, string toKey, string text, float? order)
    {
        configurators.Add(builder => builder.AddCall(caseKey, fromKey, toKey, text, order));
    }

    public void AddCase(string key, string description)
    {
        configurators.Add(builder => builder.AddCase(key, description));
    }

    public void AddComment(string caseKey, string text, float? order, string overKey)
    {
        configurators.Add(builder => builder.AddComment(caseKey, text, order, overKey));
    }

    public void AddComponent(string key, string type, string description)
    {
        configurators.Add(builder => builder.AddComponent(key, type, description));
    }

    public void AddRef(string caseFromKey, string caseToKey, float? order)
    {
        configurators.Add(builder => builder.AddRef(caseFromKey, caseToKey, order));
    }

    public void Apply(IBuilder builder)
    {
        configurators.ForEach(configurator => 
        {
            try
            {
                configurator.Invoke(builder);
            }
            catch (Exception e)
            {
                logger?.LogError(e, "Error while configuring...");
            }
        });
    }
}