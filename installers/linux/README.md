# Linux Installer

The `build_deb.sh` script creates a Debian package that installs the binaries under `/usr/local/PioneerConverter` and places a wrapper script `PioneerConverter` in `/usr/local/bin`.

## Building

Run the script on a Debian-based system:

```bash
./build_deb.sh
```

The resulting `PioneerConverter-linux_<version>_amd64.deb` can be installed
with `dpkg -i` where `<version>` is the release tag.
