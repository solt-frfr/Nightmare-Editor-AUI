using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using SixLabors.ImageSharp;

namespace Nightmare_Editor.NewTools;

public static class PMP
{
    public class PMPFile
    {
        public PMPHeader Header { get; set; }
        public List<PMPObject> Objects { get; set; }
        public List<PMO.PMOTexture> Textures { get; set; }
    }

    public class PMPHeader
    {
        public readonly byte[] Magic = [0x50, 0x4D, 0x50, 0x00];
        public UInt16 Version { get; set; }
        public PMPFlag Flag { get; set; }
        public UInt16 ModelInstanceCount { get; set; }
    }

    public enum PMPFlag
    {
        NO_FLAG = 0,
        MAPFLAG_DISPOFF = 1,
        MAPFLAG_PRESETOFF = 2,
        MAPFLAG_SYSPRESETOFF = 4
    }
    
    public class PMOFile
    {
        public PMO.PMOHeader Header { get; set; }
        public List<PMO.PMOMesh> Meshes { get; set; }
        public List<int> TextureIndexes { get; set; }
        public PMO.PMOSkeleton Skeleton { get; set; }
    }
    
    public class PMPObject
    {
        public float[] Position { get; set; }
        public float[] Rotation { get; set; }
        public float[] Scale { get; set; }
        public PMOFile PMO { get; set; }
        public uint Pointer { get; set; }
        public UInt16 Flag { get; set; }
        public UInt16 ID { get; set; }
    }

    public static PMPFile Load(string source)
    {
        PMPFile file = new PMPFile();
        PMPHeader header = new PMPHeader();
        List<PMO.PMOTexture> textures = new List<PMO.PMOTexture>();
        List<PMPObject> objects = new List<PMPObject>();
        byte[] data = File.ReadAllBytes(source);
        int o = 0x4;
        header.Version = (UInt16)(data[o++] + (data[o++] * 0x100));
        o += 9;
        header.Flag = (PMPFlag)data[o++];
        int objectCount = data[o++] + (data[o++] * 0x100);
        header.ModelInstanceCount = (UInt16)(data[o++] + (data[o++] * 0x100));
        o += 6;
        int textureCount = data[o++] + (data[o++] * 0x100);
        int texListOffset =  data[o++] + (data[o++] * 0x100) + (data[o++] * 0x10000) + (data[o++] * 0x1000000);
        List<uint> texOffsets = new List<uint>();
        int t = texListOffset;
        for (int i = 0; i < textureCount; i++)
        {
            var tex = new PMO.PMOTexture();
            tex.Offset = (uint)(data[t++] + (data[t++] * 0x100) + (data[t++] * 0x10000) + (data[t++] * 0x1000000));
            texOffsets.Add(tex.Offset);
            byte[] nameBytes2 =
            {
                data[t++], data[t++], data[t++], data[t++], data[t++], data[t++], data[t++], data[t++],
                data[t++], data[t++], data[t++], data[t++]
            };
            nameBytes2 = nameBytes2.Where(b => b != 0).ToArray();
            tex.Name = System.Text.Encoding.ASCII.GetString(nameBytes2);
            tex.TilingX = BitConverter.ToSingle(data, t);
            t += 4;
            tex.TilingY = BitConverter.ToSingle(data, t);
            t += 12;
            int height = data[tex.Offset + 0x22] + (data[tex.Offset + 0x23] * 0x100);
            int width = data[tex.Offset + 0x20] + (data[tex.Offset + 0x21] * 0x100);
            int format = data[tex.Offset + 0x1C];
            double bpp = 0;
            if (format == 0)
            {
                bpp = 4;
            }
            else if (format == 1)
            {
                bpp = 3;
            }
            else if (format <= 6)
            {
                bpp = 2;
            }
            else if (format <= 9 || format == 13)
            {
                bpp = 1;
            }
            else
            {
                bpp = 0.5;
            }
            
            int k = 0;
            int size = (int)(0x80 + (width * height * bpp));
            if (tex.Offset > 0 && tex.Offset < data.Length)
            {
                tex.CTT = new byte[size];
                for (int j = 0; j < size; j++)
                {
                    if (k + tex.Offset >= data.Length)
                    {
                        string e = "e";
                    }
                    tex.CTT[j] = data[(k++) + tex.Offset];
                }
            }
            else
            {
                tex.CTT = null;
            }
            textures.Add(tex);
        }
        for (int i = 0; i < objectCount; i++)
        {
            PMPObject obj = new PMPObject();
            float[] pos =  new float[3];
            float[] rot = new float[3];
            float[] scale = new float[3];
            pos[0] = BitConverter.ToSingle(data, o);
            o += 4;
            pos[1] = BitConverter.ToSingle(data, o);
            o += 4;
            pos[2] = BitConverter.ToSingle(data, o);
            o += 4;
            rot[0] = BitConverter.ToSingle(data, o);
            o += 4;
            rot[1] = BitConverter.ToSingle(data, o);
            o += 4;
            rot[2] = BitConverter.ToSingle(data, o);
            o += 4;
            scale[0] = BitConverter.ToSingle(data, o);
            o += 4;
            scale[1] = BitConverter.ToSingle(data, o);
            o += 4;
            scale[2] = BitConverter.ToSingle(data, o);
            o += 4;
            uint pmoOffset = (uint)(data[o++] + (data[o++] * 0x100) + (data[o++] * 0x10000) + (data[o++] * 0x1000000));
            obj.Pointer = (uint)(data[o++] + (data[o++] * 0x100) + (data[o++] * 0x10000) + (data[o++] * 0x1000000));
            obj.Position = pos;
            obj.Rotation = rot;
            obj.Scale = scale;
            obj.Flag = (UInt16)(data[o++] + (data[o++] * 0x100));
            obj.ID = (UInt16)(data[o++] + (data[o++] * 0x100));
            objects.Add(obj);
        }
        file.Textures = textures;
        file.Objects = objects;
        file.Header = header;
        return file;
    }
    
