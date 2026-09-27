# Nightmare Editor

Nightmare Editor is a free, open source toolset focused on modding Kingdom Hearts 3D: Dream Drop Distance. Built in is its own mod manager, Exam Editor, focused on providing a simplified way to build multiple mods into the 3DS version of the game. This tool works with games running both on real 3DS hardware and emulators such as Azahar, with texture pack support for emulators.

## Installation

### Windows

A zip file is provided containing a portable program in the Releases page. There is no installer version currently.

### MacOS

Use osx-x64 for Intel Macs and osx-arm64 for Apple Silicon Macs.

### Linux

A flatpak is avalible in the downloads and will work for most end users. There is also archives containing executables for each processor type.

## Building from source

Ensure .NET 8 is installed on your system. Then run `dotnet build` in the top directory of the project.

## About

This project was solo developed by me, Solt11, out of hatred for the modding process and the wish to make it more convenient. This snowballed into massive rewrites of the original program for cross-platform compatibility reasons and the goal of making open-source functions for parsing these files. It also acts as its own mod manager.

## Credit

### Deep Drive Translations Team

I have no idea who these people are, but their tools are what inspired the entire project. They are much appreciated for that, and I thank them dearly. Since there is no good archive of them online, I have kept their toolset inside this repository. If you are one of these people and would like this toolset removed, please let me know.

### OpenKH

The #ddd-modding channel has bore the pains with me as I developed this. They also showed to me that modding this game actually is possible. They also helped me with UI design and some of the more painful parts of programming. I am also using their BLZ decompression algorithm.
