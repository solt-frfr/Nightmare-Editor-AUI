using System.Collections.Generic;

namespace NightmareLibrary.L2D;

public class SQ2
{
    public class Header
    {
        public readonly char[] signature = ['S', 'Q', '2', '@'];
        public string version { get; set; }
        public List<Sequence> sequence { get; set; }
        public List<Control> control { get; set; }
        public List<Animation> animation { get; set; }
        public List<Key> key { get; set; }
        public int name { get; set; }
    }

    public class Sequence
    {
        
    }
    
    public class Control
    {
        
    }
    
    public class Animation
    {
        
    }
    
    public enum AnimKind
    {
        KindParent = 0,
        KindNormal = 1,
        KindFont = 2,
        KindMax = 3
    }

    public enum AnimBlendType
    {
        BlendBlend = 0,
        BlendAdd = 1,
        BlendSub = 2
    }

    public enum AnimBitFlag
    {
        None = 0,
        DitherOff = 32,
        Bilinear = 64,
        Both = 96
    }

    public class Key
    {
        
    }

    public enum KeyValue
    {
        
    }
}