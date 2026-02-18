using PioneerConverter.Core.Application;

namespace PioneerConverter.Cli;

public static class ArgumentParser
{
    public static ConversionOptions ParseArguments(string[] args)
    {
        var options = new ConversionOptions();

        if (args.Length == 0)
        {
            options.ShouldShowHelp = true;
            return options;
        }

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-b":
                case "--batch-size":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out int batchSize))
                    {
                        options.BatchSize = batchSize;
                    }
                    else
                    {
                        Console.WriteLine("Invalid value for {0}", args[i]);
                        options.ShouldShowHelp = true;
                        return options;
                    }
                    break;
                case "-o":
                case "--output-dir":
                    if (i + 1 < args.Length)
                    {
                        options.OutputDir = args[++i];
                    }
                    else
                    {
                        Console.WriteLine("Missing value for {0}", args[i]);
                        options.ShouldShowHelp = true;
                        return options;
                    }
                    break;
                case "--skip-existing":
                    options.SkipExisting = true;
                    break;
                case "-n":
                case "--concurrent-files":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out int concurrentFiles))
                    {
                        options.ConcurrentFiles = concurrentFiles;
                    }
                    else
                    {
                        Console.WriteLine("Invalid value for {0}", args[i]);
                        options.ShouldShowHelp = true;
                        return options;
                    }
                    break;
                case "-t":
                case "--threads-per-file":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out int threadsPerFile))
                    {
                        options.ThreadsPerFile = threadsPerFile;
                    }
                    else
                    {
                        Console.WriteLine("Invalid value for {0}", args[i]);
                        options.ShouldShowHelp = true;
                        return options;
                    }
                    break;
                case "--scan-chunk-size":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out int scanChunkSize))
                    {
                        options.ScanChunkSize = scanChunkSize;
                    }
                    else
                    {
                        Console.WriteLine("Invalid value for {0}", args[i]);
                        options.ShouldShowHelp = true;
                        return options;
                    }
                    break;
                case "--version":
                    options.ShouldShowVersion = true;
                    break;
                case "-h":
                case "--help":
                    options.ShouldShowHelp = true;
                    break;
                default:
                    if (args[i].StartsWith("-", StringComparison.Ordinal))
                    {
                        Console.WriteLine("Unknown option: {0}", args[i]);
                        options.ShouldShowHelp = true;
                        return options;
                    }

                    if (string.IsNullOrEmpty(options.RawPath))
                    {
                        options.RawPath = args[i];
                    }
                    else
                    {
                        Console.WriteLine("Unexpected argument: {0}", args[i]);
                        options.ShouldShowHelp = true;
                        return options;
                    }
                    break;
            }
        }

        return options;
    }
}
