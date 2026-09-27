#!/bin/bash

dotnet publish "NightmareEditor/NightmareEditor.csproj" \
    -c Release --runtime linux-x64 --sc \
    --output ./dist/linux-x64 -p:PublishSingleFile=true

flatpak-builder --force-clean --install-deps-from=flathub --repo=repo flatpak-build fyi.soltfrfr.NightmareEditor.yml

flatpak build-bundle repo nightmare-editor.flatpak fyi.soltfrfr.NightmareEditor --runtime-repo=https://dl.flathub.org/repo/flathub.flatpakrepo
