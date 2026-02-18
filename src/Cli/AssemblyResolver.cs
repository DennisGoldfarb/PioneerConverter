using System.Reflection;
using System.Runtime.Loader;

namespace PioneerConverter.Cli;

internal static class AssemblyResolver
{
    private static int _isRegistered;

    public static void Register()
    {
        if (Interlocked.Exchange(ref _isRegistered, 1) == 1)
        {
            return;
        }

        AssemblyLoadContext.Default.Resolving += ResolveManagedAssembly;
    }

    private static Assembly? ResolveManagedAssembly(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        if (string.IsNullOrWhiteSpace(assemblyName.Name))
        {
            return null;
        }

        string fileName = assemblyName.Name + ".dll";
        string baseDirectory = AppContext.BaseDirectory;

        string[] directCandidates =
        {
            Path.Combine(baseDirectory, fileName),
            Path.Combine(baseDirectory, "lib", fileName),
            Path.Combine(baseDirectory, "lib", "NetCore", fileName)
        };

        foreach (string candidate in directCandidates)
        {
            if (File.Exists(candidate))
            {
                return context.LoadFromAssemblyPath(candidate);
            }
        }

        string libDirectory = Path.Combine(baseDirectory, "lib");
        if (!Directory.Exists(libDirectory))
        {
            return null;
        }

        string[] matches = Directory.GetFiles(libDirectory, fileName, SearchOption.AllDirectories);
        if (matches.Length == 0)
        {
            return null;
        }

        return context.LoadFromAssemblyPath(matches[0]);
    }
}
