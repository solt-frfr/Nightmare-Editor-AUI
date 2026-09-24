# Tutorial

## Making your first mod! (3)

### Replacing the Kingdom Key's Textures

Now that we have our textures, we'll replace the ones already on the Kingdom Key.

1. In the second column, find and click on `w_so010.pmo`
   
   - If the second column does not exist, click on `chara_wep.rbin` and wait for it to load.
   - You can search for files using the text box at the top of the second column.
   - This will load the first texture in the file.

2. In the bottom-right corner, click on __*Link New Texture*__.
   
   - Press OK on the pop-up. The textures we extracted have the same dimensions as the one we want to replace, so its warning does not matter right now.
   - Find the first texture we extracted and select it.
   - This will link the texture (wow, crazy). This means the editor will remember that this is the texture we want to use when packing it.
   - The two texture views are now different! The Embedded view shows whatever is currently inside the texture. The Linked view shows what we want to replace it with.

3. In the third column, click `w_so010_01.ctt`.

4. In the bottom-right corner, click on __*Link New Texture*__.
   
   - Same as Step 4, but select the second texture we extracted instead.

5. In the third column, right-click `w_so010_00.ctt` and select `Queue/Unqueue Pack`.
   
   - You can check if the file is queued by clicking on `View Queue` in the bottom-left corner.

6. In the third column, right-click `w_so010_01.ctt` and select `Queue/Unqueue Pack`.
   
   - Same as Step 5 but with the other texture.

7. Click on `View Queue` in the bottom-left corner.
   
   - This will show all the files currently in the queue, including files that are parents of the files you've queued.

8. Click on __*Export Mod*__.

9. Fill out the metadata however you want and click `Confirm`.
   
   - If you're unsure what to put, try using this as a guide:
     
     - Name: `Kingdom Key D`
     
     - Description: `Replaces the Kingdom Key with King Mickey's Keyblade, Kingdom Key D.`
     
     - Author - put your name or username here
     
     - Link - You'd put a Gamebanana link or Github link here, but for now leave it blank. You won't need it for this one.
     
     - ID - The best practice is to do `author``.``name`, so I would probably put `solt-frfr.kingdomkeyd`. You're only allowed to use lowercase letters, numbers, and `-` or `.`.
     
     - Prefix - leave blank.
     
     - Preview - If you have an image demonstrating the mod, open it here.
     
     - Color - Anything that fits the mod or strikes you as a nice color! 

10. You'll be prompted to save the mod as a `.nem` file.
    
    - You can save it if you'd like. Regardless of whether you save the `.nem` file, it will appear in your list of mods inside Exam Editor.

11. Click `OK` on the disclaimer.



***You've learned how to make a mod!*** Our last step is testing! 

You can now leave Nightmare Editor at this point by pressing `View Files` in the bottom-left corner, and clicking the pink backwards arrow. That will take you back to Exam Editor.
