# Other File Types

### User-Added.rbin

`User-Added.rbin` is where files you add, that aren't .rbin files, go. It has a few things you should know.  

1. Files inside User-Added.rbin cannot be queued.  
2. `User-Added.rbin` cannot be re-extracted or packed. If you look in the file directory for it, it's simply a dummy file with no data inside.  
3. Files inside `User-Added.rbin` can be removed, unlike files inside real .rbin files.

### PMO Files

Nightmare Editor is able to edit the textures inside these, but in order to edit the model, you'll need to go through some more effort.

- You can convert the model to `.fbx` using Kité's version of KHModels

- You can convert an `.fbx` model to a `.pmo` using Kité's PMO Builder.

- Using `Replace File` in Nightmare Editor, you can bring a new model into the editor where you can queue it for a mod or continue to edit its textures.

### TXA Files

Due to how different they are, they've been put into their own separate editor.

I've tried to make the editor as versatile as possible, but the simplest form of editing will use the Atlas feature.

- Use the `Create Atlas` button to build an Atlas of all the textures used for a given group of the TXA.

- This gives you a large image containing all of the different frames of animations. Edit it however you want.

- When you're done, use the `Import Atlas` button.

When you're done making changes, pressing the `Pack` button will act as packing the file in the main editor.

Make sure to Queue the file if putting it into a mod.

### Other file types

Because this is mostly focused on textures, files that are not textures or do not contain textures can't really be edited here. However, I know some other resources I can link to.   

- `.moflex` files: Unfortunately, I don't have any programs to link to for these, but they *are* 3D movie files.   

- `.bcsar` and `.bcstm` files: These are sound archive files and sound files respectively. Citric Composer can edit these. I found mu-wave to be the best tool for making new ones.

- `.bcfnt`: As the name may suggest, these are font files. There's a couple editors I've seen online, but the one I personally use is NintyFont.  

### Resources

- [Citric Composer on Github](https://github.com/Gota7/Citric-Composer)

- [mu-wave](https://kazuki-4ys.github.io/web_apps/mu-wave/)

- [Ninty Font on Github](https://github.com/hadashisora/NintyFont)


