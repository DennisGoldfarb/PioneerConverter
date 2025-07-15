# Linux Installer

The `build_deb.sh` script creates a Debian package that installs the binaries under `/usr/local/PioneerConverter` and places a wrapper script `pioneerconverter` in `/usr/local/bin`.

## Building

Run the script on a Debian-based system:

```bash
./build_deb.sh
```

The resulting `PioneerConverter_1.0.0_amd64.deb` can be installed with `dpkg -i`.
