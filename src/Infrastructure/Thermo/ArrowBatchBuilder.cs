using System.Buffers;
using Apache.Arrow;
using Apache.Arrow.Types;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace PioneerConverter.Infrastructure.Thermo;

internal static class ArrowBatchBuilder
{
    private static void EnsureScratchCapacity(ref float[] buffer, int requiredLength)
    {
        if (buffer.Length >= requiredLength)
        {
            return;
        }

        int newLength = buffer.Length == 0 ? requiredLength : Math.Max(requiredLength, buffer.Length * 2);
        float[] replacement = ArrayPool<float>.Shared.Rent(newLength);
        if (buffer.Length > 0)
        {
            ArrayPool<float>.Shared.Return(buffer, clearArray: false);
        }

        buffer = replacement;
    }

    private static void AppendPeaks(
        double[] masses,
        double[] intensities,
        int centroidLength,
        FloatArray.Builder massValueBuilder,
        FloatArray.Builder intensityValueBuilder,
        ref float[] massScratchBuffer,
        ref float[] intensityScratchBuffer)
    {
        EnsureScratchCapacity(ref massScratchBuffer, centroidLength);
        EnsureScratchCapacity(ref intensityScratchBuffer, centroidLength);

        for (int j = 0; j < centroidLength; j++)
        {
            massScratchBuffer[j] = (float)masses[j];
            intensityScratchBuffer[j] = (float)intensities[j];
        }

        massValueBuilder.AppendRange(new ArraySegment<float>(massScratchBuffer, 0, centroidLength));
        intensityValueBuilder.AppendRange(new ArraySegment<float>(intensityScratchBuffer, 0, centroidLength));
    }

