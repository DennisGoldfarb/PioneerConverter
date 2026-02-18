namespace PioneerConverter.Cli;

internal static class HelpPrinter
{
    public static void ShowHelp()
    {
        Console.WriteLine($"{AppMetadata.AppName} {AppMetadata.Version}");
        Console.WriteLine();
        Console.WriteLine($"Usage: {AppMetadata.AppName} RAW_PATH [options]");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  RAW_PATH                   Path to .raw file or directory containing .raw files");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -b, --batch-size <size>    Process this many scans in each batch (default: 10000)");
        Console.WriteLine("  -o, --output-dir <path>    Output directory for .arrow files (default: <input_dir>/arrow_out)");
        Console.WriteLine("      --skip-existing        Skip conversion when existing output appears complete");
        Console.WriteLine("  -n, --concurrent-files <n> Number of files to convert at the same time (default: 2)");
        Console.WriteLine("  -t, --threads-per-file <n> Scan extraction threads used for each file (default: 3)");
        Console.WriteLine("      --scan-chunk-size <n>  Scan chunk size for scan-thread mode (default: 128)");
        Console.WriteLine("      --version              Show version information");
        Console.WriteLine("  -h, --help                 Show help information");
    }
}
