namespace PioneerConverter.Core.Application;

public interface IFileConverter
{
    void ConvertFile(string inputFile, string outputFile, int batchSize, int scanThreads, int scanChunkSize);
}
