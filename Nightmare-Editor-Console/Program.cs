using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Nightmare_Editor;
using Nightmare_Editor.NewTools;
using SixLabors.ImageSharp;

string[] AllPaths = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "filelist.txt")).Distinct().ToArray();

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("________________________________________________________________________________");
Console.WriteLine("::::::::::::::::::: Kingdom Hearts 3D Romhacking Suite v0.0.0 ::::::::::::::::::");
Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("________________________________________________________________________________");
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("::::::::::::::::::::::::::::: Developed by Solt 11 :::::::::::::::::::::::::::::");
Console.WriteLine("::::::::::::::: Based on the work of Deep Dive Translations Team :::::::::::::::");
Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine("________________________________________________________________________________");

if (args.Length != 0)
{
    int option = -1;
    bool silent = false;
    if (args.Length >= 2)
    {
        option = int.Parse(args[1]);
    }
    if (args.Length >= 3)
    {
        silent = (args[2] == "-s" || args[2] == "-silent");
    }

    if (option == -1)
    {
        Console.Write(@"Choose what you want to do :
1) Unpack Files                 // Input: .rbin
2) Repack Files                 // Input: .rbin
3) Parse CTD Files              // Not Implemented
4) Repoint CTD Files            // Not Implemented
5) Decompress BLZ               // Input: Any BLZ Compressed File
6) Compress BLZ                 // Not Implemented
7) Parse BCFNT Files            // Not Implemented
8) Repack BCFNT Files           // Not Implemented
9) Deswizzle CTT Files          // Input: .ctt
10) Swizzle CTT Files           // Input: .png
11) Extract Container* Files    // Input: Container File
12) Repack Container* Files     // Input: Container File
13) Create TXA Atlas            // Input: .txa, required textures must be in the same directory
14) Exit

*Container files include .l2d, .fep, .pmo, .pmp, and the operations affect the textures inside.

Make your choice : ");
        
        option = int.Parse(Console.ReadLine());
    }
    if (option == 1)
    {
        try
        {
            RBIN.Load(args[0], Path.GetDirectoryName(args[0]), true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    if (option == 2)
    {
        try
        {
            RBIN.Pack(args[0], true, null, args[0]);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    if (option == 9)
    {
        try
        {
            CTT.Decode(args[0], true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    if (option == 10)
    {
        try
        {
            string filename = Path.GetFileName(args[0]);
            int format = (int)Enum.Parse(typeof(CTT.Format), args[0].Split('.')[^2]);
            byte[] data = CTT.Swizzle(File.ReadAllBytes(args[0]), format);
            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(args[0]), filename.Split('.')[0] + ".ctt"), data);
            Console.WriteLine("Saved file to: " + Path.Combine(Path.GetDirectoryName(args[0]), filename.Split('.')[0] + ".ctt"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    if (option == 11)
    {
        try
        {
            string filename = Path.GetFileName(args[0]);
            if (Containers.IsArc(filename))
            {
                Containers.Generic.Unpack(args[0]);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    if (option == 12)
    {
        try
        {
            string filename = Path.GetFileName(args[0]);
            if (Containers.IsArc(filename))
            {
                Containers.Generic.Pack(args[0]);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    if (option == 13)
    {
        try
        {
            string filename = Path.GetFileName(args[0]);
            var result = TXA.Load(args[0], Path.GetDirectoryName(args[0]));
            TXA.TXAFile txa = result.TXAFile;
            if (result.ErrorCode == Misc.ErrorCode.FailedFileFind)
            {
                Console.WriteLine(result.ErrorValue);
            }
            else
            {
                for (int i = 0; i < txa.DestTextures.Count; i++)
                {
                    using (var img = TXA.Atlas(txa, i))
                    {
                        img.SaveAsPng(Path.Combine(Path.GetDirectoryName(args[0]),
                            Path.GetFileNameWithoutExtension(args[0]) + i.ToString() + "a.png"));
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    if (!silent)
    {
        if (option != 14)
        {
            Console.WriteLine("Done!");
            Console.ReadLine();
        }
        Console.WriteLine("The program will now close.");    
    }
}
else
{
    Console.ForegroundColor = ConsoleColor.DarkMagenta;
    Console.WriteLine(@"Usage : Drag & Drop a file ON the executable!
        Keep in mind that the unpacking/repacking will happen
        in the same folder of the dragged & dropped files.

        Detailed explanation :

        1) Unpack Files : it requires a .rbin to be dragged & dropped
           on the executable. It will create a folder in the same directory
           where the .rbin file is and will unpack its files in there.

        2) Repack Files : it requires a .rbin to be dragged & dropped
           on the executable. The .rbin required is the same you used to unpack.
           Example : message.rbin. It will overwrite your previous file.

        3) Parse CTD Files : it requires an unpacked .ctd file to be
           dragged & dropped on the executable. It will create two files,
           a .txt, that will contains texts, and a .pnt file.
           You'll just have to translate the .txt one.

        4) Repoint CTD Files : it requires an unpacked .ctd to be
           dragged & dropped on the executable. The one you used with
           'Parse CTD Files' is the same you need here, too.
           It will overwrite your previous .ctd file.

        5) Decompress LZOvl : it requires any file, to be dragged & dropped on
           the executable. It will overwrite your file with the decompressed one.

        6) Compress LZOvl : it requires any file, to be dragged & dropped on
           the executable. It will create a decompressed .ovl file.

        7) Parse BCFNT Files : it requires a .bcfnt file to be
           dragged & dropped on the executable. It will create a folder
           with all the extracted data.

        8) Repack BCFNT Files : it requires a .bcfnt file to be 
            dragged & dropped on the executable. It will overwrite your
            previous file.

        9) Deswizzle CTT Files : it requires a .ctt file to be 
            dragged & dropped on the executable.

        10) Swizzle CTT Files : it requires a .png file to be 
            dragged & dropped on the executable.

        11) Extract CTT Files : it requires a container* file to be 
            dragged & dropped on the executable. It will create a folder with
            its name and will extract all the files inside of it.

        12) Repack CTT Files : it requires a container* file to be 
            dragged & dropped inside the executable. It will
            overwrite your previous file.

        *Container files include .l2d, .fep, .pmo, .pmp, and the operations affect the textures inside.

        13) Exit : it just closes this program.


                                 Thank you for using our tool,

                                        -Deep Dive Translations Team
        
                                 Thank you for using Nightmare Editor!
                                        -Solt11");
    Console.ReadKey();
}
return;


void HashTest()
{
    string rbin = "";
    string filename = "";
    foreach (string file in AllPaths)
    {
        string[] sub = file.Split('/');
        if (file.StartsWith("chara"))
        {
            rbin = sub[0] + "_" + sub[1];
        }
        else
        {
            rbin = sub[0];
        }

        filename = sub[^1];
        (string hashPath, bool handled) = RBIN.HashPath(rbin, filename);
        if (handled)
        {
            if (file != hashPath)
            {
                Console.WriteLine("Correct: " + file + "\n" + "Outputs: " + hashPath + "\n");
            }
        }
    }
    Console.WriteLine("Done.");
}
