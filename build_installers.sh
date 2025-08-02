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

# Build binaries for all platforms so macOS precompiled binaries are released
./build.sh all

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
        echo "Debug: iscc path: $(command -v iscc)"
        export MSYS2_ARG_CONV_EXCL="/D*"
        echo "Debug: MSYS2_ARG_CONV_EXCL=${MSYS2_ARG_CONV_EXCL}"
        echo "Debug: running iscc with /DMyAppVersion=${VERSION} PioneerConverter.iss via cmd.exe"
        echo "Debug: directory contents:" && ls -1
        cmd.exe //C "iscc /DMyAppVersion=${VERSION} PioneerConverter.iss"
        mv "Output/PioneerConverter-win-${VERSION}-Setup.exe" "PioneerConverter-win-${VERSION}-Setup.exe"
        popd > /dev/null
        ;;
esac

