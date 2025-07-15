#!/bin/bash

# Function to print step information
print_step() {
    echo "-------------------------"
    echo "Step: $1"
    echo "-------------------------"
}

# Create dist directory if it doesn't exist
mkdir -p dist

# Build for macOS ARM64 (M1/M2)
print_step "Building for macOS ARM64"
dotnet publish PioneerConverter.csproj -c Release \
  -r osx-arm64 \
  -p:PublishSingleFile=false \
  -p:PublishTrimmed=false \
  --self-contained true \
  -o dist/PioneerConverter-osx-arm64

# Build for macOS x64 (Intel)
print_step "Building for macOS x64"
dotnet publish PioneerConverter.csproj -c Release \
  -r osx-x64 \
  -p:PublishSingleFile=false \
  -p:PublishTrimmed=false \
  --self-contained true \
  -o dist/PioneerConverter-osx-x64

# Build for Linux x64
print_step "Building for Linux x64"
dotnet publish PioneerConverter.csproj -c Release \
  -r linux-x64 \
  -p:PublishSingleFile=false \
  -p:PublishTrimmed=false \
  --self-contained true \
  -o dist/PioneerConverter-linux-x64

# Build for Windows x64
print_step "Building for Windows x64"
dotnet publish PioneerConverter.csproj -c Release \
  -r win-x64 \
  -p:PublishSingleFile=false \
  -p:PublishTrimmed=false \
  --self-contained true \
  -o dist/PioneerConverter-win-x64

# Make executables runnable on Unix-like systems
chmod +x dist/PioneerConverter-osx-arm64/PioneerConverter
chmod +x dist/PioneerConverter-osx-x64/PioneerConverter
chmod +x dist/PioneerConverter-linux-x64/PioneerConverter

# Create zip archives
print_step "Creating zip archives"
cd dist
zip -r PioneerConverter-osx-arm64.zip PioneerConverter-osx-arm64
zip -r PioneerConverter-osx-x64.zip PioneerConverter-osx-x64
zip -r PioneerConverter-linux-x64.zip PioneerConverter-linux-x64
zip -r PioneerConverter-win-x64.zip PioneerConverter-win-x64
cd ..

echo "Build complete! Check the dist directory for the output files."