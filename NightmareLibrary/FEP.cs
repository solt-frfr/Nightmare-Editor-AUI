/*using System;

namespace Nightmare_Editor;

public static class FEP
{
    public class FEPFile
    {
        public FEPHeader Header { get; set; }
        public List<PMOMesh> Meshes { get; set; }
        public List<PMOTexture> Textures { get; set; }
        public PMOSkeleton Skeleton { get; set; }
    }

    public class FEPHeader
    {
        public readonly byte[] Magic = [0x46, 0x45, 0x50];
        public byte Unknown1;
        public short MajorVersion;
        public short MinorVersion;
        public uint PointerToListOfFEDVersionCheckPointers;
        public int Flag;
        public int Size;
        public ushort Mode;
        public ushort Zone;
        public ushort fer_C;
        public ushort ed_C;
        public ushort TexCount;
        public ushort ModelCount;
        public ushort AnimCount;
        public ushort VTXCount;
        public uint FERDataPointer;
        public uint EFFECTDataPointer;
        public uint TexResourcePointer;
        public uint ModelResourcePointer;
        public uint AnimResourcePointer;
        public uint VTXListResourcePointer;
        public uint LeavesHeaderPointer;
        public uint StringIDPointer;
    }
}*/