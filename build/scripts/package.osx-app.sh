#!/usr/bin/env bash

set -e
set -o
set -u
set pipefail

cd build

mkdir -p Suity.Agentic.app/Contents/Resources
mv Suity.Agentic Suity.Agentic.app/Contents/MacOS
cp resources/app/App.icns Suity.Agentic.app/Contents/Resources/App.icns
sed "s/SUITY_AGENTIC_VERSION/$VERSION/g" resources/app/App.plist > Suity.Agentic.app/Contents/Info.plist
rm -rf Suity.Agentic.app/Contents/MacOS/Suity.Agentic.dsym
rm -f Suity.Agentic.app/Contents/MacOS/*.pdb

zip "suity-agentic_$VERSION.$RUNTIME.zip" -r Suity.Agentic.app
