using PioneerConverter.Core.Application;

namespace PioneerConverter.UnitTests;

public sealed class ConversionPlanBuilderTests
{
    [Fact]
    public void GetOutputPaths_RewritesExtensionAndDirectory()
    {
        string outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        string[] filePaths =
        {
            "/tmp/a.raw",
            "/tmp/b.raw"
        };

        string[] outputPaths = ConversionPlanBuilder.GetOutputPaths(outputDir, filePaths);

        Assert.Equal(Path.Combine(outputDir, "a.arrow"), outputPaths[0]);
        Assert.Equal(Path.Combine(outputDir, "b.arrow"), outputPaths[1]);
    }

    [Fact]
    public void BuildPlan_SkipExisting_TracksQueueCounts()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            string[] filePaths =
            {
                Path.Combine(tempDir, "one.raw"),
                Path.Combine(tempDir, "two.raw"),
                Path.Combine(tempDir, "three.raw")
            };
            string[] outputPaths =
            {
                Path.Combine(tempDir, "one.arrow"),
                Path.Combine(tempDir, "two.arrow"),
                Path.Combine(tempDir, "three.arrow")
            };

            File.WriteAllText(outputPaths[0], "complete");
            File.WriteAllText(outputPaths[1], "incomplete");

            var checker = new FakeOutputCompletenessChecker(
                (filePaths[0], outputPaths[0], true),
                (filePaths[1], outputPaths[1], false));

            ConversionPlan plan = ConversionPlanBuilder.BuildPlan(filePaths, outputPaths, skipExisting: true, checker);

            Assert.Single(plan.FilesToConvert, 1);
            Assert.Contains(2, plan.FilesToConvert);
            Assert.Equal(1, plan.SkippedCompleteFiles);
            Assert.Equal(1, plan.ReconvertedIncompleteFiles);
            Assert.Equal(1, plan.MissingOutputFiles);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private sealed class FakeOutputCompletenessChecker : IOutputCompletenessChecker
    {
        private readonly Dictionary<(string input, string output), bool> _map;

        public FakeOutputCompletenessChecker(params (string input, string output, bool complete)[] entries)
        {
            _map = entries.ToDictionary(k => (k.input, k.output), v => v.complete);
        }

        public bool HasCompleteExistingOutput(string inputFile, string outputFile)
        {
            return _map.TryGetValue((inputFile, outputFile), out bool complete) && complete;
        }
    }
}
