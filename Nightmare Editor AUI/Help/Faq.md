# Frequently Asked Questions
## A window that says "Kingdom Hearts 3D Romhacking Suite v0.8.5" showed up. What do I do?
 Check the "**Romhacking Suite**" section. It'll tell *exactly* you what to do.
#### 
## A blank console window appeared and everything froze. What do I do?
 Don't panic. That's likely ETC.exe running. Just wait for it to finish. It takes about a minute, though it depends on texture size.
#### 
## The program just froze!
 Although it's unlikely for this to happen, since I patched one of the main reasons this would happen during the middle of development, my advice is just to wait if it does. If it crashes or doesn't unfreeze after a long period of time, contact me and/or leave a bug report on this program's GitHub page, at **https://github.com/solt-frfr/Nightmare-Editor/issues**.
#### 
## How do I edit (insert file type here)?
 Because this is mostly focused on textures, files that are not textures or do not contain textures can't really be edited here. However, I know some other resources I can link to.
#### 
 **.moflex** files: Unfortunately, I don't have any programs to link to for these, but they *are* 3D movie files.
#### 
 **.bcsar** and **.bcstm** files: These are sound archive files and sound files respectively. Citric Composer can edit these. I found mu-wave to be the best tool for making new ones. Have fun!

 **https://github.com/Gota7/Citric-Composer**

 **https://kazuki-4ys.github.io/web_apps/mu-wave/**
#### 
 **.bcfnt**: As the name may suggest, these are font files. There's a couple editors I've seen online, but the one I personally use is NintyFont.

 **https://github.com/hadashisora/NintyFont**
#### 
## Why is this necessary?
It isn't. This program is made for convenience, but I *highly* recommend someone uses it instead of not. There's quite a few reasons, actually, and they're all quite stupid.
#### 
### 1. Repacking .rbin Files.
 After repacking an .rbin file, it compelete messes up the data of each file. Couldn't tell you why.
#### 
 By keeping things organized into different folders, it cleans the process significantly, since you can copy over everything to start again.
#### 
 Typically when I did my projects before this tool, I had 3 separate folders for each .rbin. A work folder, a base folder, and the folder that will be packed. This tool keeps everything streamlined so that *you* aren't having to jumble around a ton of folders.
#### 
### 2. Inconvenient Texture Packing.
 It was difficult to figure out *how* to replace textures in the first place. Even though I know how to now, that doesn't make it simple. You have to figure out the format of the texture, first. If it's etc, do this, if it's 565, do that, etc. Whatever you end up doing will spit out a .ctt texture.
#### 
 Here arises the next problem. From what I've noticed, miscellanous data can be packed in or around these textures, especially if they reside inside a .pmo or .l2d file. The only way to properly insert the new data is to go in a hex editor, find where the texture starts, and paste without inserting. As an aside, linking textures with the base toolkit *never* worked for me, so I've always had to do this process regardless. I tried testing to see if I can get rid of the extra data, but it caused some visual bugs, like Sora's HUD icon appearing in the center of the screen.
#### 
### 3. Previous Failures.
 When I got into hacking this game, although it took me a while, days in fact, I got the hang of the whole process and managed to import some textures. Months later, I came back for another project, and did not remember at all how to do it. It took me about a day to remember the whole process, which I then outlined in the OpenKH discord server so that I wouldn't forget.

SOLT11 IF YOU WANT TO EDIT TEXTURES READ THIS

> 1. Dump game
> 2. unpack .rbin
> 3. copy pmo and move folder of choice to root directory for working
> 4. delete all other folders (not files), the extracted directory should contain only files and no folders
> 5. edit png
> 6. take the PNG and run ETC.exe on the png. this will rebuild the CTT. 
> 7. open pmo and ctt in hex editor
> 8. replace texture data inside pmo with ctt (starts with CTRT)
> 8. put pmo back in extracted directory
> 9. rebuild .rbin
> 10. place in layeredfs romfs directory 
> 11. unpack .rbin if you want to make another modification, or else it won't work

#### 
## I still need help!
 Please use logic and try to figure this out best you can. The answers can't always be obvious. Consider rereading the entire help window, as it'll likely answer your question in one of the sections. If you *really* are still puzzled, go to the OpenKH discord server and ping me in **#ddd-modding**. I will not be linking the server on purpose, so that it is more tedious for you to ask others before figuring it out yourself. Besides, if you have this program, you're likely already in the server, and have talked to me before.
#### 
## Are there any future plans you have?
 I'd like to learn how all these file formats work myself so that I can implement them myself manually, without reliance on tools, since it would then allow the larger community to understand how they work, since this is open source. I already had to reverse engineer most of .ctt textures because packing for RGB565 textures failed with the toolkit. Still don't really know why.
#### 
# Reimplemented Functions
 As I move away from using the Deep Drive Translation Team's tools, I will talk about functions I've reimplemented here.
#### 
## CTT Encoding and Decoding
 Packing CTT files in the toolkit just *didn't work*. It basically just returned a blank image. So I spent almost the entire next day implementing decoding/encoding ctt textures from scratch. Within that day, I was able to completely implement RGBA8888, RGB888, and RGB565. I also adjusted many minor things, like switching to the correct format identifiers, and implementing a half-baked texture linking that didn't store your progress, unlike the final version.
#### 
# About
 This project was solo developed by me, Solt11, out of hatred for the modding process and the wish to make it more convenient. It is a program that acts as a helper for a different set of tools made by the Deep Dive Translations team. My hope is that this program helps someone in their modding endevours.
#### 
## Credit
### Deep Drive Translations Team
 I have no idea who these people are, but there tools are what make this whole thing possible. They are much appreciated for that, and I thank them dearly.
#### 
### OpenKH
 The #ddd-modding channel has bore the pains with me as I developed this. They also showed to me that modding this game actually *is* possible.