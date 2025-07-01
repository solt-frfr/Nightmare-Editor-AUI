# Linking Textures
 Linking textures is a way to keep the textures you're working on safe from compression. The texture held next to the file, the one you'd find by looking at the folder it's held inside, will always be unpacked again after packing the file again. This is so that you can see how the texture looks after any compression algorithms it goes through, such as ETC1 or RGB565.
#### 
## To link a texture...

![Lnk1](Help/Images/Lnk1.png)

 If you wish to link a texture, simply press the button below the texture information that says "__***Link New Texture***__". It will open a file window where you can then select a file to link to the .ctt texture.

## Textures will remain linked upon closing.
 The files and correlating linked textures are stored in a .json file that will save which textures correspond to which file.

## To unlink a texture...
 Simply double-click the textbox that shows the linked texture location. A prompt will open to unlink the texture.