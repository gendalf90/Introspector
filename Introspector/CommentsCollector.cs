using System.Reflection;

namespace Introspector;

internal static class CommentsCollector
{
    public static IEnumerable<string> Collect()
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            LoadReferencedAssembly(assembly);
        }

        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.FullName.EndsWith("GeneratedCommentSource"))
            .Select(type => type.GetMethod("GetList").Invoke(null, Array.Empty<object>()))
            .Cast<IEnumerable<string>>()
            .SelectMany(comment => comment);
    }
    
    private static void LoadReferencedAssembly(Assembly assembly)
    {
        foreach (AssemblyName name in assembly.GetReferencedAssemblies())
        {
            if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName == name.FullName))
            {
                LoadReferencedAssembly(Assembly.Load(name));
            }
        }
    }
}