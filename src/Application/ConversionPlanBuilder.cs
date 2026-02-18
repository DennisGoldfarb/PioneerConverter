using System.IO;

namespace PioneerConverter.Core.Application;

public static class ConversionPlanBuilder
{
    public static bool TryResolveInput(string rawPath, out string inputMode, out string inputDirectory, out string error)
    {
        bool rawPathIsFile = File.Exists(rawPath);
        bool rawPathIsDirectory = Directory.Exists(rawPath);
        if (!rawPathIsFile && !rawPathIsDirectory)
        {
            inputMode = string.Empty;
            inputDirectory = string.Empty;
            error = $"File or Directory does not exist: {rawPath}";
            return false;
        }

        inputMode = rawPathIsDirectory ? "directory" : "file";
        if (rawPathIsDirectory)
        {
            inputDirectory = Path.GetFullPath(rawPath);
            error = string.Empty;
            return true;
        }

        string inputFilePath = Path.GetFullPath(rawPath);
        string? directory = Path.GetDirectoryName(inputFilePath);
        if (directory == null)
        {
            inputDirectory = string.Empty;
            error = "Invalid input directory";
            return false;
        }

        inputDirectory = directory;
        error = string.Empty;
        return true;
    }

    public static string[] GetFilePaths(string rawPath)
    {
        if (File.Exists(rawPath))
        {
            return new[] { Path.GetFullPath(rawPath) };
        }

        if (Directory.Exists(rawPath))
        {
            string directoryPath = Path.GetFullPath(rawPath);
            return Directory.GetFiles(directoryPath, "*.raw", SearchOption.TopDirectoryOnly);
        }

        return Array.Empty<string>();
    }

    public static bool TryBuildOutputDirectory(string inputDirectory, string requestedOutputDir, out string outputDirectory, out string error)
    {
        outputDirectory = string.IsNullOrWhiteSpace(requestedOutputDir)
            ? Path.Combine(inputDirectory, "arrow_out")
            : Path.GetFullPath(requestedOutputDir);

        if (File.Exists(outputDirectory))
        {
            error = $"Output path points to an existing file: {outputDirectory}";
            outputDirectory = string.Empty;
            return false;
        }

        try
        {
            Directory.CreateDirectory(outputDirectory);
        }
        catch (Exception ex)
        {
            error = $"Could not create output directory '{outputDirectory}': {ex.Message}";
            outputDirectory = string.Empty;
            return false;
        }

        error = string.Empty;
        return true;
    }

    public static string[] GetOutputPaths(string outputDirectory, string[] filePaths)
    {
        string[] outputPaths = new string[filePaths.Length];
        for (int i = 0; i < filePaths.Length; i++)
        {
            string fileBasename = Path.GetFileNameWithoutExtension(filePaths[i]);
            outputPaths[i] = Path.Combine(outputDirectory, fileBasename + ".arrow");
        }

        return outputPaths;
    }

    public static ConversionPlan BuildPlan(
        string[] filePaths,
        string[] outputPaths,
        bool skipExisting,
        IOutputCompletenessChecker outputCompletenessChecker)
    {
        var filesToConvert = new List<int>(filePaths.Length);
        int skippedCompleteFiles = 0;
        int reconvertedIncompleteFiles = 0;
        int missingOutputFiles = 0;

        for (int i = 0; i < filePaths.Length; i++)
        {
            if (!skipExisting)
            {
                filesToConvert.Add(i);
                continue;
            }

            if (!File.Exists(outputPaths[i]))
            {
                missingOutputFiles++;
                filesToConvert.Add(i);
                continue;
            }

            if (outputCompletenessChecker.HasCompleteExistingOutput(filePaths[i], outputPaths[i]))
            {
                skippedCompleteFiles++;
                continue;
            }

            reconvertedIncompleteFiles++;
            filesToConvert.Add(i);
        }

        return new ConversionPlan(
            filePaths,
            outputPaths,
            filesToConvert,
            skippedCompleteFiles,
            reconvertedIncompleteFiles,
            missingOutputFiles);
    }
}
