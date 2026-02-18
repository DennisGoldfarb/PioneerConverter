namespace PioneerConverter.Core.Application;

public interface IOutputCompletenessChecker
{
    bool HasCompleteExistingOutput(string inputFile, string outputFile);
}
