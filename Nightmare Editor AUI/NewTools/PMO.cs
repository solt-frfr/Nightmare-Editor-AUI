using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nightmare_Editor.NewTools
{
    public static class PMO
    {
        public class PMOFile
        {

        }
        public class PMOHeader
        {
            public readonly byte[] Magic = [0x50, 0x4D, 0x4F, 0x00];
            public byte Number { get; set; }
            public byte Group { get; set; }
            public byte Version { get; set; }
            public UInt16 Flags { get; set; }
            public UInt16 TriCount { get; set; }
            public UInt16 VTXCount { get; set; }
            public float[,] BoundBox { get; set; } = new float[4, 8];
        }
        public class PMOTexture
        {

        }
    }
}
