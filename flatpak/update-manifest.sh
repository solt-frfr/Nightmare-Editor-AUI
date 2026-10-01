#!/bin/bash

cd ..

tag=$(git describe --tags --abbrev=0)
commit=$(git rev-list -n 1 "$tag")

echo "Tag: $tag"
echo "Commit: $commit"

cd flatpak

sed -i "s/tag: .*$/tag: $tag/" fyi.soltfrfr.NightmareEditor.yml
sed -i "s/commit: .*$/commit: $commit/" fyi.soltfrfr.NightmareEditor.yml
