using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace PioneerConverter.Infrastructure.Thermo;

internal static class ThermoScanReader
{
    public static void ReadScanRowIntoBuffers(
        IRawDataPlus rawFile,
        int scanNumber,
        ref int hcdEnergyFieldIndex,
        int bufferIndex,
        double[][] massesBuffer,
        double[][] intensitiesBuffer,
        int[] centroidLengthBuffer,
        string[] scanHeaderBuffer,
        byte[] msOrderBuffer,
        float[] centerMzBuffer,
        float[] isolationWidthMzBuffer,
        float[] collisionEnergyBuffer,
        float[] collisionEnergyEvBuffer,
        bool[] hasCollisionEnergyEvBuffer)
    {
        var scan = Scan.FromFile(rawFile, scanNumber);
        var centroidScan = scan.CentroidScan;
        massesBuffer[bufferIndex] = centroidScan.Masses;
        intensitiesBuffer[bufferIndex] = centroidScan.Intensities;
        centroidLengthBuffer[bufferIndex] = centroidScan.Length;
        scanHeaderBuffer[bufferIndex] = rawFile.GetFilterForScanNumber(scanNumber).ToString();

        var scanEvent = rawFile.GetScanEventForScanNumber(scanNumber);
        byte msOrder = (byte)scanEvent.MSOrder;
        msOrderBuffer[bufferIndex] = msOrder;
        hasCollisionEnergyEvBuffer[bufferIndex] = false;

        if (msOrder <= 1)
        {
            return;
        }

        centerMzBuffer[bufferIndex] = (float)scanEvent.GetMass(0);
        isolationWidthMzBuffer[bufferIndex] = (float)scanEvent.GetIsolationWidth(0) + (float)scanEvent.GetIsolationWidthOffset(0);
        collisionEnergyBuffer[bufferIndex] = (float)scanEvent.GetEnergy(0);

        float ev = 0.0f;
        bool foundEv = false;
        var trailerData = rawFile.GetTrailerExtraInformation(scanNumber);
        if (TryResolveHcdEnergyFieldIndex(trailerData.Labels, trailerData.Length, ref hcdEnergyFieldIndex))
        {
            string energyValue = trailerData.Values[hcdEnergyFieldIndex].Trim();
            foundEv = CollisionEnergyParser.TryParseCollisionEnergyEv(energyValue, out ev);
        }

        if (foundEv)
        {
            collisionEnergyEvBuffer[bufferIndex] = ev;
            hasCollisionEnergyEvBuffer[bufferIndex] = true;
        }
    }

    public static bool TryResolveHcdEnergyFieldIndex(IReadOnlyList<string> labels, int trailerLength, ref int hcdEnergyFieldIndex)
    {
        int labelCount = Math.Min(trailerLength, labels.Count);
        if (hcdEnergyFieldIndex >= 0 &&
            hcdEnergyFieldIndex < labelCount &&
            labels[hcdEnergyFieldIndex] == ThermoConstants.HcdEnergyTrailerLabel)
        {
            return true;
        }

        for (int j = 0; j < labelCount; j++)
        {
            if (labels[j] == ThermoConstants.HcdEnergyTrailerLabel)
            {
                hcdEnergyFieldIndex = j;
                return true;
            }
        }

        hcdEnergyFieldIndex = -1;
        return false;
    }
}
