#!/bin/bash

flatpak-builder --force-clean --install-deps-from=flathub --repo=repo flatpak-build flatpak/fyi.soltfrfr.NightmareEditor.yml

flatpak build-bundle repo nightmare-editor.flatpak fyi.soltfrfr.NightmareEditor --runtime-repo=https://dl.flathub.org/repo/flathub.flatpakrepo
