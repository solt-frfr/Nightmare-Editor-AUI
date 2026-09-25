#!/bin/bash

dotnet publish "NightmareEditor/NightmareEditor.csproj" \
    -c Release --runtime linux-x64 --sc \
    --output ./dist/linux-x64 -p:PublishSingleFile=true