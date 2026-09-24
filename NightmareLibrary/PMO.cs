using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace NightmareLibrary
{
    public static class PMO // Right now I'm just structuring everything to load the file, then I can adjust for texture and mesh data.
    {
        public class PMOFile
        {
            public PMOHeader Header { get; set; }
            public List<PMOMesh> Meshes { get; set; }
            public List<PMOTexture> Textures { get; set; }
            public PMOSkeleton Skeleton { get; set; }
        }
        
        public class PMOHeader
        {
            public readonly byte[] Magic = [0x50, 0x4D, 0x4F, 0x00];
            public byte Number { get; set; }
            public byte Group { get; set; }
            public byte Version { get; set; }
            public byte TexCount { get; set; }
            public UInt16 Flags { get; set; }
            public uint SkeletonOffset { get; set; }
            public uint MeshOffset0 { get; set; }
            public UInt16 TriCount { get; set; }
            public UInt16 VTXCount { get; set; }
            public float Scale { get; set; }
            public uint MeshOffset1 { get; set; }
            public float[,] BoundBox { get; set; } = new float[4, 8];
        }

        public class PMOMesh
        {
            public UInt16 VTXCount { get; set; }
            public sbyte TexID { get; set; }
            public byte VTXSize { get; set; }
            public uint VTXFlags { get; set; }
            public byte Group { get; set; }
            public byte TriStripCount { get; set; }
            public byte[] BoneIndecies { get; set; } = new byte[8];
            public uint Color { get; set; }
        }
        
        public class PMOTexture
        {
            public byte[] CTT { get; set; }
            public uint Offset { get; set; }
            public string Name { get; set; }
            public float TilingX { get; set; }
            public float TilingY { get; set; }
        }

        public class PMOSkeleton
        {
            public readonly byte[] Magic = [0x42, 0x4F, 0x4E, 0x00];
            public UInt16 BoneCount { get; set; }
            public UInt16 SkinBones { get; set; }
            public UInt16 SkinBonesInitIndex { get; set; }
        }
        
        public class PMOJoint
        {
            public UInt16 BoneIndex { get; set; }
            public UInt16 ParentIndex { get; set; }
            public UInt16 SkinIndex { get; set; }
            public string BoneName { get; set; }
            public Matrix4x4 Transform { get; set; }
            public Matrix4x4 InvTransform { get; set; }
        }

        public static PMOFile Load(string input)
        {
            byte[] data = System.IO.File.ReadAllBytes(input);
            int o = 0x4;
            PMOFile pmo = new PMOFile();
            pmo.Header = new PMOHeader();
            pmo.Meshes = new List<PMOMesh>();
            pmo.Textures = new List<PMOTexture>();
            pmo.Skeleton = new PMOSkeleton();

            pmo.Header.Number = data[o++];
            pmo.Header.Group = data[o++];
            pmo.Header.Version = data[o++];
            o++;
            pmo.Header.TexCount = data[o++];
            o++;
            pmo.Header.Flags = (UInt16)(data[o++] + (data[o++] * 0x100));
            pmo.Header.SkeletonOffset = (uint)(data[o++] + (data[o++] * 0x100) + (data[o++] * 0x10000) + (data[o++] * 0x1000000));
            pmo.Header.MeshOffset0 = (uint)(data[o++] + (data[o++] * 0x100) + (data[o++] * 0x10000) + (data[o++] * 0x1000000));
            pmo.Header.TriCount = (UInt16)(data[o++] + (data[o++] * 0x100));
            pmo.Header.VTXCount = (UInt16)(data[o++] + (data[o++] * 0x100));
            pmo.Header.Scale = BitConverter.ToSingle(data, o);
            o += 4;
            pmo.Header.MeshOffset1 = (uint)(data[o++] + (data[o++] * 0x100) + (data[o++] * 0x10000) + (data[o++] * 0x1000000));
            
            pmo.Header.BoundBox = new float[4, 8];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    pmo.Header.BoundBox[i, j] = BitConverter.ToSingle(data, o);
                    o += 4;
                }
            }
            // Done with header, load rest
            for (int i = 0; i < pmo.Header.TexCount; i++)
            {
                var tex = new PMOTexture(); 
                tex.Offset = (uint)(data[o++] + (data[o++] * 0x100) + (data[o++] * 0x10000) + (data[o++] * 0x1000000));
                byte[] nameBytes2 =
                {
                    data[o++], data[o++], data[o++], data[o++], data[o++], data[o++], data[o++], data[o++],
                    data[o++], data[o++], data[o++], data[o++]
                };
                nameBytes2 = nameBytes2.Where(b => b != 0).ToArray();
                tex.Name = System.Text.Encoding.ASCII.GetString(nameBytes2);
                tex.TilingX = BitConverter.ToSingle(data, o);
                o += 4;
                tex.TilingY = BitConverter.ToSingle(data, o);
                o += 12;
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
                pmo.Textures.Add(tex);
            }
            return pmo;
        }

        public static void ExtractAllTextures(string source)
        {
            PMOFile pmo = Load(source);
            if (pmo.Header.TexCount > 0)
            {
                string path = Path.Combine(Path.GetDirectoryName(source), Path.GetFileNameWithoutExtension(source));
                Directory.CreateDirectory(path);
                foreach (PMOTexture tex in pmo.Textures)
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
            PMOFile pmo = Load(source);
            byte[] data = File.ReadAllBytes(source);
            if (pmo.Header.TexCount > 0)
            {
                foreach (PMOTexture tex in pmo.Textures)
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
}
