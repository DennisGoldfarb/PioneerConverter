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
  --install-location "/" "${APPNAME}-unsigned.pkg"

if [[ -n "$CODESIGN_IDENTITY" ]]; then
  echo "Codesigning binaries"
  codesign --force --options runtime --timestamp \
    --sign "$CODESIGN_IDENTITY" \
    "$PKGROOT/usr/local/$APPNAME/PioneerConverter/PioneerConverter"
fi

if [[ -n "$PKG_SIGN_IDENTITY" ]]; then
  echo "Signing package"
  productsign --sign "$PKG_SIGN_IDENTITY" \
    "${APPNAME}-unsigned.pkg" "$APPNAME.pkg"
  rm "${APPNAME}-unsigned.pkg"
else
  mv "${APPNAME}-unsigned.pkg" "$APPNAME.pkg"
fi

echo "Package created: $APPNAME.pkg"
