using System.Diagnostics;
using PioneerConverter.Core.Common;

namespace PioneerConverter.Core.Application;

public sealed class ConversionRunner
{
    private readonly IFileConverter _fileConverter;
    private readonly IOutputCompletenessChecker _outputCompletenessChecker;
    private readonly IReporter _reporter;

    public ConversionRunner(
        IFileConverter fileConverter,
        IOutputCompletenessChecker outputCompletenessChecker,
        IReporter reporter)
    {
        _fileConverter = fileConverter;
        _outputCompletenessChecker = outputCompletenessChecker;
        _reporter = reporter;
    }

    public void Run(ConversionOptions options, string appDisplayName)
    {
        options.Normalize();

        var totalExecutionWatch = Stopwatch.StartNew();

        if (!ConversionPlanBuilder.TryResolveInput(options.RawPath, out string inputMode, out string inputDirectory, out string inputError))
        {
            _reporter.WriteLine(appDisplayName);
            _reporter.WriteLine(inputError);
            return;
        }

        string[] filePaths = ConversionPlanBuilder.GetFilePaths(options.RawPath);
        if (!ConversionPlanBuilder.TryBuildOutputDirectory(inputDirectory, options.OutputDir, out string outputDirectory, out string outputError))
        {
            _reporter.WriteLine(outputError);
            return;
        }

        string[] outputPaths = ConversionPlanBuilder.GetOutputPaths(outputDirectory, filePaths);
        ConversionPlan plan = ConversionPlanBuilder.BuildPlan(filePaths, outputPaths, options.SkipExisting, _outputCompletenessChecker);

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = options.ConcurrentFiles
        };

        _reporter.WriteLine(appDisplayName);
        _reporter.WriteLine("==================================================");
        _reporter.WriteLine(
            $"Config: concurrent-files={options.ConcurrentFiles}  threads-per-file={options.ThreadsPerFile}  scan-chunk-size={options.ScanChunkSize}  batch-size={options.BatchSize}");
        _reporter.WriteLine($"Config: output={outputDirectory}");
        _reporter.WriteLine($"Config: skip-existing={options.SkipExisting.ToString().ToLowerInvariant()}");
        _reporter.WriteLine(string.Empty);
        _reporter.WriteLine($"Input : {inputMode} {options.RawPath}");
        _reporter.WriteLine($"Queue : discovered={plan.FilePaths.Length}  convert={plan.FilesToConvert.Count}");
        if (options.SkipExisting)
        {
            _reporter.WriteLine(
                $"Queue : skipped-complete={plan.SkippedCompleteFiles}  reconvert-incomplete={plan.ReconvertedIncompleteFiles}  missing-output={plan.MissingOutputFiles}");
        }

        _reporter.WriteLine("==================================================");

        if (plan.FilePaths.Length == 0)
        {
            totalExecutionWatch.Stop();
            _reporter.WriteLine("No .raw files found to process");
            _reporter.WriteLine("Total conversion time: {0}", DurationFormatter.Format(totalExecutionWatch.Elapsed));
            return;
        }

        if (plan.FilesToConvert.Count == 0)
        {
            totalExecutionWatch.Stop();
            _reporter.WriteLine("No files to convert.");
            _reporter.WriteLine("Total conversion time: {0}", DurationFormatter.Format(totalExecutionWatch.Elapsed));
            return;
        }

        Parallel.ForEach(plan.FilesToConvert, parallelOptions, fileIndex =>
        {
            _fileConverter.ConvertFile(
                plan.FilePaths[fileIndex],
                plan.OutputPaths[fileIndex],
                options.BatchSize,
                options.ThreadsPerFile,
                options.ScanChunkSize);
        });

        totalExecutionWatch.Stop();
        _reporter.WriteLine("Total conversion time: {0}", DurationFormatter.Format(totalExecutionWatch.Elapsed));
    }
}
