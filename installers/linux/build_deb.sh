#!/bin/bash
set -e

APPNAME="PioneerConverter"
VERSION="${VERSION:-1.0.0}"
ARCH="amd64"
BUILD="debian"

rm -rf "$BUILD"
mkdir -p "$BUILD/DEBIAN"
mkdir -p "$BUILD/usr/local/$APPNAME"
mkdir -p "$BUILD/usr/local/bin"

cp -R ../../dist/PioneerConverter-linux-x64/* "$BUILD/usr/local/$APPNAME/"

cat <<WRAP > "$BUILD/usr/local/bin/PioneerConverter"
#!/bin/bash
/usr/local/$APPNAME/PioneerConverter "\$@"
WRAP
chmod +x "$BUILD/usr/local/bin/PioneerConverter"

cat <<CTRL > "$BUILD/DEBIAN/control"
Package: pioneerconverter
Version: $VERSION
Section: utils
Priority: optional
Architecture: $ARCH
Maintainer: Unknown
Description: PioneerConverter command line tool
CTRL

dpkg-deb --build "$BUILD" "${APPNAME}-linux_${VERSION}_${ARCH}.deb"

echo "Package created: ${APPNAME}-linux_${VERSION}_${ARCH}.deb"