    public static void ExtractAllTextures(string source)
        {
            PMPFile pmp = Load(source);
            if (pmp.Textures.Count > 0)
            {
                string path = Path.Combine(Path.GetDirectoryName(source), Path.GetFileNameWithoutExtension(source));
                Directory.CreateDirectory(path);
                foreach (PMO.PMOTexture tex in pmp.Textures)
                {
                    if (tex.CTT != null)
                    {
                        File.WriteAllBytes(Path.Combine(path, tex.Name + ".ctt"), tex.CTT);
                        var split = CTT.SplitHeader(tex.CTT);
                        var attrib = CTT.GetAttributesFromHeader(split.header);
                        
                        using (var image = CTT.Deswizzle(split.data, attrib.width, attrib.height, (int)attrib.format))
                        {
                            image.SaveAsPng(Path.Combine(path, tex.Name + "." + attrib.format.ToString() + ".png"));
                        }
                    }
                }
            }
        }
        
        public static void ReplaceAllTextures(string source)
        {
            PMPFile pmp = Load(source);
            byte[] data = File.ReadAllBytes(source);
            if (pmp.Textures.Count > 0)
            {
                foreach (PMO.PMOTexture tex in pmp.Textures)
                {
                    string path = Path.Combine(Path.GetDirectoryName(source), Path.GetFileNameWithoutExtension(source));
                    path = Path.Combine(path, tex.Name + ".ctt");
                    byte[] ctt = File.ReadAllBytes(path);
                    for (int i = 0; i < tex.CTT.Length; i++)
                    {
                        tex.CTT[i] = ctt[i];
                        data[tex.Offset + i] = ctt[i];
                    }
                }
            }
            File.WriteAllBytes(source, data);
        }
}