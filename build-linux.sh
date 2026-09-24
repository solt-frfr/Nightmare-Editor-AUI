#!/bin/bash

dotnet publish "Nightmare Editor AUI/Nightmare Editor AUI.csproj" \
    -c Release --runtime linux-x64 --sc \
    --output ./dist/linux-x64 -p:PublishSingleFile=true