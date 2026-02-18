using System.Reflection;

namespace PioneerConverter.Cli;

internal static class AppMetadata
{
    public const string AppName = "PioneerConverter";
    private const string DefaultVersion = "0.0.0-dev";
    private static readonly Lazy<string> VersionProvider = new(ResolveVersion);

    public static string Version => VersionProvider.Value;

    private static string ResolveVersion()
    {
        Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        string? informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            return NormalizeVersion(informationalVersion);
        }

        string? assemblyVersion = assembly.GetName().Version?.ToString();
        return string.IsNullOrWhiteSpace(assemblyVersion) ? DefaultVersion : NormalizeVersion(assemblyVersion);
    }

    private static string NormalizeVersion(string value)
    {
        int metadataSeparatorIndex = value.IndexOf('+');
        return metadataSeparatorIndex >= 0 ? value.Substring(0, metadataSeparatorIndex) : value;
    }
}
