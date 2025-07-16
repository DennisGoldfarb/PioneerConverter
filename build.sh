#!/bin/bash
set -e

# Function to print step information
print_step() {
    echo "-------------------------"
    echo "Step: $1"
    echo "-------------------------"
}

TARGET_OS="${1:-all}"

# Create dist directory if it doesn't exist
mkdir -p dist

build_macos() {
    print_step "Building for macOS ARM64"
    dotnet publish PioneerConverter.csproj -c Release \
      -r osx-arm64 \
      -p:PublishSingleFile=false \
      -p:PublishTrimmed=false \
      --self-contained true \
      -o dist/PioneerConverter-osx-arm64

    print_step "Building for macOS x64"
    dotnet publish PioneerConverter.csproj -c Release \
      -r osx-x64 \
      -p:PublishSingleFile=false \
      -p:PublishTrimmed=false \
      --self-contained true \
      -o dist/PioneerConverter-osx-x64

    chmod +x dist/PioneerConverter-osx-arm64/PioneerConverter
    chmod +x dist/PioneerConverter-osx-x64/PioneerConverter
}

build_linux() {
    print_step "Building for Linux x64"
    dotnet publish PioneerConverter.csproj -c Release \
      -r linux-x64 \
      -p:PublishSingleFile=false \
      -p:PublishTrimmed=false \
      --self-contained true \
      -o dist/PioneerConverter-linux-x64

    chmod +x dist/PioneerConverter-linux-x64/PioneerConverter
}

build_windows() {
    print_step "Building for Windows x64"
    dotnet publish PioneerConverter.csproj -c Release \
      -r win-x64 \
      -p:PublishSingleFile=false \
      -p:PublishTrimmed=false \
      --self-contained true \
      -o dist/PioneerConverter-win-x64
}

BUILT=()

case "$TARGET_OS" in
    macos)
        build_macos
        BUILT+=(PioneerConverter-osx-arm64 PioneerConverter-osx-x64)
        ;;
    linux)
        build_linux
        BUILT+=(PioneerConverter-linux-x64)
        ;;
    windows)
        build_windows
        BUILT+=(PioneerConverter-win-x64)
        ;;
    all)
        build_macos
        build_linux
        build_windows
        BUILT+=(PioneerConverter-osx-arm64 PioneerConverter-osx-x64 \
               PioneerConverter-linux-x64 PioneerConverter-win-x64)
        ;;
    *)
        echo "Unknown target OS: $TARGET_OS" >&2
        exit 1
        ;;
esac

print_step "Creating zip archives"
cd dist
for dir in "${BUILT[@]}"; do
    if command -v zip >/dev/null 2>&1; then
        zip -r "$dir.zip" "$dir"
    elif command -v 7z >/dev/null 2>&1; then
        7z a "$dir.zip" "$dir" >/dev/null
    elif command -v powershell.exe >/dev/null 2>&1; then
        powershell.exe -Command "Compress-Archive -Path '$dir' -DestinationPath '$dir.zip'" >/dev/null
    else
        echo "No zip utility found" >&2
        exit 1
    fi
done
cd ..

echo "Build complete! Check the dist directory for the output files."