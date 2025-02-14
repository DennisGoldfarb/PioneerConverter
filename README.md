# PioneerConverter

### Cross Platform Conversion of Thermo Raw Files to Pioneer Compatible Apache Arrow Tables

Uses [Thermo RawFileReader .NET Assemblies](https://github.com/thermofisherlsms/RawFileReader) to convert Thermo '.raw' files to the [Apache Arrow](https://arrow.apache.org/) format. 
Converts either an individual '.raw' file or all '.raw' files in a given directory. Output tables are ready to search with [Pioneer](https://github.com/nwamsley1/Pioneer.jl).

## Installation

1. Download the appropriate version for your system:
   - macOS M1/M2 (ARM64): `PioneerConverter-osx-arm64.zip`
   - macOS Intel (x64): `PioneerConverter-osx-x64.zip`
   - Linux (x64): `PioneerConverter-linux-x64.zip`
   - Windows (x64): `PioneerConverter-win-x64.zip`

2. Extract the zip file:
   ```bash
   unzip PioneerConverter-*-*.zip
   ```

3. Make the executable runnable (macOS/Linux only):
   ```bash
   chmod +x PioneerConverter-*/PioneerConverter
   ```

## Usage

The input can be either a path to a single .raw file or a directory containing .raw files. In the latter case, all .raw files in the directory are converted. The output files are written into a new directory 'arrow_out' created within the input directory.

Options:
- `-b, --batch-size`: Number of scans to convert per batch (default: 10000)
- `-n, --threads`: Number of threads to use (default: 2)
- `-h, --help`: Show help information

### Examples

Convert a single file:
```bash
./PioneerConverter path/to/raw/file.raw

# With options
./PioneerConverter path/to/raw/file.raw -b 5000 -n 4
```

Convert all files in a directory:
```bash
./PioneerConverter path/to/directory/with/raw/files/

# With options
./PioneerConverter path/to/directory/with/raw/files/ -b 5000 -n 4
```

## Building from Source

1. Prerequisites:
   - Install [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

2. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/PioneerConverter.git
   cd PioneerConverter
   ```

3. Build for all platforms:
   ```bash
   chmod +x build.sh
   ./build.sh
   ```

   Or build for a specific platform:
   ```bash
   dotnet publish -c Release \
     -r osx-arm64 \
     -p:PublishSingleFile=false \
     -p:PublishTrimmed=false \
     --self-contained true \
     -o dist/PioneerConverter-osx-arm64
   ```

## Output Format

The output files have the following fields with one entry per scan in the *.raw file:

| Name | Type | Description |
|------|------|-------------|
| masses | Float32[] | List of masses for peaks in the centroided spectra in ascending order |
| intensities | Float32[] | List of intensities for peaks in the centroided spectra |
| scanHeader | String | Scan Header (e.g., "FTMS + c NSI Full ms [375.0000-1000.0000]") |
| scanNumber | Int32 | Scan index in the .raw file |
| basePeakMz | Float32 | Mass of the most intense peak |
| basePeakIntensity | Float32 | Intensity of the most intense peak |
| packetType | Int32 | Type of spectrum packet |
| retentionTime | Float32 | Retention time in minutes |
| lowMz | Float32 | First mass in the scan |
| highMz | Float32 | Last mass in the scan |
| TIC | Float32 | Total Ion Current (summed intensity) |
| centerMz | Float32? | Precursor MZ for MSn scans (null for MS1) |
| isolationWidth | Float32? | Width of quadrupole isolation window (null for MS1) |
| collisionEnergy | Float32? | NCE collision energy (null for MS1) |
| msOrder | UInt8 | MS level (1 for MS1, 2 for MS2, etc.) |