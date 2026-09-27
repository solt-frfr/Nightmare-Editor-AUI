# Glossary

### Adding Files

Simply click the large `+` button. You can either add an .rbin file, or any other file.

- `.rbin` files will be added to the column at the left after extracting. Clicking on it will let you browse the contents.

- Other files will be added to `User-Added.rbin` after any unpacking it may need to go through. You can then browse its contents after clicking on `User-Added.rbin`.

### Linked Textures

- Click on __*Link New Texture*__ and select a `.png` file. The editor will remember that this is the texture we want to use when packing it.
- The two texture views are now different! The Embedded view shows whatever is currently inside the texture. The Linked view shows what we want to replace it with.
- If you'd like to unlink a texture shift-click the textbox that shows the linked texture location. A prompt will open to unlink the texture.

### Packing Files

- If you want to make changes to a file, you need to save them when you're done. That's what packing is, it tells the program: "Hey, I made the changes, put it in a state that it can be put back into the game."

- Let's use a quick demo.
  
  - `p_ex010.pmo` is DDD Sora's model. The textures are placed in a folder Nightmare Editor knows where to find.
  
  - If I link a texture to his body texture, `p_ex010_01.ctt`, nothing inside the actual file changes. Packing the file is what does, and it'll update the file inside the folder.
  
  - On its own, this still does nothing, because the model file has the same idea behind it. Packing the model takes all the textures from that folder and inserts them back into the actual model file. If a texture wasn't packed, it won't be changed when packing the model.

### Queued Files

- This tells the program what files to put into the mod.

- Every file queued will be packed before being put into the mod.

- If a file relies on another one, it'll also be queued.
  
  - If I mark Sora's body texture, `p_ex010_01.ctt`, as queued, it will also mark his model file, `p_ex010.pmo`, to be queued, since that's where it comes from.
  
  - Since `p_ex010.pmo` comes from `chara_pc.rbin`, it will also be marked.
    
    - When just exporting a mod, any `.rbin` files won't be packed. This is because they are the reason this program exists, which is the need to pack multiple changes into a single `.rbin` file.

- The queue can be viewed at any time by clicking the `View Queue` button. You can return to the normal view by pressing the`View Files`button, which will appear in the same location.

### Exporting Mods

- __*Pack Queued*__ will pack the queued files into their respective `.rbin` files, where you will then be prompted to save each one as they finish. Note that only the `.rbin` files showed in the queue will be exported, as it would be pointless to do the rest.

- __*Export Mod*__ will pack the queued files, aside from `.rbin` files, into a mod archive, which is sharable, and users can install them into the built in Mod Manager, **Exam Editor**. You may specifiy the mod's name, author, description, link, and preview image. After finishing, you will be prompted to save the archive. Regardless of where you save it, the files will also be added to your Exam Editor's mods.

### Emulator Textures

- If you'd like to include support for emulator textures, you can put them inside an `~emulator-textures` folder inside your mod's folder that contains the `rbin` folders. Please use the filenames that are made from your texture dumps *while your mod is running*.