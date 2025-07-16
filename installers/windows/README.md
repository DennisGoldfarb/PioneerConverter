# Windows Installer

The `PioneerConverter.iss` script can be built with [Inno Setup](https://jrsoftware.org/isinfo.php) to produce a GUI installer.

## Building

1. Install Inno Setup.
2. Open `PioneerConverter.iss` in the Inno Setup Compiler or run
   `iscc "/DMyAppVersion=<version>" "PioneerConverter.iss"` from the command line,
   replacing `<version>` with the release tag.
3. The output is `PioneerConverter-win-<version>-Setup.exe`.

The installer places the application in `Program Files\\PioneerConverter` and optionally adds the directory to your `PATH`.
