# Tutorial

## Making your first mod!

### Spolier warning!

You will be viewing the game files. If you have not finished the game, you might find spoilers ahead! Please be aware!



### What we will accomplish

This tutorial will show you the basics of Nightmare Editor, while also creating a mod for you to use.

We will make a very simple mod to show the basics, by replacing the Kingdom Key's textures with the Kingdom Key D's textures. This will make it look identical to the Kingdom Key D, since they share the same geometry.

### Initial Setup

Getting started, you'll want to have a couple things set up. You won't have to repeat this.

- [Azahar](https://azahar-emu.org/pages/download/), to make testing a lot simpler.

- A dump of the 3DS version of Kingdom Hearts 3D: Dream Drop Distance. 
  
  - If you do not have this, refer to https://3ds.hacks.guide/dumping-titles-and-game-cartridges.html. 
  
  - Rather than building a .cia or dumping a .3ds, I prefer to use a .cxi file. 
  
  - Copy this to a memorable location on your computer.

This guide will assume you have never installed Azahar, and have never done any setup for Nightmare Editor. If you need to go back to Exam Editor, press the pink backwards arrow button.

First, follow these steps in Azahar:

- Double-click the center of the window to add a new folder to the application list.
  
  - Select the location that contains your dump of the game.

- Right-click KINGDOM HEARTS 3D and click `Dump RomFS`.

Next, follow these steps in Exam Editor

- Set Exam Editor's deploy path to Azahar's `load` folder. 
  
  - You can find this by launching Azahar, go to `File -> Open Azahar Folder -> User Folder`, and then finding the `load` folder. 
  
  - If it does not exist, create it.

- Enable the Emulator setting.

- Set your game region to the version of the game you are using. 
  
  - Only support for the North American version is guaranteed. Some mods may not be compatible with other versions.
