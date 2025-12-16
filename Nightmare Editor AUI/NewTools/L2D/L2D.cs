using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nightmare_Editor.NewTools.L2D
{
    public static class L2D
    {
        public class File
        {
            public Header header { get; set; }
            public List<SQ2P> sq2p { get; set; }
        }
        
        public class Header
        {
            public readonly char[] signature = ['L', '2', 'D', '@'];
            public string version { get; set; }
            public string date { get; set; }
            public string name { get; set; }
        }
        
        public class SQ2P
        {
            public readonly char[] signature = ['S', 'Q', '2', 'P'];
            public string version { get; set; }
            public SP2 sp2 { get; set; }
            public SQ2 sq2 { get; set; }
            public byte[] ctt { get; set; }
        }
        
        public static void Load()
        {

        }
    }
}
