#!/bin/bash
set -e

# Build cross-platform binaries
./build.sh

OS="$(uname)"
case "$OS" in
    Linux*)
        echo "Creating Linux .deb package"
        installers/linux/build_deb.sh
        ;;
    Darwin*)
        echo "Creating macOS .pkg installer"
        installers/macos/build_pkg.sh
        ;;
    MINGW*|MSYS*|CYGWIN*|Windows_NT)
        echo "Creating Windows installer"
        iscc installers/windows/PioneerConverter.iss
        ;;
    *)
        echo "Unsupported OS: $OS" >&2
        exit 1
        ;;
esac

