# Windows Installer

The `PioneerConverter.iss` script can be built with [Inno Setup](https://jrsoftware.org/isinfo.php) to produce a GUI installer.

## Building

1. Install Inno Setup.
2. Open `PioneerConverter.iss` in the Inno Setup Compiler or run
   `iscc "PioneerConverter.iss"` from the command line.
3. Rename the produced file in the `Output` directory to
   `PioneerConverter-win-<version>-Setup.exe`, replacing `<version>` with the
   release tag.

The installer places the application in `Program Files\\PioneerConverter` and
optionally adds the directory to your system `PATH`.  Because both of these
actions modify system-wide locations, you must run the installer with
administrator privileges.