    public static RecordBatch BuildRecordBatch(
        IRawDataPlus rawFile,
        int batchStart,
        int batchSize,
        int lastScanNumber,
        int scanChunkSize,
        List<ScanReaderWorker>? scanWorkers,
        ParallelOptions? scanParallelOptions,
        ref int hcdEnergyFieldIndex,
        Schema schema,
        ref float[] massScratchBuffer,
        ref float[] intensityScratchBuffer)
    {
        int batchEnd = Math.Min(batchStart + batchSize - 1, lastScanNumber);
        int batchRowCount = batchEnd - batchStart + 1;
        ulong batchPeakCount = 0;

        var massListBuilder = new ListArray.Builder(FloatType.Default);
        var massValueBuilder = massListBuilder.ValueBuilder as FloatArray.Builder
            ?? throw new InvalidOperationException("Expected float value builder for mz array");
        var intensityListBuilder = new ListArray.Builder(FloatType.Default);
        var intensityValueBuilder = intensityListBuilder.ValueBuilder as FloatArray.Builder
            ?? throw new InvalidOperationException("Expected float value builder for intensity array");

        var scanHeaderBuilder = new StringArray.Builder();
        var scanNumberBuilder = new Int32Array.Builder();
        var basePeakMzBuilder = new FloatArray.Builder();
        var basePeakIntensityBuilder = new FloatArray.Builder();
        var packetTypeBuilder = new Int32Array.Builder();
        var retentionTimeBuilder = new FloatArray.Builder();
        var lowMzBuilder = new FloatArray.Builder();
        var highMzBuilder = new FloatArray.Builder();
        var ticBuilder = new FloatArray.Builder();

        var centerMzBuilder = new FloatArray.Builder();
        var isolationWidthMzBuilder = new FloatArray.Builder();
        var collisionEnergyBuilder = new FloatArray.Builder();
        var collisionEnergyEvBuilder = new FloatArray.Builder();
        var msOrderBuilder = new UInt8Array.Builder();

        var basePeakMzCache = new float[batchRowCount];
        var packetTypeCache = new int[batchRowCount];
        var basePeakIntensityCache = new float[batchRowCount];
        var retentionTimeCache = new float[batchRowCount];
        var lowMzCache = new float[batchRowCount];
        var highMzCache = new float[batchRowCount];
        var ticCache = new float[batchRowCount];

        for (int rowIndex = 0; rowIndex < batchRowCount; rowIndex++)
        {
            int scanNumber = batchStart + rowIndex;
            var scanStats = rawFile.GetScanStatsForScanNumber(scanNumber);
            batchPeakCount += (ulong)scanStats.PacketCount;
            basePeakMzCache[rowIndex] = (float)scanStats.BasePeakMass;
            packetTypeCache[rowIndex] = scanStats.PacketType;
            basePeakIntensityCache[rowIndex] = (float)scanStats.BasePeakIntensity;
            retentionTimeCache[rowIndex] = (float)scanStats.StartTime;
            lowMzCache[rowIndex] = (float)scanStats.LowMass;
            highMzCache[rowIndex] = (float)scanStats.HighMass;
            ticCache[rowIndex] = (float)scanStats.TIC;
        }

        massListBuilder.Reserve(batchRowCount);
        intensityListBuilder.Reserve(batchRowCount);
        scanNumberBuilder.Reserve(batchRowCount);
        basePeakMzBuilder.Reserve(batchRowCount);
        basePeakIntensityBuilder.Reserve(batchRowCount);
        packetTypeBuilder.Reserve(batchRowCount);
        retentionTimeBuilder.Reserve(batchRowCount);
        lowMzBuilder.Reserve(batchRowCount);
        highMzBuilder.Reserve(batchRowCount);
        ticBuilder.Reserve(batchRowCount);
        centerMzBuilder.Reserve(batchRowCount);
        isolationWidthMzBuilder.Reserve(batchRowCount);
        collisionEnergyBuilder.Reserve(batchRowCount);
        collisionEnergyEvBuilder.Reserve(batchRowCount);
        msOrderBuilder.Reserve(batchRowCount);
        massValueBuilder.Reserve((int)batchPeakCount);
        intensityValueBuilder.Reserve((int)batchPeakCount);

        if (scanWorkers != null && scanParallelOptions != null)
        {
            int workerCount = scanWorkers.Count;
            int chunkBufferSize = Math.Min(scanChunkSize, batchRowCount);
            var chunkMasses = new double[chunkBufferSize][];
            var chunkIntensities = new double[chunkBufferSize][];
            var chunkCentroidLengths = new int[chunkBufferSize];
            var chunkScanHeaders = new string[chunkBufferSize];
            var chunkMsOrders = new byte[chunkBufferSize];
            var chunkCenterMz = new float[chunkBufferSize];
            var chunkIsolationWidthMz = new float[chunkBufferSize];
            var chunkCollisionEnergy = new float[chunkBufferSize];
            var chunkCollisionEnergyEv = new float[chunkBufferSize];
            var chunkHasCollisionEnergyEv = new bool[chunkBufferSize];

            for (int chunkStart = batchStart; chunkStart <= batchEnd; chunkStart += scanChunkSize)
            {
                int chunkEnd = Math.Min(chunkStart + scanChunkSize - 1, batchEnd);
                int chunkCount = chunkEnd - chunkStart + 1;

                Parallel.For(
                    0,
                    workerCount,
                    scanParallelOptions,
                    workerIndex =>
                    {
                        var worker = scanWorkers[workerIndex];
                        for (int localIndex = workerIndex; localIndex < chunkCount; localIndex += workerCount)
                        {
                            int scanNumber = chunkStart + localIndex;
                            ThermoScanReader.ReadScanRowIntoBuffers(
                                worker.RawFile,
                                scanNumber,
                                ref worker.HcdEnergyFieldIndex,
                                localIndex,
                                chunkMasses,
                                chunkIntensities,
                                chunkCentroidLengths,
                                chunkScanHeaders,
                                chunkMsOrders,
                                chunkCenterMz,
                                chunkIsolationWidthMz,
                                chunkCollisionEnergy,
                                chunkCollisionEnergyEv,
                                chunkHasCollisionEnergyEv);
                        }
                    });

                for (int localIndex = 0; localIndex < chunkCount; localIndex++)
                {
                    int scanNumber = chunkStart + localIndex;
                    int rowIndex = scanNumber - batchStart;
                    var masses = chunkMasses[localIndex];
                    var intensities = chunkIntensities[localIndex];
                    int centroidLength = chunkCentroidLengths[localIndex];

                    massListBuilder.Append();
                    intensityListBuilder.Append();
                    AppendPeaks(
                        masses,
                        intensities,
                        centroidLength,
                        massValueBuilder,
                        intensityValueBuilder,
                        ref massScratchBuffer,
                        ref intensityScratchBuffer);

                    scanHeaderBuilder.Append(chunkScanHeaders[localIndex]);
                    scanNumberBuilder.Append(scanNumber);
                    basePeakMzBuilder.Append(basePeakMzCache[rowIndex]);
                    packetTypeBuilder.Append(packetTypeCache[rowIndex]);
                    basePeakIntensityBuilder.Append(basePeakIntensityCache[rowIndex]);
                    retentionTimeBuilder.Append(retentionTimeCache[rowIndex]);
                    lowMzBuilder.Append(lowMzCache[rowIndex]);
                    highMzBuilder.Append(highMzCache[rowIndex]);
                    ticBuilder.Append(ticCache[rowIndex]);

                    byte msOrder = chunkMsOrders[localIndex];
                    if (msOrder > 1)
                    {
                        centerMzBuilder.Append(chunkCenterMz[localIndex]);
                        isolationWidthMzBuilder.Append(chunkIsolationWidthMz[localIndex]);
                        collisionEnergyBuilder.Append(chunkCollisionEnergy[localIndex]);
                        if (chunkHasCollisionEnergyEv[localIndex])
                        {
                            collisionEnergyEvBuilder.Append(chunkCollisionEnergyEv[localIndex]);
                        }
                        else
                        {
                            collisionEnergyEvBuilder.AppendNull();
                        }
                    }
                    else
                    {
                        centerMzBuilder.AppendNull();
                        isolationWidthMzBuilder.AppendNull();
                        collisionEnergyBuilder.AppendNull();
                        collisionEnergyEvBuilder.AppendNull();
                    }

                    msOrderBuilder.Append(msOrder);
                }
            }
        }
        else
        {
            for (int scanNumber = batchStart; scanNumber <= batchEnd; scanNumber++)
            {
                int rowIndex = scanNumber - batchStart;
                var scan = Scan.FromFile(rawFile, scanNumber);
                var centroidScan = scan.CentroidScan;
                var masses = centroidScan.Masses;
                var intensities = centroidScan.Intensities;
                int centroidLength = centroidScan.Length;

                massListBuilder.Append();
                intensityListBuilder.Append();
                AppendPeaks(
                    masses,
                    intensities,
                    centroidLength,
                    massValueBuilder,
                    intensityValueBuilder,
                    ref massScratchBuffer,
                    ref intensityScratchBuffer);

                scanHeaderBuilder.Append(rawFile.GetFilterForScanNumber(scanNumber).ToString());
                scanNumberBuilder.Append(scanNumber);
                basePeakMzBuilder.Append(basePeakMzCache[rowIndex]);
                packetTypeBuilder.Append(packetTypeCache[rowIndex]);
                basePeakIntensityBuilder.Append(basePeakIntensityCache[rowIndex]);
                retentionTimeBuilder.Append(retentionTimeCache[rowIndex]);
                lowMzBuilder.Append(lowMzCache[rowIndex]);
                highMzBuilder.Append(highMzCache[rowIndex]);
                ticBuilder.Append(ticCache[rowIndex]);

                var scanEvent = rawFile.GetScanEventForScanNumber(scanNumber);
                if ((byte)scanEvent.MSOrder > 1)
                {
                    centerMzBuilder.Append((float)scanEvent.GetMass(0));
                    isolationWidthMzBuilder.Append((float)scanEvent.GetIsolationWidth(0) + (float)scanEvent.GetIsolationWidthOffset(0));
                    collisionEnergyBuilder.Append((float)scanEvent.GetEnergy(0));

                    float ev = 0.0f;
                    bool foundEv = false;
                    var trailerData = rawFile.GetTrailerExtraInformation(scanNumber);
                    if (ThermoScanReader.TryResolveHcdEnergyFieldIndex(trailerData.Labels, trailerData.Length, ref hcdEnergyFieldIndex))
                    {
                        string energyValue = trailerData.Values[hcdEnergyFieldIndex].Trim();
                        foundEv = CollisionEnergyParser.TryParseCollisionEnergyEv(energyValue, out ev);
                    }

                    if (!foundEv)
                    {
                        collisionEnergyEvBuilder.AppendNull();
                    }
                    else
                    {
                        collisionEnergyEvBuilder.Append(ev);
                    }
                }
                else
                {
                    centerMzBuilder.AppendNull();
                    isolationWidthMzBuilder.AppendNull();
                    collisionEnergyBuilder.AppendNull();
                    collisionEnergyEvBuilder.AppendNull();
                }

                msOrderBuilder.Append((byte)scanEvent.MSOrder);
            }
        }

        var massArray = massListBuilder.Build();
        var intensityArray = intensityListBuilder.Build();
        IArrowArray scanHeaderArray = scanHeaderBuilder.Build();
        IArrowArray scanNumberArray = scanNumberBuilder.Build();
        IArrowArray basePeakMzArray = basePeakMzBuilder.Build();
        IArrowArray basePeakIntensityArray = basePeakIntensityBuilder.Build();
        IArrowArray packetTypeArray = packetTypeBuilder.Build();
        IArrowArray retentionTimeArray = retentionTimeBuilder.Build();
        IArrowArray lowMzArray = lowMzBuilder.Build();
        IArrowArray highMzArray = highMzBuilder.Build();
        IArrowArray ticArray = ticBuilder.Build();
        IArrowArray centerMzArray = centerMzBuilder.Build();
        IArrowArray isolationWidthMzArray = isolationWidthMzBuilder.Build();
        IArrowArray collisionEnergyArray = collisionEnergyBuilder.Build();
        IArrowArray collisionEnergyEvArray = collisionEnergyEvBuilder.Build();
        IArrowArray msOrderArray = msOrderBuilder.Build();

        return new RecordBatch(schema, new[]
        {
            massArray,
            intensityArray,
            scanHeaderArray,
            scanNumberArray,
            basePeakMzArray,
            basePeakIntensityArray,
            packetTypeArray,
            retentionTimeArray,
            lowMzArray,
            highMzArray,
            ticArray,
            centerMzArray,
            isolationWidthMzArray,
            collisionEnergyArray,
            collisionEnergyEvArray,
            msOrderArray
        }, batchRowCount);
    }
}
