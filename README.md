# PioneerConverter

### Cross Platform Conversion of Thermo Raw Files to Pioneer Compatible Apache Arrow Tables

Uses [Thermo RawFileReader .NET Assemblies](https://github.com/thermofisherlsms/RawFileReader) to convert Thermo '.raw' files to the [Apache Arrow](https://arrow.apache.org/) format. 
Converts either an individual '.raw' file or all '.raw' files in a given directory. Output tables are ready to search with [Pioneer](https://github.com/nwamsley1/Pioneer.jl).

## Installation

Platform specific installers and zipped binaries are available from the
project releases.  The installers place the application in a standard
location and put a `PioneerConverter` wrapper script on your `PATH` so
the command is available system&#8209;wide.

### Using the installers

- **Windows**: run `PioneerConverter-win-1.2.3-Setup.exe` (replace `1.2.3`
  with the release tag).  The program is
  installed under *Program&nbsp;Files* and a `PioneerConverter` command
  is optionally added to your `PATH`.
- **macOS**: open `PioneerConverter-<arch>-1.2.3.pkg` (replace `1.2.3` with the
  release tag).  This installs the files in
  `/usr/local/PioneerConverter` and links the `PioneerConverter` command
  to `/usr/local/bin`.
- **Linux**: install the `.deb` package, e.g.
  ```bash
  sudo dpkg -i PioneerConverter-linux_1.2.3_amd64.deb
  # replace 1.2.3 with the release tag
  ```
  This also installs the wrapper command to `/usr/local/bin`.

After installation you can simply run:

```bash
PioneerConverter <input path>
```

### Using the zipped binaries

If you prefer a portable copy download one of the zip files:

- macOS M1/M2 (ARM64): `PioneerConverter-osx-arm64.zip`
- macOS Intel (x64): `PioneerConverter-osx-x64.zip`
- Linux (x64): `PioneerConverter-linux-x64.zip`
- Windows (x64): `PioneerConverter-win-x64.zip`

Extract the archive and run the `PioneerConverter` executable from the
extracted directory.  On macOS and Linux you may need to make the file
executable first:

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

   Or build only for your current platform:
   ```bash
   ./build.sh macos    # or linux / windows
   ```

## Output Format

The output files have the following fields with one entry per scan in the *.raw file:

| Column Name | Data Type | Description |
|------------|-----------|-------------|
| mz_array | Vector{Union{Missing, Float32}} | List of masses for peaks in the centroided spectra |
| intensity_array | Vector{Union{Missing, Float32}} | List of intensities for peaks in the centroided spectra |
| scanHeader | String | A description of the scan |
| scanNumber | Int32 | Scan index of i'th scan in the .*raw file in order of occurrence |
| basePeakMz | Float32 | m/z of the base peak |
| basePeakIntensity | Float32 | Intensity of the base peak |
| packetType | Int32 | |
| retentionTime | Float32 | Retention time of the scan |
| lowMz | Float32 | Lowest m/z in the scan range |
| highMz | Float32 | Highest m/z in the scan range |
| TIC | Float32 | Total ion current |
| centerMz | Union{Float32, Missing} | For an MSn scan, the m/z center of the quadrupole isolation window |
| isolationWidthMz | Union{Float32, Missing} | For an MSn scan, the m/z width of the quadrupole isolation window |
| collisionEnergyField | Union{Float32, Missing} | Normalized collision energy |
| collisionEnergyEvField | Union{Float32, Missing} | Collision energy (eV) |
| msOrder | UInt8 | The n for an MSn scan |