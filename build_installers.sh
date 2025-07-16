#!/bin/bash
set -e

# Build cross-platform binaries
./build.sh

OS="$(uname)"
case "$OS" in
    Linux*)
        echo "Creating Linux .deb package"
        pushd installers/linux > /dev/null
        ./build_deb.sh
        popd > /dev/null
        ;;
    Darwin*)
        echo "Creating macOS .pkg installers"
        pushd installers/macos > /dev/null
        PKG_ARCH=arm64 ./build_pkg.sh
        PKG_ARCH=x64 ./build_pkg.sh
        popd > /dev/null
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

