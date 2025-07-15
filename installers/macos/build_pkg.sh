#!/bin/bash
set -e

APPNAME="PioneerConverter"
VERSION="1.0.0"
PKGROOT="pkgroot"

rm -rf "$PKGROOT"
mkdir -p "$PKGROOT/usr/local/$APPNAME"
mkdir -p "$PKGROOT/usr/local/bin"

cp -R ../../dist/PioneerConverter-osx-x64/* "$PKGROOT/usr/local/$APPNAME/"

cat <<'WRAP' > "$PKGROOT/usr/local/bin/pioneerconverter"
#!/bin/bash
/usr/local/$APPNAME/PioneerConverter/PioneerConverter "$@"
WRAP
chmod +x "$PKGROOT/usr/local/bin/pioneerconverter"

pkgbuild --root "$PKGROOT" \
  --identifier "com.example.pioneerconverter" \
  --version "$VERSION" \
  --install-location "/" "$APPNAME.pkg"

echo "Package created: $APPNAME.pkg"
