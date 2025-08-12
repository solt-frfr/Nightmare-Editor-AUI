using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Nightmare_Editor.NewTools
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
            public uint TM2Offset { get; set; }
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
            
            return pmo;
        }
    }
}
