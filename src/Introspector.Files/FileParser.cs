using Microsoft.Extensions.Logging;

namespace Introspector.Files;

public static class FileParser
{
    public static IEnumerable<Element> Parse(
        string rootDirectory, 
        string filePattern = "*",
        string stringPrefix = "@>",
        ILogger logger = null,
        CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(rootDirectory);
        ArgumentException.ThrowIfNullOrEmpty(filePattern);
        ArgumentNullException.ThrowIfNull(stringPrefix);
        
        var directory = new DirectoryInfo(rootDirectory);

        var builders = directory
            .EnumerateFiles(filePattern, SearchOption.AllDirectories)
            .AsParallel()
            .WithCancellation(token)
            .Select(file => ProcessFile(file, logger, stringPrefix))
            .ToList();

        return Builder.Build(builder =>
        {
            builders.ForEach(defferred => defferred.Apply(builder));
        });
    }

    private static DefferredBuilder ProcessFile(FileInfo file, ILogger logger, string stringPrefix)
    {
        var builder = new DefferredBuilder(logger);

        using var cursor = new Cursor(builder);

        foreach (var line in ReadLines(file))
        {
            if (TryGetPrefixSubstring(line, stringPrefix, out var result))
            {
                cursor.Append(result);
            }
        }

        return builder;
    }

    private static IEnumerable<string> ReadLines(FileInfo file)
    {
        using var fs = file.OpenRead();
        using var sr = new StreamReader(fs);

        while (true)
        {
            var line = sr.ReadLine();

            if (line == null)
            {
                yield break;
            }
            
            yield return line;
        }
    }

    private static bool TryGetPrefixSubstring(string line, string prefix, out string result)
    {
        result = null;
        
        var index = line.IndexOf(prefix);

        if (index < 0)
        {
            return false;
        }

        result = line.Substring(index + prefix.Length);

        return true;
    }
}
