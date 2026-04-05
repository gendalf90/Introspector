namespace Introspector;

public interface IBuilder
{
    void AddCase(string key, string description = null);

    void AddComponent(string key, string type, string description = null);

    void AddCall(string caseKey, string fromKey, string toKey, string text = null, float? order = default);

    void AddRef(string caseFromKey, string caseToKey, float? order = default);

    void AddComment(string caseKey, string text = null, float? order = default, string overKey = null);
}
