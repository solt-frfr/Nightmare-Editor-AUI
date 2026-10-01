#!/bin/bash

BUILDS=("win-x64" "win-arm64" "win-x86" "linux-x64" "linux-arm64" "linux-arm" "osx-x64" "osx-arm64")

for TARGET in "${BUILDS[@]}"; do

    dotnet publish "NightmareEditor/NightmareEditor.csproj" \
        -c Release --runtime "$TARGET" --sc \
        --output "./dist/$TARGET" -p:PublishSingleFile=true

    zip -r -j "dist/zip/$TARGET.zip" "dist/$TARGET/"

done

