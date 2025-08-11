using System;
using System.Collections.Generic;
using System.Linq;
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
            
        }
    }
}
