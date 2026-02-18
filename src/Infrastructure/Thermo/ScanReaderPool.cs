using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace PioneerConverter.Infrastructure.Thermo;

internal sealed class ScanReaderWorker : IDisposable
{
    public IRawDataPlus RawFile { get; }
    public int HcdEnergyFieldIndex = -2;

    private ScanReaderWorker(IRawDataPlus rawFile)
    {
        RawFile = rawFile;
        RawFile.SelectInstrument(Device.MS, 1);
    }

    public static ScanReaderWorker Create(IRawFileThreadManager threadManager)
    {
        return new ScanReaderWorker((IRawDataPlus)threadManager.CreateThreadAccessor());
    }

    public void Dispose()
    {
        RawFile.Dispose();
    }
}

internal static class ScanReaderPool
{
    public static List<ScanReaderWorker> Create(IRawFileThreadManager threadManager, int scanThreads)
    {
        int workerCount = Math.Max(1, scanThreads);
        var workers = new List<ScanReaderWorker>(workerCount);
        try
        {
            for (int i = 0; i < workerCount; i++)
            {
                workers.Add(ScanReaderWorker.Create(threadManager));
            }
        }
        catch
        {
            Dispose(workers);
            throw;
        }

        return workers;
    }

    public static void Dispose(List<ScanReaderWorker>? workers)
    {
        if (workers == null)
        {
            return;
        }

        foreach (var worker in workers)
        {
            worker.Dispose();
        }
    }
}
