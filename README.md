# PioneerConverter

### Cross Platform Conversion of Thermo Raw Files to Pioneer Compatible Apache Arrow Tables

Uses [Thermo RawFileReader .NET Assemblies](https://github.com/thermofisherlsms/RawFileReader) to convert Thermo '.raw' files to the [Apache Arrow](https://arrow.apache.org/) format. 
Converts either an indivdual '.raw' file or all '.raw' files in a given directory. Output tables are ready to search with [Pioneer](https://github.com/nwamsley1/Pioneer.jl).

# Instalation

Download .NET 8.0 SDK and .NET Runtime 8.0 [here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

###### Using Release
1) Download and decompress the latest release for your operating system.

###### Build
1) Clone the PioneerConverter Repository
2) Inside the PioneerConverter/ directory run
```
dotnet build -c Release
```

# Usage 
The input can either be a path to a single .raw file or a directory containing .raw files. In the later case all .raw files in the directory are converted. The output files
are written into a new directory 'arrow_out' created within the input directory. 

1) <b>-n</b> <b>--threads</b> flag specifies the number of threads to use. Defaults to 2. 
2) <b>-b</b> <b>--batch-size</b> batch size. Number of scans to convert per-batch. Setting to high will cause significant memory allocation. Defaults to 10000
3) <b>-o</b> path to folder where the converted files will be saved. Defaults to 'arrow_out' directory within the directory of the input. 

###### POSIX
```
bin/Release/net8.0/PioneerConverter /path/to/raw/file.raw -b 5000

bin/Release/net8.0/PioneerConverter /directory/containing/raw/files/ -b 5000
```

###### Windows
```
cmd bin\Release\net8.0\PioneerConverter.exe \path\to\raw\file.raw -b 5000

cmd bin\Release\net8.0\PioneerConverter.exe \directory\containing\raw\files\ -b 5000
```

# Output 

 The output files have the following fields with one entry per scan in the *.raw file. 
 |Name                |Type                |Description                    |
 |--------------------|--------------------|--------------------|
 |masses              | SubArray{Union{Missing, Float32}, 1, Arrow.Primitive{Union{Missing, Float32}, Vector{Float32}}, Tuple{UnitRange{Int64}}, true}|List of masses for peaks in the centroided <br> spectra in ascending order
 |intensities         | SubArray{Union{Missing, Float32}, 1, Arrow.Primitive{Union{Missing, Float32}, Vector{Float32}}, Tuple{UnitRange{Int64}}, true}|List of intensities for peaks in the centroiede <br> spectra. Corresponds with the peaks in `masses`
 |scanHeader |String| Scan Header:  "FTMS + c NSI Full ms [375.0000-1000.0000]"
 |scanNumber          |Int32|Scan index of i'th scan in the .*raw file in order of occurence
 |basePeakMass        |Float32|Mass of the most intense peak in the spectrum 
 |basePeakIntensity   |Float32|Intensity of the most intense peak in the spectrum
 |packetType          |Int32|
 |retentionTime       |Float32|Retention time recorded for the scan 
 |lowMass             |Float32|First mass in the scan
 |highMass            |Float32|Last mass in the scan
 |TIC                 |Float32|Summed intensity of all peaks in the scan
 |centerMass             |Union{Missing, Float32}|Precursor MZ or the center of the isolation window for an MSN scan <br> If no precursor was assigned. Missing for an MS1 scan.
 |isolationWidth    |Union{Missing, Float32}|Width of quadrupole isolation window. Missing for MS1 scan
 |collisionEnergyField            |Union{Missing, Float32}| NCE collision energy
 |msOrder             |UInt8|As in MS1, MS2, or MSN scan. Is "2" for an MS2 scan.
 

# Notes/Future Work

 1) Currently does not handle non-centroided data. The Thermo DLLs have functionality to centroid profile mode scans, so this can be fixed.
 2) Conversion to other formats?
 3) Bug Fixes
