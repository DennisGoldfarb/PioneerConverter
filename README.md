<p align="center">
  <img src="assets/Converter.svg" alt="PioneerConverter Logo" width="256" height="256"/>
</p>

# PioneerConverter

Convert Thermo `.raw` files into [Apache Arrow](https://arrow.apache.org/) tables ready for [Pioneer](https://github.com/nwamsley1/Pioneer.jl). The tool uses Thermo's [RawFileReader](https://github.com/thermofisherlsms/RawFileReader) and runs on Windows, macOS and Linux.

## Install

Grab an installer or zipped binary from the project releases. Installers place a `PioneerConverter` command on your `PATH`. Zip archives can be extracted anywhere. If using a zipped binary on macOS or Linux run `chmod +x PioneerConverter` before executing.

## Usage

Provide a single `.raw` file or a directory containing them. Converted Arrow tables are written to `arrow_out` inside the input directory.

```bash
# convert a single file
PioneerConverter path/to/file.raw

# convert a directory with options
PioneerConverter path/to/dir -b 5000 -n 4
```

Options
- `-b, --batch-size`  number of scans per batch (default: 10000)
- `-n, --threads`     threads to use (default: 2)
- `-h, --help`        show help

## Build from source

1. Install [.NET&nbsp;8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
2. Clone this repository and run:

```bash
chmod +x build.sh
./build.sh          # or ./build.sh macos|linux|windows
```

## Output format

Each row in the Arrow table corresponds to a scan in the `.raw` file:

| Column Name | Data Type | Description |
|------------|-----------|-------------|
| mz_array | Vector{Union{Missing, Float32}} | List of masses for peaks in the centroided spectra |
| intensity_array | Vector{Union{Missing, Float32}} | List of intensities for peaks in the centroided spectra |
| scanHeader | String | A description of the scan |
| scanNumber | Int32 | Scan index of i'th scan in the `.raw` file |
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
