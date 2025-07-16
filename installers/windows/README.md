# Windows Installer

The `PioneerConverter.iss` script can be built with [Inno Setup](https://jrsoftware.org/isinfo.php) to produce a GUI installer.

## Building

1. Install Inno Setup.
2. Open `PioneerConverter.iss` in the Inno Setup Compiler.
3. Compile the script to generate `PioneerConverter-win-<version>-Setup.exe`,
   where `<version>` matches the release tag.

The installer places the application in `Program Files\\PioneerConverter` and optionally adds the directory to your `PATH`.
