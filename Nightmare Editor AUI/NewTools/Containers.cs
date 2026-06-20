using System;
using System.IO;
using System.Runtime.CompilerServices;
using MsBox.Avalonia;
using SixLabors.ImageSharp;

namespace Nightmare_Editor.NewTools;

public class Containers
{
    public class Generic
    {
        public static void Pack(string file)
        {
            
            try
            {
                if (Path.GetExtension(file).ToLower() == ".l2d")
                {
                    L2D.L2D.ReplaceAllTextures(file);
                }
                if (Path.GetExtension(file).ToLower() == ".pmo")
                {
                    PMO.ReplaceAllTextures(file);
                }
                if (Path.GetExtension(file).ToLower() == ".pmp")
                {
                    PMP.ReplaceAllTextures(file);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void Unpack(string file)
        {
            if (Path.GetExtension(file).ToLower() == ".l2d")
            {
                L2D.L2D.ExtractAllTextures(file);
            }
            if (Path.GetExtension(file).ToLower() == ".pmo")
            {
                PMO.ExtractAllTextures(file);
            }
            if (Path.GetExtension(file).ToLower() == ".pmp")
            {
                PMP.ExtractAllTextures(file);
            }

            if (Path.GetExtension(file).ToLower() == ".fep")
            {
                GenericUnpack(file);
            }
        }
    }
    
    public static bool IsArc(string file)
    {
        if (Path.GetExtension(file) == ".pmo" || Path.GetExtension(file) == ".l2d" || Path.GetExtension(file) == ".fep" || Path.GetExtension(file) == ".pmp")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void GenericUnpack(string file)
    {
        // Files unpacked with this method will not have proper filenames and will revert to an index.
        // The file will be sifted through until a texture header is found and then start unpacking.
        int tex_idx = 0;
        byte[] data = File.ReadAllBytes(file);
        for (int i = 0; i < data.Length; i+=4)
        {
            if (i + 0x80 < data.Length)
            {
                if (data[0x00 + i] == 0x43
                    && data[0x01 + i] == 0x54
                    && data[0x02 + i] == 0x52
                    && data[0x03 + i] == 0x54)
                {
                    byte[] header = new byte[0x80];
                    for (int k = 0; k < 0x80; k++)
                    {
                        header[k] = data[i + k];
                    }

                    var attrib = CTT.GetAttributesFromHeader(header);
                    int total = header[0x14] + (header[0x15] << 8) + (header[0x16] << 16) + (header[0x17] << 24);
                    byte[] newData = new byte[total];

                    for (int k = 0; k < total; k++)
                    {
                        newData[k] = data[i + k + 0x80];
                    }
                    
                    string path = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file));
                    Directory.CreateDirectory(path);
                    
                    byte[] saveData = new byte[total + 0x80];
                    for (int k = 0; k < 0x80 + total; k++)
                    {
                        if (k < 0x80)
                        {
                            saveData[k] = header[k];
                        }
                        else
                        {
                            saveData[k] = newData[k - 0x80];
                        }
                    }
                    
                    File.WriteAllBytes(Path.Combine(path, tex_idx + ".ctt"), saveData);

                    using (var image = CTT.Deswizzle(newData, attrib.width, attrib.height, (int)attrib.format))
                    {
                        image.SaveAsPng(Path.Combine(path, tex_idx + "." + attrib.format.ToString() + ".png"));
                    }
                    tex_idx++;
                }
            }
        }
    }

    public static void GenericPack(string file)
    {
        // Files packed with this method will have been assumed to be unpacked with the GenericUnpack method.
        // Otherwise, it will fail.
        int tex_idx = 0;
        byte[] data = File.ReadAllBytes(file);
        string path = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file));
        for (int i = 0; i < data.Length; i+=4)
        {
            if (i + 0x80 < data.Length)
            {
                if (data[0x00 + i] == 0x43
                    && data[0x01 + i] == 0x54
                    && data[0x02 + i] == 0x52
                    && data[0x03 + i] == 0x54)
                {
                    byte[] header = new byte[0x80];
                    for (int k = 0; k < 0x80; k++)
                    {
                        header[k] = data[i + k];
                    }
                    
                    int total = header[0x14] + (header[0x15] << 8) + (header[0x16] << 16) + (header[0x17] << 24);
                    string texpath = Path.Combine(path, tex_idx + ".ctt");
                    if (File.Exists(texpath))
                    {
                        byte[] texData = File.ReadAllBytes(texpath);
                        if (texData.Length == total + 0x80)
                        {
                            for (int k = 0; k < texData.Length; k++)
                            {
                                data[i + k] = texData[k];
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    tex_idx++;
                }
            }
        }
        File.WriteAllBytes(file, data);
    }
}