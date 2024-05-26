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
            .Select(GetCommentsOrNull)
            .Where(comments => comments != null)
            .SelectMany(comments => comments);
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

    private static IEnumerable<string> GetCommentsOrNull(Type type)
    {
        if (!type.FullName.EndsWith("CommentSource"))
        {
            return null;
        }

        if (type.GetMethod("GetList") is not MethodInfo method)
        {
            return null;
        }

        if (!method.IsStatic || method.GetParameters().Any() || method.ReturnParameter.ParameterType != typeof(IEnumerable<string>))
        {
            return null;
        }

        return method.Invoke(null, Array.Empty<string>()) as IEnumerable<string>;
    }
}