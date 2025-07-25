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

# Determine version from env or latest Git tag
VERSION="${VERSION:-$(git describe --tags --abbrev=0 2>/dev/null || echo "0.0.0")}"
# Remove a leading 'v' and any trailing newline
VERSION="${VERSION#v}"
VERSION="$(echo "$VERSION" | tr -d '\n')"
export VERSION

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
        iscc "PioneerConverter.iss"
        output_file="$(ls Output/PioneerConverter-win-*-Setup.exe 2>/dev/null | head -n1)"
        if [ -n "$output_file" ]; then
            mv "$output_file" "PioneerConverter-win-${VERSION}-Setup.exe"
            echo "first move"
        elif [ -f Output/PioneerConverter-win-Setup.exe ]; then
            mv Output/PioneerConverter-win-Setup.exe "PioneerConverter-win-${VERSION}-Setup.exe"
            echo "second move"
            echo "PioneerConverter-win-${VERSION}-Setup.exe"
        fi
        popd > /dev/null
        ;;
esac

