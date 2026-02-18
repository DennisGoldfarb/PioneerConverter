using PioneerConverter.Cli;

namespace PioneerConverter.UnitTests;

public sealed class ArgumentParserTests
{
    [Fact]
    public void ParseArguments_ParsesCoreFlags()
    {
        string[] args =
        {
            "input.raw",
            "-b", "5000",
            "-o", "./out",
            "--skip-existing",
            "-n", "4",
            "-t", "6",
            "--scan-chunk-size", "256"
        };

        var options = ArgumentParser.ParseArguments(args);

        Assert.Equal("input.raw", options.RawPath);
        Assert.Equal("./out", options.OutputDir);
        Assert.True(options.SkipExisting);
        Assert.Equal(5000, options.BatchSize);
        Assert.Equal(4, options.ConcurrentFiles);
        Assert.Equal(6, options.ThreadsPerFile);
        Assert.Equal(256, options.ScanChunkSize);
        Assert.False(options.ShouldShowHelp);
    }

    [Fact]
    public void ParseArguments_UnknownFlag_ShowsHelp()
    {
        var options = ArgumentParser.ParseArguments(new[] { "--not-real" });

        Assert.True(options.ShouldShowHelp);
    }
}
