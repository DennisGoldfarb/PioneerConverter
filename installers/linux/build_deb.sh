#!/bin/bash
set -e

APPNAME="PioneerConverter"
VERSION="${VERSION:-1.0.0}"
# Architecture used inside the Debian control file
ARCH_DEB="amd64"
# Architecture label used for generated package filenames
ARCH_OUT="x64"
BUILD="debian"

rm -rf "$BUILD"
mkdir -p "$BUILD/DEBIAN"
mkdir -p "$BUILD/usr/local/$APPNAME"
mkdir -p "$BUILD/usr/local/bin"

cp -R ../../dist/PioneerConverter-linux-x64/* "$BUILD/usr/local/$APPNAME/"

cat <<WRAP > "$BUILD/usr/local/bin/PioneerConverter"
#!/bin/bash
  /usr/local/$APPNAME/bin/PioneerConverter "\$@"
WRAP
chmod +x "$BUILD/usr/local/bin/PioneerConverter"

cat <<CTRL > "$BUILD/DEBIAN/control"
Package: pioneerconverter
Version: $VERSION
Section: utils
Priority: optional
Architecture: $ARCH_DEB
Maintainer: edu.washu.goldfarblab.pioneerconverter
Description: PioneerConverter command line tool
CTRL

dpkg-deb --build "$BUILD" "${APPNAME}-linux-${ARCH_OUT}-${VERSION}.deb"

echo "Package created: ${APPNAME}-linux-${ARCH_OUT}-${VERSION}.deb"
