#!/bin/bash
set -e

APPNAME="PioneerConverter"
VERSION="1.0.0"
PKGROOT="pkgroot"
PKG_ARCH="${PKG_ARCH:-$(uname -m)}"

case "$PKG_ARCH" in
  arm64|aarch64)
    DIST="../../dist/${APPNAME}-osx-arm64"
    PKGFILE="${APPNAME}-arm64.pkg"
    ;;
  x64|x86_64)
    DIST="../../dist/${APPNAME}-osx-x64"
    PKGFILE="${APPNAME}-x64.pkg"
    ;;
  *)
    echo "Unsupported architecture: $PKG_ARCH" >&2
    exit 1
    ;;
esac

rm -rf "$PKGROOT"
mkdir -p "$PKGROOT/usr/local/$APPNAME"
mkdir -p "$PKGROOT/usr/local/bin"

cp -R "$DIST"/* "$PKGROOT/usr/local/$APPNAME/"

cat <<WRAP > "$PKGROOT/usr/local/bin/pioneerconverter"
#!/bin/bash
/usr/local/$APPNAME/PioneerConverter "\$@"
WRAP
chmod +x "$PKGROOT/usr/local/bin/pioneerconverter"

if [[ -n "$CODESIGN_IDENTITY" ]]; then
  echo "Codesigning binaries"
  while IFS= read -r -d '' file; do
      if file "$file" | grep -q 'Mach-O'; then
        codesign --verbose=4 --force --options runtime --timestamp \
          --entitlements "$(dirname "$0")/entitlements.plist" \
          --sign "$CODESIGN_IDENTITY" "$file"
      fi
    done < <(find "$PKGROOT/usr/local/$APPNAME" -type f -print0)
fi

UNSIGNED="${PKGFILE%.pkg}-unsigned.pkg"

pkgbuild --root "$PKGROOT" \
  --identifier "com.example.pioneerconverter" \
  --version "$VERSION" \
  --install-location "/" "$UNSIGNED"

if [[ -n "$PKG_SIGN_IDENTITY" ]]; then
  echo "Signing package"
  productsign --sign "$PKG_SIGN_IDENTITY" \
    "$UNSIGNED" "$PKGFILE"
  rm "$UNSIGNED"
else
  mv "$UNSIGNED" "$PKGFILE"
fi

echo "Package created: $PKGFILE"
