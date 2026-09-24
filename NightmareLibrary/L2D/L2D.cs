using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace NightmareLibrary.L2D
{
    public static class L2D
    {
        public class File
        {
            public Header header { get; set; }
            public List<SQ2P> sq2p { get; set; }
            public List<LY2> ly2 { get; set; }
            public List<byte[]> textures { get; set; }
            public List<int> originalTexOffsets { get; set; }
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
            public int ctt_idx { get; set; }
        }
        
        public static File Load(string input)
        {
            File file = new File();
            Header header = new Header();
            List<SQ2P> sq2Ps = new List<SQ2P>();
            List<LY2> ly2s = new List<LY2>();
            List<byte[]> textures = new List<byte[]>();
            List<int> texOffsets = new List<int>();
            byte[] data = System.IO.File.ReadAllBytes(input);
            int o = 0x4;
            byte[] verBytes =
            {
                data[o++], data[o++], data[o++], data[o++]
            };
            header.version = System.Text.Encoding.ASCII.GetString(verBytes);
            byte[] dateBytes =
            {
                data[o++], data[o++], data[o++], data[o++], data[o++], data[o++], data[o++], data[o++]
            };
            header.date = System.Text.Encoding.ASCII.GetString(verBytes);
            byte[] nameBytes =
            {
                data[o++], data[o++], data[o++], data[o++]
            };
            header.name = System.Text.Encoding.ASCII.GetString(verBytes);
            o += 12;
            file.header = header;
            int sq2pCount = data[o++] + data[o++] * 0x100 + data[o++] * 0x10000 + data[o++] * 0x1000000;
            int offsetoffset = data[o++] + data[o++] * 0x100 + data[o++] * 0x10000 + data[o++] * 0x1000000;
            for (int i = 0; i < sq2pCount; i++)
            {
                int offset = data[offsetoffset + i * 4] + data[offsetoffset + (i * 4) + 1] * 0x100 + data[offsetoffset + (i * 4) + 2] * 0x10000 + data[offsetoffset + (i * 4) + 3] * 0x1000000 + offsetoffset;
                int j = offset;
                j += 4;
                SQ2P sq2p = new SQ2P();
                byte[] verBytes2 =
                {
                    data[j++], data[j++], data[j++], data[j++]
                };
                sq2p.version = System.Text.Encoding.ASCII.GetString(verBytes);
                j += 8;
                // Next 4 bytes should be SP2 Offset
                // Next 4 bytes should be SQ2 Offset
                j += 8;
                int texOffset = data[j++] + data[j++] * 0x100 + data[j++] * 0x10000 + data[j++] * 0x1000000 + offset;
                if (!texOffsets.Contains(texOffset))
                {
                    texOffsets.Add(texOffset);
                }
                sq2p.ctt_idx = texOffsets.IndexOf(texOffset);
                sq2Ps.Add(sq2p);
            }

            for (int i = 0; i < texOffsets.Count; i++)
            {
                int j = texOffsets[i];
                byte[] ctt_header = new byte[0x80];
                for (int p = 0; p < 0x80; p++)
                {
                    ctt_header[p] = data[j++];
                }

                var attrib = CTT.GetAttributesFromHeader(ctt_header);
                int width = attrib.width;
                int height = attrib.height;
                int format = (int)attrib.format;
                int total = 0;
                if (format == 0)
                {
                    total = height * width * 4;
                }
                else if (format == 1)
                {
                    total = height * width * 3;
                }
                else if (format >= 2 && format <= 6)
                {
                    total = height * width * 2;
                }
                else if (format >= 7 && format <= 9)
                {
                    total = height * width * 1;
                }
                else if (format >= 10 && format <= 11)
                {
                    total = height * width / 2;
                }
                else if (format >= 12 && format < 13)
                {
                    total = (height / 4) * (width / 4) * 8;
                }
                else if (format <= 13)
                {
                    total = (height / 4) * (width / 4) * 8 * 2;
                }

                int l = 0;
                byte[] ctt = new byte[total];
                for (int p = 0; p < ctt.Length; p++)
                {
                    ctt[p] = data[j++];
                }
                ctt = CTT.MeldHeader(ctt_header, ctt);
                textures.Add(ctt);
            }

            file.header = header;
            file.sq2p = sq2Ps;
            file.ly2 = ly2s;
            file.textures = textures;
            file.originalTexOffsets = texOffsets;
            return file;
        }

        public static void ExtractAllTextures(string input)
        {
            File file = Load(input);
            string path = Path.Combine(Path.GetDirectoryName(input), Path.GetFileNameWithoutExtension(input));
            Directory.CreateDirectory(path);
            for (int i = 0; i < file.textures.Count; i++)
            {
                if (file.textures[i] != null)
                    System.IO.File.WriteAllBytes(Path.Combine(path, i + ".ctt"), file.textures[i]);

                var split = CTT.SplitHeader(file.textures[i]);
                var attrib = CTT.GetAttributesFromHeader(split.header);
                using (var image = CTT.Deswizzle(split.data, attrib.width, attrib.height, (int)attrib.format))
                {
                    image.SaveAsPng(Path.Combine(path, i + "." + attrib.format.ToString() + ".png"));
                }
            }
        }

        public static void ReplaceAllTextures(string input)
        {
            File file = Load(input);
            byte[] data = System.IO.File.ReadAllBytes(input);
            string path = Path.Combine(Path.GetDirectoryName(input), Path.GetFileNameWithoutExtension(input));
            try
            {
                for (int i = 0; i < file.textures.Count; i++)
                {
                    byte[] ctt = System.IO.File.ReadAllBytes(Path.Combine(path, i + ".ctt"));
                    for (int j = 0; j < file.textures[i].Length; j++)
                    {
                        file.textures[i][j] = ctt[j];
                        data[file.originalTexOffsets[i] + j] = ctt[j];
                    }
                }
                System.IO.File.WriteAllBytes(input, data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
