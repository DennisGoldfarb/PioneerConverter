using System.Buffers;
using System.Diagnostics;
using System.IO;
using Apache.Arrow.Ipc;
using PioneerConverter.Core.Application;
using PioneerConverter.Core.Common;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader;

namespace PioneerConverter.Infrastructure.Thermo;

public sealed class ThermoFileConverter : IFileConverter
{
    private readonly IReporter _reporter;

    public ThermoFileConverter(IReporter reporter)
    {
        _reporter = reporter;
    }

    public void ConvertFile(string inputFile, string outputFile, int batchSize, int scanThreads, int scanChunkSize)
    {
        _reporter.WriteLine("Starting Conversion For: {0}", Path.GetFileNameWithoutExtension(inputFile));

        using var rawFile = RawFileReaderAdapter.FileFactory(inputFile);
        if (!rawFile.IsOpen || rawFile.IsError)
        {
            if (rawFile.IsError)
            {
                _reporter.WriteLine("Error opening ({0}) - {1}", rawFile.FileError.ErrorMessage, inputFile);
                return;
            }

            _reporter.WriteLine("Unable to access the RAW file using the RawFileReader class!");
            return;
        }

        rawFile.SelectInstrument(Device.MS, 1);

        int firstScanNumber = rawFile.RunHeaderEx.FirstSpectrum;
        int lastScanNumber = rawFile.RunHeaderEx.LastSpectrum;
        var schema = ArrowSchemaFactory.Create();

        var watch = Stopwatch.StartNew();

        int hcdEnergyFieldIndex = -2;
        IRawFileThreadManager? scanThreadManager = null;
        List<ScanReaderWorker>? scanWorkers = null;
        ParallelOptions? scanParallelOptions = null;
        float[] massScratchBuffer = Array.Empty<float>();
        float[] intensityScratchBuffer = Array.Empty<float>();

        try
        {
            if (scanThreads > 1)
            {
                try
                {
                    IRawFileThreadManager threadManager = RawFileReaderFactory.CreateThreadManager(inputFile);
                    scanThreadManager = threadManager;
                    scanWorkers = ScanReaderPool.Create(threadManager, scanThreads);
                    scanParallelOptions = new ParallelOptions
                    {
                        MaxDegreeOfParallelism = scanWorkers.Count
                    };
                }
                catch (Exception ex)
                {
                    ScanReaderPool.Dispose(scanWorkers);
                    scanWorkers = null;
                    scanThreadManager?.Dispose();
                    scanThreadManager = null;
                    scanParallelOptions = null;
                    _reporter.WriteLine(
                        "Warning: scan-thread mode unavailable for {0} ({1}: {2}). Falling back to single-thread scan extraction.",
                        Path.GetFileNameWithoutExtension(inputFile),
                        ex.GetType().Name,
                        ex.Message);
                }
            }

            using var fileStream = new FileStream(
                outputFile,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                1 << 20);
            using var writer = new ArrowFileWriter(fileStream, schema);

            writer.WriteStartAsync().GetAwaiter().GetResult();

            for (int batchStart = firstScanNumber; batchStart <= lastScanNumber; batchStart += batchSize)
            {
                using var batch = ArrowBatchBuilder.BuildRecordBatch(
                    rawFile,
                    batchStart,
                    batchSize,
                    lastScanNumber,
                    scanChunkSize,
                    scanWorkers,
                    scanParallelOptions,
                    ref hcdEnergyFieldIndex,
                    schema,
                    ref massScratchBuffer,
                    ref intensityScratchBuffer);

                writer.WriteRecordBatch(batch);
            }

            writer.WriteEndAsync().GetAwaiter().GetResult();
        }
        finally
        {
            watch.Stop();
            ScanReaderPool.Dispose(scanWorkers);
            scanThreadManager?.Dispose();
            if (massScratchBuffer.Length > 0)
            {
                ArrayPool<float>.Shared.Return(massScratchBuffer, clearArray: false);
            }

            if (intensityScratchBuffer.Length > 0)
            {
                ArrayPool<float>.Shared.Return(intensityScratchBuffer, clearArray: false);
            }
        }

        _reporter.WriteLine(
            "Execution Time: {0} ms for {1}",
            watch.ElapsedMilliseconds,
            Path.GetFileNameWithoutExtension(inputFile));
    }
}
