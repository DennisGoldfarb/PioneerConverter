#!/bin/bash
set -e

OS="$(uname)"
case "$OS" in
    Linux*)
        TARGET=linux
        ;;
    Darwin*)
        TARGET=macos
        ;;
    MINGW*|MSYS*|CYGWIN*|Windows_NT)
        TARGET=windows
        ;;
    *)
        echo "Unsupported OS: $OS" >&2
        exit 1
        ;;
esac

# Build binaries only for the current platform
./build.sh "$TARGET"

case "$TARGET" in
    linux)
        echo "Creating Linux .deb package"
        pushd installers/linux > /dev/null
        ./build_deb.sh
        popd > /dev/null
        ;;
    macos)
        echo "Creating macOS .pkg installers"
        pushd installers/macos > /dev/null
        PKG_ARCH=arm64 ./build_pkg.sh
        PKG_ARCH=x64 ./build_pkg.sh
        popd > /dev/null
        ;;
    windows)
        echo "Creating Windows installer"
        pushd installers/windows > /dev/null
        iscc PioneerConverter.iss
        if [ -f Output/PioneerConverter-Setup.exe ]; then
            mv Output/PioneerConverter-Setup.exe PioneerConverter-Setup.exe
        fi
        popd > /dev/null
        ;;
esac

