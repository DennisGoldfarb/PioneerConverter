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

   Or build only for your current platform:
   ```bash
   ./build.sh macos    # or linux / windows
   ```

## Building Installers

Installer scripts are provided for Windows, macOS and Linux under the
`installers/` directory.  These scripts package the binaries produced by
`build.sh` into native installers and place a wrapper on the system `PATH`.

### Windows

Use [Inno Setup](https://jrsoftware.org/isinfo.php) to compile
`installers/windows/PioneerConverter.iss`.  The resulting
`PioneerConverter-Setup.exe` installs the application under
`Program Files` and can optionally add the directory to your `PATH`.

### macOS

Run `installers/macos/build_pkg.sh` on a Mac with Xcode command line tools.
If the environment variables `CODESIGN_IDENTITY` and `PKG_SIGN_IDENTITY` are
set, the script will codesign the binary using the entitlements in
`installers/macos/entitlements.plist` and sign the resulting installer.
This entitlements file is required for running the signed binaries.
The script produces `PioneerConverter.pkg` which installs files to
`/usr/local/PioneerConverter` and symlinks the `pioneerconverter` command to
`/usr/local/bin`.

### Linux

Run `installers/linux/build_deb.sh` on a Debian-based system to create a
`PioneerConverter_1.0.0_amd64.deb` package.  Installing this package places the
wrapper script in `/usr/local/bin` and the application files under
`/usr/local/PioneerConverter`.

### Automated Builds

Run `./build_installers.sh` to build the binaries and create the installer
for the current platform.  A GitHub Actions workflow (`build.yml`) executes
this script on Windows, macOS and Linux whenever a version tag is pushed and
publishes the installer packages to the project packages section and uploads
the zipped binaries for each platform as release assets.
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