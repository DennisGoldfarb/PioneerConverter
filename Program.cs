// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;


using ThermoFisher.CommonCore.BackgroundSubtraction;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.MassPrecisionEstimator;
using ThermoFisher.CommonCore.RawFileReader;


using Apache.Arrow;
using Apache.Arrow.Ipc;
using Apache.Arrow.Memory;
using Apache.Arrow.Types;

using CommandLine;
internal static class Program
{
    //probably remove --self-contained flag so that the .zip of fits on github
    // dotnet publish -c Release -r win-x64 -p:PublishReadyToRun=true
    //dotnet publish -c Release -r osx-x64 -p:PublishReadyToRun=true
    //dotnet publish -c Release -r linux-x64 -p:PublishReadyToRun=true
    //Console.WriteLine("Error opening ({0}) - {1}", rawFile.FileError.ErrorMessage, inputFile);
    class Options
    {
        [Value(0, Required = true)]
        public string? raw_path { get; set; }

        [Option('b', "batch-size", Default = 10000, HelpText = "Process this many scans in each batch...")]
        public int batchSize { get; set; }

        [Option('n', "threads", Default = 2, HelpText = "Maximum number of threads to use..")]
        public int threads { get; set; }


    }



    public static void Main(string[] args)
    {


        //Argument placeholders
        string? raw_path = "";
        int batchSize = 0;
        int n_threads = 0;

        //Convert an individual .raw file or all .raw files in a directory
        Parser.Default.ParseArguments<Options>(args)
              .WithParsed<Options>(opts =>
              {
                  // Assign the parsed values to variables
                  raw_path = opts.raw_path;
                  batchSize = opts.batchSize;
                  n_threads = opts.threads;
              }).WithNotParsed<Options>(opts =>
              {
                    raw_path = args[0];
                    batchSize = 10000;
                    n_threads = 2;
              });
        
        string[] file_paths = GetFilePaths(raw_path);
        string? input_dir = Path.GetDirectoryName(file_paths[0]);
        if (input_dir == null)
        {   return; }

        string output_dir = buildOutputDir(input_dir);
        string[] output_paths = getOutputPaths(output_dir, file_paths);

        //Write .arrow file in small batches of 10,000 scans to avoid problems with memory consumption. 
        //const int batchSize = 10000; // Adjust this based on your memory constraints and performance needs
        ParallelOptions parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = n_threads // Limit to 4 threads/cores
        };
        //Convert .raw files 
        Console.WriteLine("batchSize: {0}", batchSize);
        Console.WriteLine("n_threads: {0}", n_threads);
        Parallel.ForEach(Enumerable.Range(0, file_paths.Length), parallelOptions, fileIndex =>
        {
            ProcessFile(file_paths[fileIndex], output_paths[fileIndex], batchSize);
        });        
    }
    public static string[] GetFilePaths(string raw_path)
    {
        //Initialize File Paths
        string[] file_paths;

        if (File.Exists(raw_path)) //Individual .raw file 
        {
            Console.WriteLine("Converting: {0}", Path.GetFileNameWithoutExtension(raw_path));
            file_paths = new string[] { raw_path };
        } else if (Directory.Exists(raw_path)) //All .raw files in a directory
        {   
            Console.WriteLine("Reading all .raw files from the directory: {0}", raw_path);
            string directory_path = raw_path;
            file_paths = Directory.GetFiles(directory_path, "*.raw", SearchOption.TopDirectoryOnly);
        } else
        {
            Console.WriteLine("File or Directory does not exist: {0}", raw_path);
            file_paths = new string[0];
        }
        return file_paths;
    }
    public static string buildOutputDir(string input_dir)
    {
        string output_dir = Path.Combine(input_dir, "arrow_out");
        Directory.CreateDirectory(output_dir);
        return output_dir;
    }
    public static string[] getOutputPaths(string output_dir, string[] file_paths)
    {
        //Make output paths by altering the file extension and directory 
        string[] output_paths = new string[file_paths.Length];
        for (var i = 0; i < file_paths.Length; i += 1) {
            string file_basename = Path.GetFileNameWithoutExtension(file_paths[i]);
            file_basename += ".arrow";
            output_paths[i] = Path.Combine(output_dir, file_basename);
        }
        return output_paths;
    }
    static void ProcessFile(string inputFile, string outputFile, int batchSize)
    {
        //var myThreadManager = RawFileReaderFactory.CreateThreadManager("/Users/n.t.wamsley/Desktop/20230324_OLEP08_200ng_30min_E20H50Y30_180K_2Th3p5ms_02.raw");
        //var rawFile = myThreadManager.CreateThreadAccessor();
        Console.WriteLine("Starting Conversion For: {0}", Path.GetFileNameWithoutExtension(inputFile));
        var rawFile = RawFileReaderAdapter.FileFactory(inputFile);
        if (!rawFile.IsOpen || rawFile.IsError)
        {
            // Check for any errors in the RAW file
            if (rawFile.IsError)
            {
                Console.WriteLine("Error opening ({0}) - {1}", rawFile.FileError.ErrorMessage, inputFile);
                rawFile.Dispose();
                return;
            }
            Console.WriteLine("Unable to access the RAW file using the RawFileReader class!");
            rawFile.Dispose();
            return;
        }
        //var rawFile = RawFileReaderAdapter.FileFactory(inputFile);

        // Get the number of instruments (controllers) present in the RAW file and set the 
        // selected instrument to the MS instrument, first instance of it
        //Console.WriteLine("The RAW file has data from {0} instruments" + rawFile.InstrumentCount);

        rawFile.SelectInstrument(Device.MS, 1);

        int firstScanNumber = rawFile.RunHeaderEx.FirstSpectrum;
        int lastScanNumber = rawFile.RunHeaderEx.LastSpectrum;
        // Build the ListArray
        var massField = new Field.Builder()
            .Name("masses")
            .DataType(new ListType(FloatType.Default))
            .Nullable(false)
            .Build();
        var intensityField = new Field.Builder()
            .Name("intensities")
            .DataType(new ListType(FloatType.Default))
            .Nullable(false)
            .Build();
        var scanHeaderField = new Field.Builder()
            .Name("scanHeader")
            .DataType(StringType.Default)
            .Nullable(false)
            .Build();
        var scanNumberField = new Field.Builder()
            .Name("scanNumber")
            .DataType(Int32Type.Default)
            .Nullable(false)
            .Build();
        var basePeakMassField = new Field.Builder()
            .Name("basePeakMass")
            .DataType(FloatType.Default)
            .Nullable(false)
            .Build();
        var basePeakIntensityField = new Field.Builder()
            .Name("basePeakIntensity")
            .DataType(FloatType.Default)
            .Nullable(false)
            .Build();
        var packetTypeField = new Field.Builder()
            .Name("packetType")
            .DataType(Int32Type.Default)
            .Nullable(false)
            .Build();
        var retentionTimeField = new Field.Builder()
            .Name("retentionTime")
            .DataType(FloatType.Default)
            .Nullable(false)
            .Build();
        var lowMassField = new Field.Builder()
            .Name("lowMass")
            .DataType(FloatType.Default)
            .Nullable(false)
            .Build();
        var highMassField = new Field.Builder()
            .Name("highMass")
            .DataType(FloatType.Default)
            .Nullable(false)
            .Build();
        var ticField = new Field.Builder()
            .Name("TIC")
            .DataType(FloatType.Default)
            .Nullable(false)
            .Build();
        var centerMassField = new Field.Builder()
            .Name("centerMass")
            .DataType(FloatType.Default)
            .Nullable(true)
            .Build();
        var isolationWidthField = new Field.Builder()
            .Name("isolationWidth")
            .DataType(FloatType.Default)
            .Nullable(true)
            .Build();
        var collisionEnergyField = new Field.Builder()
            .Name("collisionEnergyField")
            .DataType(FloatType.Default)
            .Nullable(true)
            .Build();
        var collisionEnergyEvField = new Field.Builder()
            .Name("collisionEnergyEvField")
            .DataType(FloatType.Default)
            .Nullable(true)
            .Build();
        var msOrderField = new Field.Builder()
            .Name("msOrder")
            .DataType(UInt8Type.Default)
            .Nullable(false)
            .Build();

        var schema = new Schema.Builder()
                            .Field(massField)
                            .Field(intensityField)
                            .Field(scanHeaderField)
                            .Field(scanNumberField)
                            .Field(basePeakMassField)
                            .Field(basePeakIntensityField)
                            .Field(packetTypeField)
                            .Field(retentionTimeField)
                            .Field(lowMassField)
                            .Field(highMassField)
                            .Field(ticField)
                            .Field(centerMassField)
                            .Field(isolationWidthField)
                            .Field(collisionEnergyField)
                            .Field(collisionEnergyEvField)
                            .Field(msOrderField)
                            .Build();
        // Get the start and end time from the RAW file
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();
                
        int totalScans = lastScanNumber - firstScanNumber + 1;
        //string[] scanHeader = new string[totalScans];
        using (var fileStream = new FileStream(outputFile, FileMode.Create))
        using (var writer = new Apache.Arrow.Ipc.ArrowFileWriter(fileStream, schema))
        {
            writer.WriteStartAsync().Wait();
            for (int batchStart = firstScanNumber; batchStart <= lastScanNumber; batchStart += batchSize)
            {
                int batchEnd = Math.Min(batchStart + batchSize - 1, totalScans);
                //Get Number of Mass Peaks in the Batch (used for pre-allocation)
                System.UInt64 batch_n_peaks = 0;
                for (int i = batchStart; i <= batchEnd; i++)
                {
                    batch_n_peaks += (ulong)rawFile.GetScanStatsForScanNumber(i)!.PacketCount;
                }
                //batch_n_peaks = (int)batch_n_peaks;
                //Mass and Intensity Lists
                var massListBuilder = new ListArray.Builder(FloatType.Default);
                var massValueBuilder = massListBuilder.ValueBuilder as FloatArray.Builder;
                var intensityListBuilder = new ListArray.Builder(FloatType.Default);
                var intensityValueBuilder = intensityListBuilder.ValueBuilder as FloatArray.Builder;
                //Scan Stats Fields 
                var scanHeaderBuilder = new StringArray.Builder();
                var scanNumberBuilder = new Int32Array.Builder();
                var basePeakMassBuilder = new FloatArray.Builder();
                var basePeakIntensityBuilder = new FloatArray.Builder();
                var packetTypeBuilder = new Int32Array.Builder();
                var retentionTimeBuilder = new FloatArray.Builder();
                var lowMassBuilder = new FloatArray.Builder();
                var highMassBuilder = new FloatArray.Builder();
                var ticBuilder = new FloatArray.Builder();
                //Scan Event Fields
                var centerMassBuilder = new FloatArray.Builder();
                var isolationWidthBuilder = new FloatArray.Builder();
                var collisionEnergyBuilder = new FloatArray.Builder();
                var collisionEnergyEvBuilder = new FloatArray.Builder();
                var msOrderBuilder = new UInt8Array.Builder();
                //Apache.ARrow.Types.StringType
                //Pre-Allocation 
                massListBuilder.Reserve(batchSize);
                intensityListBuilder.Reserve(batchSize);
                scanNumberBuilder.Reserve(batchSize);
                basePeakMassBuilder.Reserve(batchSize);
                basePeakIntensityBuilder.Reserve(batchSize);
                packetTypeBuilder.Reserve(batchSize);
                retentionTimeBuilder.Reserve(batchSize);
                lowMassBuilder.Reserve(batchSize);
                highMassBuilder.Reserve(batchSize);
                ticBuilder.Reserve(batchSize);
                centerMassBuilder.Reserve(batchSize);
                isolationWidthBuilder.Reserve(batchSize);
                collisionEnergyBuilder.Reserve(batchSize);
                collisionEnergyEvBuilder.Reserve(batchSize);
                msOrderBuilder.Reserve(batchSize);
                //scanHeaderBuilder.Reserve(batchSize);
                massValueBuilder?.Reserve((int)batch_n_peaks);
                intensityValueBuilder?.Reserve((int)batch_n_peaks);

                //Read batch of raw file 
                for (int i = batchStart; i <= batchEnd; i++)
                {
                    //Mass And Intensity Lists 
                    var scan = Scan.FromFile(rawFile, i);
                    massListBuilder.Append();
                    intensityListBuilder.Append();
                    for (int j = 0; j < scan.CentroidScan.Length; j++)
                    {
                        massValueBuilder?.Append((float)scan.CentroidScan.Masses[j]);
                        intensityValueBuilder?.Append((float)scan.CentroidScan.Intensities[j]);
                    }
                    //Scan Number
                    scanHeaderBuilder.Append(rawFile.GetFilterForScanNumber(i).ToString());
                    scanNumberBuilder.Append(i);

                    //Scan Stats Fields 
                    var scanStats = rawFile.GetScanStatsForScanNumber(i);
                    basePeakMassBuilder.Append((float)scanStats.BasePeakMass);
                    packetTypeBuilder.Append(scanStats.PacketType);
                    basePeakMassBuilder.Append((float)scanStats.BasePeakMass);
                    basePeakIntensityBuilder.Append((float)scanStats.BasePeakIntensity);
                    retentionTimeBuilder.Append((float)scanStats.StartTime);
                    lowMassBuilder.Append((float)scanStats.LowMass);
                    highMassBuilder.Append((float)scanStats.HighMass);
                    ticBuilder.Append((float)scanStats.TIC);

                    //Scan Event Fields 
                    var scanEvent = rawFile.GetScanEventForScanNumber(i);
                    var trailerData = rawFile.GetTrailerExtraInformation(i);
                    if ((byte)scanEvent.MSOrder > 1)
                    {
                        centerMassBuilder.Append((float)scanEvent.GetMass(0));
                        isolationWidthBuilder.Append((float)scanEvent.GetIsolationWidth(0) + (float)scanEvent.GetIsolationWidthOffset(0));
                        collisionEnergyBuilder.Append((float)scanEvent.GetEnergy(0));
                        
                        //Extra information fields. Useful for eV collision energy values
                        float ev = -1.0f;
                        for (int j = 0; j < trailerData.Length; j++)
                        {
                            if (trailerData.Labels[j] == "HCD Energy V:")
                            {
                                ev = Convert.ToSingle(trailerData.Values[j]);
                                break;
                            }
                        }
                        if (ev < 0)
                        {
                            collisionEnergyEvBuilder.AppendNull();
                        } else
                        {
                            collisionEnergyEvBuilder.Append((float)ev);
                        }

                    } else{
                        centerMassBuilder.AppendNull();
                        isolationWidthBuilder.AppendNull();
                        collisionEnergyBuilder.AppendNull();
                        collisionEnergyEvBuilder.AppendNull();
                    }

                    msOrderBuilder.Append((byte)scanEvent.MSOrder);



                }

                //Write Batch
                var massArray = massListBuilder.Build();
                var intensityArray = intensityListBuilder.Build();
                IArrowArray scanHeaderArray = scanHeaderBuilder.Build();
                IArrowArray scanNumberArray = scanNumberBuilder.Build();
                IArrowArray basePeakMassArray = basePeakMassBuilder.Build();
                IArrowArray basePeakIntensityArray = basePeakIntensityBuilder.Build();
                IArrowArray packetTypeArray = packetTypeBuilder.Build();
                IArrowArray retentionTimeArray = retentionTimeBuilder.Build();
                IArrowArray lowMassArray = lowMassBuilder.Build();
                IArrowArray highMassArray = highMassBuilder.Build();
                IArrowArray ticArray = ticBuilder.Build();
                IArrowArray centerMassArray = centerMassBuilder.Build();
                IArrowArray isolationWidthArray = isolationWidthBuilder.Build();
                IArrowArray collisionEnergyArray = collisionEnergyBuilder.Build();
                IArrowArray collisionEnergyEvArray = collisionEnergyEvBuilder.Build();
                IArrowArray msOrderArray = msOrderBuilder.Build();
                //var scanHeader = scanHeaderBuilder.Build();
                var recordBatch = new RecordBatch(schema, new[] { 
                    massArray, 
                    intensityArray, 
                    scanHeaderArray,
                    scanNumberArray,
                    basePeakMassArray,
                    basePeakIntensityArray,
                    packetTypeArray,
                    retentionTimeArray,
                    lowMassArray,
                    highMassArray,
                    ticArray,
                    centerMassArray,
                    isolationWidthArray,
                    collisionEnergyArray,
                    collisionEnergyEvArray,
                    msOrderArray }, batchEnd - batchStart + 1);
                writer.WriteRecordBatch(recordBatch);
            }
            writer.WriteEndAsync().Wait(); // Finish the Arrow file
        }
        watch.Stop();
        Console.WriteLine("Execution Time: {0} ms for {1}", watch.ElapsedMilliseconds, Path.GetFileNameWithoutExtension(inputFile));
        rawFile.Dispose();
    }
}