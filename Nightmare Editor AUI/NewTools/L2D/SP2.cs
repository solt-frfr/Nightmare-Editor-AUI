using System;
using System.Collections.Generic;
using System.Numerics;

namespace Nightmare_Editor.NewTools.L2D;

public class SP2
{
    public class Header
    {
        public readonly char[] signature = ['S', 'P', '2', '@'];
        public string version { get; set; }
        public List<Parts> parts { get; set; }
        public List<Group> group { get; set; }
        public List<Sprite> sprite { get; set; }
    }
        
    public class Parts
    {
        public int[] UV0 { get; set; }
        public int[] UV1 { get; set; }
        public uint[] RGBA { get; set; }
    }

    public class Group
    {
        public int[] XY0 { get; set; }
        public int[] XY1 { get; set; }
        public UInt16 parts_idx { get; set; }
        public GroupAttribute attribute { get; set; }
    }

    public enum GroupAttribute
    {
        ATTR_XYUV = 0x100,
        ATTR_SCISSOR_ON = 0x200,
        ATTR_SCISSOR_OFF = 0x400
    }
        
    public class Sprite
    {
        public UInt16 group_value { get; set; }
        public UInt16 group_idx { get; set; }
    }
}