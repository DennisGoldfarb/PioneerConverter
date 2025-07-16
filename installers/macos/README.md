# macOS Installer

The `build_pkg.sh` script generates a `.pkg` installer using `pkgbuild`.
The installer places files under `/usr/local/PioneerConverter` and installs a wrapper script `PioneerConverter` into `/usr/local/bin` so the command is on your `PATH`.

## Building

Run the script on macOS with Xcode command line tools installed:

```bash
./build_pkg.sh
```

The result is either `PioneerConverter-arm64-<version>.pkg` or
`PioneerConverter-x64-<version>.pkg` depending on the architecture.
Replace `<version>` with the release tag.
