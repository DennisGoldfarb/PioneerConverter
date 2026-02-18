namespace PioneerConverter.Core.Application;

public sealed class ConversionOptions
{
    public string RawPath { get; set; } = string.Empty;
    public string OutputDir { get; set; } = string.Empty;
    public bool SkipExisting { get; set; }
    public bool ShouldShowHelp { get; set; }
    public bool ShouldShowVersion { get; set; }
    public int BatchSize { get; set; } = 10000;
    public int ConcurrentFiles { get; set; } = 2;
    public int ThreadsPerFile { get; set; } = 3;
    public int ScanChunkSize { get; set; } = 128;

    public void Normalize()
    {
        BatchSize = Math.Max(1, BatchSize);
        ConcurrentFiles = Math.Max(1, ConcurrentFiles);
        ThreadsPerFile = Math.Max(1, ThreadsPerFile);
        ScanChunkSize = Math.Max(1, ScanChunkSize);
    }
}
