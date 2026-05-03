#!/usr/bin/env bash

set -e
set -o
set -u
set pipefail

arch=
appimage_arch=
target=
case "$RUNTIME" in
    linux-x64)
        arch=amd64
        appimage_arch=x86_64
        target=x86_64;;
    linux-arm64)
        arch=arm64
        appimage_arch=arm_aarch64
        target=aarch64;;
    *)
        echo "Unknown runtime $RUNTIME"
        exit 1;;
esac

APPIMAGETOOL_URL=https://github.com/AppImage/appimagetool/releases/download/continuous/appimagetool-x86_64.AppImage

cd build

if [[ ! -f "appimagetool" ]]; then
    curl -o appimagetool -L "$APPIMAGETOOL_URL"
    chmod +x appimagetool
fi

rm -f Suity.Agentic/*.dbg
rm -f Suity.Agentic/*.pdb

mkdir -p Suity.Agentic.AppDir/opt
mkdir -p Suity.Agentic.AppDir/usr/share/metainfo
mkdir -p Suity.Agentic.AppDir/usr/share/applications

cp -r Suity.Agentic Suity.Agentic.AppDir/opt/suity-agentic
desktop-file-install resources/_common/applications/suity-agentic.desktop --dir Suity.Agentic.AppDir/usr/share/applications \
    --set-icon com.suity.SuityAgentic --set-key=Exec --set-value=AppRun
mv Suity.Agentic.AppDir/usr/share/applications/{suity-agentic,com.suity.SuityAgentic}.desktop
cp resources/_common/icons/suity-agentic.png Suity.Agentic.AppDir/com.suity.SuityAgentic.png
ln -rsf Suity.Agentic.AppDir/opt/suity-agentic/suity-agentic Suity.Agentic.AppDir/AppRun
ln -rsf Suity.Agentic.AppDir/usr/share/applications/com.suity.SuityAgentic.desktop Suity.Agentic.AppDir
cp resources/appimage/suity-agentic.appdata.xml Suity.Agentic.AppDir/usr/share/metainfo/com.suity.SuityAgentic.appdata.xml

ARCH="$appimage_arch" ./appimagetool -v Suity.Agentic.AppDir "suity-agentic-$VERSION.linux.$arch.AppImage"

mkdir -p resources/deb/opt/suity-agentic/
mkdir -p resources/deb/usr/bin
mkdir -p resources/deb/usr/share/applications
mkdir -p resources/deb/usr/share/icons
cp -f Suity.Agentic/* resources/deb/opt/suity-agentic
ln -rsf resources/deb/opt/suity-agentic/suity-agentic resources/deb/usr/bin
cp -r resources/_common/applications resources/deb/usr/share
cp -r resources/_common/icons resources/deb/usr/share
# Calculate installed size in KB
installed_size=$(du -sk resources/deb | cut -f1)
# Update the control file
sed -i -e "s/^Version:.*/Version: $VERSION/" \
    -e "s/^Architecture:.*/Architecture: $arch/" \
    -e "s/^Installed-Size:.*/Installed-Size: $installed_size/" \
    resources/deb/DEBIAN/control
# Build deb package with gzip compression
dpkg-deb -Zgzip --root-owner-group --build resources/deb "suity-agentic_$VERSION-1_$arch.deb"

rpmbuild -bb --target="$target" resources/rpm/SPECS/build.spec --define "_topdir $(pwd)/resources/rpm" --define "_version $VERSION"
mv "resources/rpm/RPMS/$target/suity-agentic-$VERSION-1.$target.rpm" ./
