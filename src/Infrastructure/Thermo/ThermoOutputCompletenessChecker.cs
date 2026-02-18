using System.IO;
using Apache.Arrow;
using Apache.Arrow.Ipc;
using PioneerConverter.Core.Application;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.RawFileReader;

namespace PioneerConverter.Infrastructure.Thermo;

public sealed class ThermoOutputCompletenessChecker : IOutputCompletenessChecker
{
    public bool HasCompleteExistingOutput(string inputFile, string outputFile)
    {
        try
        {
            int rawLastScan = GetRawLastScanNumber(inputFile);
            int? outputLastScan = GetOutputLastScanNumber(outputFile);
            return outputLastScan.HasValue && outputLastScan.Value == rawLastScan;
        }
        catch
        {
            return false;
        }
    }

    private static int GetRawLastScanNumber(string inputFile)
    {
        using var rawFile = RawFileReaderAdapter.FileFactory(inputFile);
        if (!rawFile.IsOpen || rawFile.IsError)
        {
            throw new InvalidOperationException($"Unable to read RAW file: {inputFile}");
        }

        rawFile.SelectInstrument(Device.MS, 1);
        return rawFile.RunHeaderEx.LastSpectrum;
    }

    private static int? GetOutputLastScanNumber(string outputFile)
    {
        using var fileStream = new FileStream(outputFile, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new ArrowFileReader(fileStream);

        int? lastScanNumber = null;
        int scanNumberFieldIndex = -1;
        while (reader.ReadNextRecordBatch() is RecordBatch batch)
        {
            using (batch)
            {
                if (batch.Length == 0)
                {
                    continue;
                }

                if (scanNumberFieldIndex < 0)
                {
                    for (int i = 0; i < batch.Schema.FieldsList.Count; i++)
                    {
                        if (string.Equals(batch.Schema.FieldsList[i].Name, ThermoConstants.ScanNumberColumnName, StringComparison.Ordinal))
                        {
                            scanNumberFieldIndex = i;
                            break;
                        }
                    }

                    if (scanNumberFieldIndex < 0)
                    {
                        throw new InvalidDataException(
                            $"Missing required column '{ThermoConstants.ScanNumberColumnName}' in output file: {outputFile}");
                    }
                }

                if (batch.Column(scanNumberFieldIndex) is not Int32Array scanNumbers)
                {
                    throw new InvalidDataException(
                        $"Column '{ThermoConstants.ScanNumberColumnName}' is not Int32 in output file: {outputFile}");
                }

                int lastIndex = checked((int)batch.Length - 1);
                int? batchLastScanNumber = scanNumbers.GetValue(lastIndex);
                if (!batchLastScanNumber.HasValue)
                {
                    throw new InvalidDataException(
                        $"Column '{ThermoConstants.ScanNumberColumnName}' has null values in output file: {outputFile}");
                }

                lastScanNumber = batchLastScanNumber.Value;
            }
        }

        return lastScanNumber;
    }
}
