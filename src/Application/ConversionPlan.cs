namespace PioneerConverter.Core.Application;

public sealed class ConversionPlan
{
    public ConversionPlan(
        string[] filePaths,
        string[] outputPaths,
        List<int> filesToConvert,
        int skippedCompleteFiles,
        int reconvertedIncompleteFiles,
        int missingOutputFiles)
    {
        FilePaths = filePaths;
        OutputPaths = outputPaths;
        FilesToConvert = filesToConvert;
        SkippedCompleteFiles = skippedCompleteFiles;
        ReconvertedIncompleteFiles = reconvertedIncompleteFiles;
        MissingOutputFiles = missingOutputFiles;
    }

    public string[] FilePaths { get; }
    public string[] OutputPaths { get; }
    public List<int> FilesToConvert { get; }
    public int SkippedCompleteFiles { get; }
    public int ReconvertedIncompleteFiles { get; }
    public int MissingOutputFiles { get; }
}
