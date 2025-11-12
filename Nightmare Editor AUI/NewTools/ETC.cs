using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System.Reflection;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using SixLabors.ImageSharp.Drawing.Processing;
using Color = ExCSS.Color;
using static Nightmare_Editor.NewTools.CTT;

namespace Nightmare_Editor.NewTools
{

    public static class ETC
    {
        public class ETC1Block
        {
            public int offsetset1 { get; set; }
            public int offsetset2 { get; set; }
            public byte blue { get; set; }
            public byte green { get; set; }
            public byte red { get; set; }
            public bool flip { get; set; }
            public bool diff { get; set; }
            public bool[] big { get; set; }
            public bool[] sub { get; set; }
        }

        public class ETC1HalfBlock
        {
            public List<ETC1Pixel> Pixels { get; set; }
            public int Score { get; set; }
            public int OffsetSet { get; set; }
            public byte[] Color { get; set; }
        }

        public class ETC1Pixel
        {
            public byte[] Color { get; set; }
            public byte[] Reduced { get; set; }
            public byte Gray { get; set; }
            public bool Big { get; set; }
            public bool Sub { get; set; }
        }

public static ETC1Block BruteForce(byte[] colorBlock)
        {
            ETC1Block main = new ETC1Block();
            byte[] u_block = new byte[24];
            byte[] d_block = new byte[24];
            byte[] l_block = new byte[24];
            byte[] r_block = new byte[24];
            int u_o = 0;
            int d_o = 0;
            int l_o = 0;
            int r_o = 0;
            for (int i = 0; i < 16 * 3; i+=3)
            {
                if (i % 12 < 6)
                {
                    u_block[u_o++] = colorBlock[i];
                    u_block[u_o++] = colorBlock[i + 1];
                    u_block[u_o++] = colorBlock[i + 2];
                }
                else
                {
                    d_block[d_o++] = colorBlock[i];
                    d_block[d_o++] = colorBlock[i + 1];
                    d_block[d_o++] = colorBlock[i + 2];
                }
                if (i < 24)
                {
                    l_block[l_o++] = colorBlock[i];
                    l_block[l_o++] = colorBlock[i + 1];
                    l_block[l_o++] = colorBlock[i + 2];
                }
                else
                {
                    r_block[r_o++] = colorBlock[i];
                    r_block[r_o++] = colorBlock[i + 1];
                    r_block[r_o++] = colorBlock[i + 2];
                }
            }
            ETC1HalfBlock u_pixels = PixelMaker(u_block, true);
            ETC1HalfBlock d_pixels = PixelMaker(d_block, true);
            ETC1HalfBlock l_pixels = PixelMaker(l_block, false);
            ETC1HalfBlock r_pixels = PixelMaker(r_block, false);


            byte b1 = 0;
            byte g1 = 0;
            byte r1 = 0;
            byte b2 = 0;
            byte g2 = 0;
            byte r2 = 0;
            main.big = new bool[16];
            main.sub = new bool[16];
            if (u_pixels.Score + d_pixels.Score < l_pixels.Score + r_pixels.Score)
            {
                main.flip = true;
                main.offsetset1 = u_pixels.OffsetSet;
                main.offsetset2 = d_pixels.OffsetSet;
                b1 = u_pixels.Color[0];
                g1 = u_pixels.Color[1];
                r1 = u_pixels.Color[2];
                b2 = d_pixels.Color[0];
                g2 = d_pixels.Color[1];
                r2 = d_pixels.Color[2];
                int j = 0;
                int k = 0;
                // It has come to my attention that I have largely forgotten about the letter k.
                for (int i = 0; i < 16; i++)
                {
                    if (i % 4 < 2)
                    {
                        main.big[i] = u_pixels.Pixels[j].Big;
                        main.sub[i] = u_pixels.Pixels[j++].Sub;
                    }
                    else
                    {
                        main.big[i] = d_pixels.Pixels[k].Big;
                        main.sub[i] = d_pixels.Pixels[k++].Sub;
                    }
                }
            }
            else
            {
                main.flip = false;
                main.offsetset1 = l_pixels.OffsetSet;
                main.offsetset2 = r_pixels.OffsetSet;
                b1 = l_pixels.Color[0];
                g1 = l_pixels.Color[1];
                r1 = l_pixels.Color[2];
                b2 = r_pixels.Color[0];
                g2 = r_pixels.Color[1];
                r2 = r_pixels.Color[2];
                int j = 0;
                int k = 0;
                for (int i = 0; i < 16; i++)
                {
                    if (i < 8)
                    {
                        main.big[i] = l_pixels.Pixels[j].Big;
                        main.sub[i] = l_pixels.Pixels[j++].Sub;
                    }
                    else
                    {
                        main.big[i] = r_pixels.Pixels[k].Big;
                        main.sub[i] = r_pixels.Pixels[k++].Sub;
                    }
                }
            }

            b1 = (byte)(b1 >> 3);
            g1 = (byte)(g1 >> 3);
            r1 = (byte)(r1 >> 3);
            b2 = (byte)(b2 >> 3);
            g2 = (byte)(g2 >> 3);
            r2 = (byte)(r2 >> 3);

            if ((b2 >= b1 - 4 && b2 < b1 + 4) && (g2 >= g1 - 4 && g2 < g1 + 4) && (r2 >= r1 - 4 && r2 < r1 + 4))
            {
                main.diff = true;
                int b2i = (b2 - b1);
                int g2i = (g2 - g1);
                int r2i = (r2 - r1);
                if (b2i < 0)
                {
                    b2i += 8;
                }
                if (g2i < 0)
                {
                    g2i += 8;
                }
                if (r2i < 0)
                {
                    r2i += 8;
                }

                b2 = (byte)b2i;
                g2 = (byte)g2i;
                r2 = (byte)r2i;

                main.blue = (byte)((b1 << 3) + b2);
                main.green = (byte)((g1 << 3) + g2);
                main.red = (byte)((r1 << 3) + r2);
            }
            else
            {
                main.diff = false;
                
                if (main.flip)
                {
                    u_pixels = PixelMaker(u_block, true, false);
                    d_pixels = PixelMaker(d_block, true, false);
                    main.offsetset1 = u_pixels.OffsetSet;
                    main.offsetset2 = d_pixels.OffsetSet;
                    int j = 0;
                    int k = 0;
                    for (int i = 0; i < 16; i++)
                    {
                        if (i % 4 < 2)
                        {
                            main.big[i] = u_pixels.Pixels[j].Big;
                            main.sub[i] = u_pixels.Pixels[j++].Sub;
                        }
                        else
                        {
                            main.big[i] = d_pixels.Pixels[k].Big;
                            main.sub[i] = d_pixels.Pixels[k++].Sub;
                        }
                    }
                    b1 = (byte)(u_pixels.Color[0] >> 4);
                    g1 = (byte)(u_pixels.Color[1] >> 4);
                    r1 = (byte)(u_pixels.Color[2] >> 4);
                    b2 = (byte)(d_pixels.Color[0] >> 4);
                    g2 = (byte)(d_pixels.Color[1] >> 4);
                    r2 = (byte)(d_pixels.Color[2] >> 4);
                
                    main.blue = (byte)((b1 << 4) + b2);
                    main.green = (byte)((g1 << 4) + g2);
                    main.red = (byte)((r1 << 4) + r2);
                }
                else
                {
                    l_pixels = PixelMaker(l_block, false, false);
                    r_pixels = PixelMaker(r_block, false, false);
                    main.offsetset1 = l_pixels.OffsetSet;
                    main.offsetset2 = r_pixels.OffsetSet;
                    int j = 0;
                    int k = 0;
                    for (int i = 0; i < 16; i++)
                    {
                        if (i < 8)
                        {
                            main.big[i] = l_pixels.Pixels[j].Big;
                            main.sub[i] = l_pixels.Pixels[j++].Sub;
                        }
                        else
                        {
                            main.big[i] = r_pixels.Pixels[k].Big;
                            main.sub[i] = r_pixels.Pixels[k++].Sub;
                        }
                    }
                    b1 = (byte)(l_pixels.Color[0] >> 4);
                    g1 = (byte)(l_pixels.Color[1] >> 4);
                    r1 = (byte)(l_pixels.Color[2] >> 4);
                    b2 = (byte)(r_pixels.Color[0] >> 4);
                    g2 = (byte)(r_pixels.Color[1] >> 4);
                    r2 = (byte)(r_pixels.Color[2] >> 4);
                
                    main.blue = (byte)((b1 << 4) + b2);
                    main.green = (byte)((g1 << 4) + g2);
                    main.red = (byte)((r1 << 4) + r2);
                }
            }
            return main;
        }

        public static ETC1HalfBlock PixelMaker(byte[] colorBlock, bool flip, bool diffbit = true)
        {
            int width = 4;
            int height = 4;
            if (flip)
            {
                height = 2;
            }
            else
            {
                width = 2;
            }

            ETC1HalfBlock halfblock = new ETC1HalfBlock();
            List<ETC1Pixel> pixels = new List<ETC1Pixel>();
            List<int> uniqueColors = new List<int>();
            for (int i = 0; i < colorBlock.Length; i += 3)
            {
                ETC1Pixel pixel = new ETC1Pixel();
                pixel.Color = new byte[3];
                pixel.Color[0] = colorBlock[i];
                pixel.Color[1] = colorBlock[i + 1];
                pixel.Color[2] = colorBlock[i + 2];
                pixels.Add(pixel);
                int colorKey = (colorBlock[i] << 16) | (colorBlock[i + 1] << 8) | colorBlock[i + 2];
                if (!uniqueColors.Contains(colorKey))
                {
                    uniqueColors.Add(colorKey);
                }
            }

            if (uniqueColors.Count <= 4)
            {
                for (int i = 0; i < pixels.Count; i++)
                {
                    pixels[i].Reduced = new byte[3];
                    pixels[i].Reduced[0] = pixels[i].Color[0];
                    pixels[i].Reduced[1] = pixels[i].Color[1];
                    pixels[i].Reduced[2] = pixels[i].Color[2];
                    pixels[i].Gray = (byte)(0.299 * pixels[i].Color[2] + 0.587 * pixels[i].Color[1] +
                                            0.114 * pixels[i].Color[0]);
                }
            }
            else
            {
                using (Image<Rgb24> block = new Image<Rgb24>(width, height))
                {
                    for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                    {
                        Rgb24 bg = new Rgb24(pixels[y + (x * height)].Color[2], pixels[y + (x * height)].Color[1],
                            pixels[y + (x * height)].Color[0]);
                        block.Mutate(ctx => { ctx.Fill(bg, new Rectangle(x, y, 1, 1)); });
                    }

                    // var quantizer = new WuQuantizer(new QuantizerOptions { MaxColors = 4 });
                    var quantizer = new OctreeQuantizer(new QuantizerOptions { MaxColors = 4 });
                    using (Image<Rgb24> reduced = block.Clone(ctx => ctx.Quantize(quantizer)))
                    {
                        for (int x = 0; x < width; x++)
                        for (int y = 0; y < height; y++)
                        {
                            ETC1Pixel pixel = pixels[y + (x * height)];
                            pixel.Reduced = new byte[3];
                            pixel.Reduced[0] = reduced[x, y].B;
                            pixel.Reduced[1] = reduced[x, y].G;
                            pixel.Reduced[2] = reduced[x, y].R;
                            pixel.Gray = (byte)(0.299 * reduced[x, y].R + 0.587 * reduced[x, y].G +
                                                0.114 * reduced[x, y].B);
                        }

                        // The quantizer is reducing the colors too much. It's taking blocks that are already 4 colors big and reducing it 
                    };
                };
            }

            List<byte[]> colors = new List<byte[]>();
            for (int i = 0; i < pixels.Count; i++)
            {
                byte[] color = new byte[4];
                color[0] = pixels[i].Color[0];
                color[1] = pixels[i].Color[1];
                color[2] = pixels[i].Color[2];
                color[3] = pixels[i].Gray;
                if (!colors.Any(c => c.SequenceEqual(color)))
                {
                    colors.Add(color);
                }
            }
            if (colors.Count == 1)
            {
                int best_score = Int32.MaxValue;
                int set = 0;
                bool sub = false;
                bool big = false;
                for (int j = 0; j < 4; j++)
                {
                    bool try_sub = false;
                    bool try_big = false;
                    if (j == 1 || j == 3)
                    {
                        try_sub = true;
                    }
                    if (j == 2 || j == 3)
                    {
                        try_big = true;
                    }

                    int try_big_ = 0;
                    int try_sub_ = -1;

                    if (try_big)
                    {
                        try_big_ = 1;
                    }
                    if (try_sub)
                    {
                        try_sub_ = 1;
                    }
                        
                    for (int i = 0; i < 8; i++)
                    {
                        int b8 = colors[0][0] + (try_sub_ * ETC1OffTable(i)[try_big_]);
                        int g8 = colors[0][1] + (try_sub_ * ETC1OffTable(i)[try_big_]);
                        int r8 = colors[0][2] + (try_sub_ * ETC1OffTable(i)[try_big_]);
                        int b4 = b8 >> 3;
                        int b8_2 = (b4 << 3) + (b4 >> 2);
                        int g4 = g8 >> 3;
                        int g8_2 = (g4 << 3) + (g4 >> 2);
                        int r4 = r8 >> 3;
                        int r8_2 = (r4 << 3) + (r4 >> 2);
                        if (!diffbit)
                        {
                            b4 = b8 >> 4;
                            b8_2 = (b4 << 4) + (b4);
                            g4 = g8 >> 4;
                            g8_2 = (g4 << 4) + (g4);
                            r4 = r8 >> 4;
                            r8_2 = (r4 << 4) + (r4);
                        }

                        int score = Math.Abs(b8 - b8_2) + Math.Abs(g8 - g8_2) + Math.Abs(r8 - r8_2);
                        if (score < best_score)
                        {
                            best_score = score;
                            set = i;
                            sub = try_sub;
                            big = try_big;
                        }
                    }
                }
                halfblock.OffsetSet = set;
                for (int i = 0; i < pixels.Count; i++)
                {
                    pixels[i].Big = big;
                    pixels[i].Sub = sub;
                }
            }
            else if (colors.Count == 2)
            {
                int diff = Math.Abs(colors[1][3] - colors[0][3]);
                colors.Sort((a, b) => a[3].CompareTo(b[3]));
                var numbers = new[] {
                    6, 10, 4, 16,
                    12, 22, 10, 34,
                    20, 38, 18, 58,
                    29, 55, 26, 84,
                    42, 78, 36, 120,
                    56, 104, 48, 160,
                    73, 139, 66, 212,
                    136, 230, 94, 366
                };
                int closest = numbers
                    .OrderBy(n => Math.Abs(n - diff))
                    .First();
                int set = Array.IndexOf(numbers, closest) % 4;

                halfblock.OffsetSet = (int)Math.Floor((decimal)Array.IndexOf(numbers, closest) / 4);

                var pix1 = new ETC1Pixel();
                var pix2 = new ETC1Pixel();
                for (int i = 0; i < pixels.Count; i++)
                {
                    int small = 1;
                    if (colors[0][3] + diff == colors[1][3])
                    {
                        small = 0;
                    }
                    if (pixels[i].Gray == colors[small][3])
                    {
                        if (set == 0)
                        {
                            pixels[i].Sub = false;
                            pixels[i].Big = false;
                        }
                        else if (set == 1)
                        {
                            pixels[i].Sub = true;
                            pixels[i].Big = false;
                        }
                        else if (set == 2)
                        {
                            pixels[i].Sub = true;
                            pixels[i].Big = false;
                        }
                        else
                        {
                            pixels[i].Sub = true;
                            pixels[i].Big = true;
                        }
                        pix1 = pixels[i];
                    }
                    else
                    {
                        if (set == 0)
                        {
                            pixels[i].Sub = false;
                            pixels[i].Big = true;
                        }
                        else if (set == 1)
                        {
                            pixels[i].Sub = false;
                            pixels[i].Big = true;
                        }
                        else if (set == 2)
                        {
                            pixels[i].Sub = false;
                            pixels[i].Big = false;
                        }
                        else
                        {
                            pixels[i].Sub = false;
                            pixels[i].Big = true;
                        }
                        pix2 = pixels[i];
                    }
                }

                int best_score = Int32.MaxValue;
                int best_score2 = Int32.MaxValue;
                for (int i = 0; i < pixels.Count; i++)
                {
                    int try_big_ = 0;
                    int try_sub_ = 1;
                    var pixel = pix1;
                    if (i == 1)
                    {
                        pixel = pix2;
                    }

                    if (pixel.Big)
                    {
                        try_big_ = 1;
                    }

                    if (pixel.Sub)
                    {
                        try_sub_ = -1;
                    }
                    int b8 = colors[0][0] + (try_sub_ * ETC1OffTable(set)[try_big_]);
                    int g8 = colors[0][1] + (try_sub_ * ETC1OffTable(set)[try_big_]);
                    int r8 = colors[0][2] + (try_sub_ * ETC1OffTable(set)[try_big_]);
                    int b4 = b8 >> 3;
                    int b8_2 = (b4 << 3) + (b4 >> 2);
                    int g4 = g8 >> 3;
                    int g8_2 = (g4 << 3) + (g4 >> 2);
                    int r4 = r8 >> 3;
                    int r8_2 = (r4 << 3) + (r4 >> 2);
                    if (!diffbit)
                    {
                        b4 = b8 >> 4;
                        b8_2 = (b4 << 4) + (b4);
                        g4 = g8 >> 4;
                        g8_2 = (g4 << 4) + (g4);
                        r4 = r8 >> 4;
                        r8_2 = (r4 << 4) + (r4);
                    }

                    int score = Math.Abs(b8 - b8_2) + Math.Abs(g8 - g8_2) + Math.Abs(r8 - r8_2);
                    if (i == 1)
                    {
                        best_score2 = score;
                    }
                    else
                    {
                        best_score = score;
                    }
                }

                for (int i = 0; i < pixels.Count; i++)
                {
                    int small = 1;
                    if (colors[0][3] + diff == colors[1][3])
                    {
                        small = 0;
                    }
                    if (pixels[i].Gray == colors[small][3])
                    {
                        if (set == 0)
                        {
                            pixels[i].Sub = true;
                            pixels[i].Big = true;
                        }
                        else if (set == 1)
                        {
                            pixels[i].Sub = true;
                            pixels[i].Big = true;
                        }
                        else if (set == 2)
                        {
                            pixels[i].Sub = true;
                            pixels[i].Big = false;
                        }
                        else
                        {
                            pixels[i].Sub = true;
                            pixels[i].Big = true;
                        }
                        pix1 = pixels[i];
                    }
                    else
                    {
                        if (set == 0)
                        {
                            pixels[i].Sub = true;
                            pixels[i].Big = false;
                        }
                        else if (set == 1)
                        {
                            pixels[i].Sub = false;
                            pixels[i].Big = false;
                        }
                        else if (set == 2)
                        {
                            pixels[i].Sub = false;
                            pixels[i].Big = false;
                        }
                        else
                        {
                            pixels[i].Sub = false;
                            pixels[i].Big = true;
                        }
                        pix2 = pixels[i];
                    }
                }
                
                int best_score3 = Int32.MaxValue;
                int best_score4 = Int32.MaxValue;
                for (int i = 0; i < pixels.Count; i++)
                {
                    int try_big_ = 0;
                    int try_sub_ = 1;
                    var pixel = pix1;
                    if (i == 1)
                    {
                        pixel = pix2;
                    }

                    if (pixel.Big)
                    {
                        try_big_ = 1;
                    }

                    if (pixel.Sub)
                    {
                        try_sub_ = -1;
                    }
                    int b8 = colors[0][0] + (try_sub_ * ETC1OffTable(set)[try_big_]);
                    int g8 = colors[0][1] + (try_sub_ * ETC1OffTable(set)[try_big_]);
                    int r8 = colors[0][2] + (try_sub_ * ETC1OffTable(set)[try_big_]);
                    int b4 = b8 >> 3;
                    int b8_2 = (b4 << 3) + (b4 >> 2);
                    int g4 = g8 >> 3;
                    int g8_2 = (g4 << 3) + (g4 >> 2);
                    int r4 = r8 >> 3;
                    int r8_2 = (r4 << 3) + (r4 >> 2);
                    if (!diffbit)
                    {
                        b4 = b8 >> 4;
                        b8_2 = (b4 << 4) + (b4);
                        g4 = g8 >> 4;
                        g8_2 = (g4 << 4) + (g4);
                        r4 = r8 >> 4;
                        r8_2 = (r4 << 4) + (r4);
                    }

                    int score = Math.Abs(b8 - b8_2) + Math.Abs(g8 - g8_2) + Math.Abs(r8 - r8_2);
                    if (i == 1)
                    {
                        best_score4 = score;
                    }
                    else
                    {
                        best_score3 = score;
                    }
                }

                if (best_score + best_score2 < best_score3 + best_score4)
                {
                    for (int i = 0; i < pixels.Count; i++)
                    {
                        int small = 1;
                        if (colors[0][3] + diff == colors[1][3])
                        {
                            small = 0;
                        }
                        if (pixels[i].Gray == colors[small][3])
                        {
                            if (set == 0)
                            {
                                pixels[i].Sub = false;
                                pixels[i].Big = false;
                            }
                            else if (set == 1)
                            {
                                pixels[i].Sub = true;
                                pixels[i].Big = false;
                            }
                            else if (set == 2)
                            {
                                pixels[i].Sub = true;
                                pixels[i].Big = false;
                            }
                            else
                            {
                                pixels[i].Sub = true;
                                pixels[i].Big = true;
                            }
                        }
                        else
                        {
                            if (set == 0)
                            {
                                pixels[i].Sub = false;
                                pixels[i].Big = true;
                            }
                            else if (set == 1)
                            {
                                pixels[i].Sub = false;
                                pixels[i].Big = true;
                            }
                            else if (set == 2)
                            {
                                pixels[i].Sub = false;
                                pixels[i].Big = false;
                            }
                            else
                            {
                                pixels[i].Sub = false;
                                pixels[i].Big = true;
                            }
                        }
                    }
                }
            }
            else if (colors.Count == 3)
            {
                int best_score = int.MaxValue;
                int blank = 0;
                colors.Sort((a, b) => a[3].CompareTo(b[3]));
                int[] diff = new int[]
                    { colors[1][3] - colors[0][3], colors[2][3] - colors[1][3] };
                for (int i = 0; i < 8; i++)
                {
                    int score = Math.Abs((ETC1OffTable(i)[1] - ETC1OffTable(i)[0]) - diff[0]) + Math.Abs((ETC1OffTable(i)[0] * 2) - diff[1]);
                    if (score < best_score)
                    {
                        best_score = score;
                        halfblock.OffsetSet = i;
                        blank = 3;
                    }
                    score = Math.Abs((ETC1OffTable(i)[1] + ETC1OffTable(i)[0]) - diff[0]) + Math.Abs((ETC1OffTable(i)[1] - ETC1OffTable(i)[0]) - diff[1]);
                    if (score < best_score)
                    {
                        best_score = score;
                        halfblock.OffsetSet = i;
                        blank = 2;
                    }
                    score = Math.Abs((ETC1OffTable(i)[1] - ETC1OffTable(i)[0]) - diff[0]) + Math.Abs((ETC1OffTable(i)[1] + ETC1OffTable(i)[0]) - diff[1]);
                    if (score < best_score)
                    {
                        best_score = score;
                        halfblock.OffsetSet = i;
                        blank = 1;
                    }
                    score = Math.Abs((ETC1OffTable(i)[0] * 2) - diff[0]) + Math.Abs((ETC1OffTable(i)[1] - ETC1OffTable(i)[0]) - diff[1]);
                    if (score < best_score)
                    {
                        best_score = score;
                        halfblock.OffsetSet = i;
                        blank = 0;
                    }
                }
                for (int i = 0; i < pixels.Count; i++)
                {
                    if (blank == 0)
                    {
                        if (pixels[i].Gray == colors[0][3])
                        {
                            pixels[i].Sub = true;
                            pixels[i].Big = false;
                        }
                        else if (pixels[i].Gray == colors[1][3])
                        {
                            pixels[i].Sub = false;
                            pixels[i].Big = false;
                        }
                        else
                        {
                            pixels[i].Sub = false;
                            pixels[i].Big = true;
                        }
                    }
                    else if (blank == 2)
                    {
                        if (pixels[i].Gray == colors[0][3])
                        {
                            pixels[i].Sub = true;
                            pixels[i].Big = true;
                        }
                        else if (pixels[i].Gray == colors[1][3])
                        {
                            pixels[i].Sub = false;
                            pixels[i].Big = false;
                        }
                        else
                        {
                            pixels[i].Sub = false;
                            pixels[i].Big = true;
                        }
                    }
                    else if (blank == 1)
                    {
                        if (pixels[i].Gray == colors[0][3])
                        {
                            pixels[i].Sub = true;
                            pixels[i].Big = true;
                        }
                        else if (pixels[i].Gray == colors[1][3])
                        {
                            pixels[i].Sub = true;
                            pixels[i].Big = false;
                        }
                        else
                        {
                            pixels[i].Sub = false;
                            pixels[i].Big = true;
                        }
                    }
                    else
                    {
                        if (pixels[i].Gray == colors[0][3])
                        {
                            pixels[i].Sub = true;
                            pixels[i].Big = true;
                        }
                        else if (pixels[i].Gray == colors[1][3])
                        {
                            pixels[i].Sub = true;
                            pixels[i].Big = false;
                        }
                        else
                        {
                            pixels[i].Sub = false;
                            pixels[i].Big = false;
                        }
                    }
                }
            }
            else
            {
                int best_score = int.MaxValue;
                colors.Sort((a, b) => a[3].CompareTo(b[3]));
                int[] diff = new int[]
                    { colors[1][3] - colors[0][3], colors[2][3] - colors[1][3], colors[3][3] - colors[2][3] };
                for (int i = 0; i < 8; i++)
                {
                    int score = Math.Abs((ETC1OffTable(i)[1] - ETC1OffTable(i)[0]) - diff[0]) + Math.Abs((ETC1OffTable(i)[0] * 2) - diff[1]) + Math.Abs((ETC1OffTable(i)[1] - ETC1OffTable(i)[0]) - diff[2]);
                    if (score < best_score)
                    {
                        best_score = score;
                        halfblock.OffsetSet = i;
                    }
                }
                for (int i = 0; i < pixels.Count; i++)
                {
                    if (pixels[i].Gray == colors[0][3])
                    {
                        pixels[i].Sub = true;
                        pixels[i].Big = true;
                    }
                    else if (pixels[i].Gray == colors[1][3])
                    {
                        pixels[i].Sub = true;
                        pixels[i].Big = false;
                    }
                    else if (pixels[i].Gray == colors[2][3])
                    {
                        pixels[i].Sub = false;
                        pixels[i].Big = false;
                    }
                    else
                    {
                        pixels[i].Sub = false;
                        pixels[i].Big = true;
                    }
                }
            }

            List<ETC1Pixel> color_ref = new List<ETC1Pixel>();
            for (int i = 0; i < pixels.Count; i++)
            {
                if (!color_ref.Any(p => p.Gray == pixels[i].Gray))
                {
                    color_ref.Add(pixels[i]);
                }
            }

            int r = 0;
            int g = 0;
            int b = 0;
            halfblock.Color = new byte[3];
            for (int i = 0; i < color_ref.Count; i++)
            {
                int sub = 1;
                int big = 0;
                if (color_ref[i].Big)
                {
                    big = 1;
                }
                if (color_ref[i].Sub)
                {
                    sub = -1;
                }

                b += color_ref[i].Reduced[0] - (sub * ETC1OffTable(halfblock.OffsetSet)[big]);
                g += color_ref[i].Reduced[1] - (sub * ETC1OffTable(halfblock.OffsetSet)[big]);
                r += color_ref[i].Reduced[2] - (sub * ETC1OffTable(halfblock.OffsetSet)[big]);
            }
            halfblock.Color[0] = (byte)Math.Clamp((b / color_ref.Count), 0, 0xFF);
            halfblock.Color[1] = (byte)Math.Clamp((g / color_ref.Count), 0, 0xFF);
            halfblock.Color[2] = (byte)Math.Clamp((r / color_ref.Count), 0, 0xFF);
            halfblock.Pixels = pixels;
            
            
            for (int i = 0; i < pixels.Count; i++)
            {
                int sub = 1;
                int big = 0;
                int r_score = 0;
                int g_score = 0;
                int b_score = 0;
                if (pixels[i].Big)
                {
                    big = 1;
                }
                if (pixels[i].Sub)
                {
                    sub = -1;
                }
                b_score = Math.Abs((pixels[i].Color[0] - (halfblock.Color[0] + (sub * ETC1OffTable(halfblock.OffsetSet)[big]))));
                g_score = Math.Abs((pixels[i].Color[1] - (halfblock.Color[1] + (sub * ETC1OffTable(halfblock.OffsetSet)[big]))));
                r_score = Math.Abs((pixels[i].Color[2] - (halfblock.Color[2] + (sub * ETC1OffTable(halfblock.OffsetSet)[big]))));
                halfblock.Score += b_score + g_score + r_score;
            }
            
            return halfblock;
        }


        /// <summary>
        /// Converts RGBA8888 bytes into ETC1 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw ETC1 data, with a CTT Header.</returns>
        /*
        public static byte[] ETC1pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.ETC1);
            byte[] newData = new byte[((ogData.Length - 0x80) / 8) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int l = 0x80;
            for (int i = 0x80; i < newData.Length; i += 8)
            {
                byte[] colorBlock = new byte[16 * 3];

                l++;
                colorBlock[0 * 3 + 0] = ogData[l++];
                colorBlock[0 * 3 + 1] = ogData[l++];
                colorBlock[0 * 3 + 2] = ogData[l++];
                l++;
                colorBlock[4 * 3 + 0] = ogData[l++];
                colorBlock[4 * 3 + 1] = ogData[l++];
                colorBlock[4 * 3 + 2] = ogData[l++];
                l++;
                colorBlock[8 * 3 + 0] = ogData[l++];
                colorBlock[8 * 3 + 1] = ogData[l++];
                colorBlock[8 * 3 + 2] = ogData[l++];
                l++;
                colorBlock[12 * 3 + 0] = ogData[l++];
                colorBlock[12 * 3 + 1] = ogData[l++];
                colorBlock[12 * 3 + 2] = ogData[l++];

                l++;
                colorBlock[1 * 3 + 0] = ogData[l++];
                colorBlock[1 * 3 + 1] = ogData[l++];
                colorBlock[1 * 3 + 2] = ogData[l++];
                l++;
                colorBlock[5 * 3 + 0] = ogData[l++];
                colorBlock[5 * 3 + 1] = ogData[l++];
                colorBlock[5 * 3 + 2] = ogData[l++];
                l++;
                colorBlock[9 * 3 + 0] = ogData[l++];
                colorBlock[9 * 3 + 1] = ogData[l++];
                colorBlock[9 * 3 + 2] = ogData[l++];
                l++;
                colorBlock[13 * 3 + 0] = ogData[l++];
                colorBlock[13 * 3 + 1] = ogData[l++];
                colorBlock[13 * 3 + 2] = ogData[l++];

                l++;
                colorBlock[2 * 3 + 0] = ogData[l++];
                colorBlock[2 * 3 + 1] = ogData[l++];
                colorBlock[2 * 3 + 2] = ogData[l++];
                l++;
                colorBlock[6 * 3 + 0] = ogData[l++];
                colorBlock[6 * 3 + 1] = ogData[l++];
                colorBlock[6 * 3 + 2] = ogData[l++];
                l++;
                colorBlock[10 * 3 + 0] = ogData[l++];
                colorBlock[10 * 3 + 1] = ogData[l++];
                colorBlock[10 * 3 + 2] = ogData[l++];
                l++;
                colorBlock[14 * 3 + 0] = ogData[l++];
                colorBlock[14 * 3 + 1] = ogData[l++];
                colorBlock[14 * 3 + 2] = ogData[l++];

                l++;
                colorBlock[3 * 3 + 0] = ogData[l++];
                colorBlock[3 * 3 + 1] = ogData[l++];
                colorBlock[3 * 3 + 2] = ogData[l++];
                l++;
                colorBlock[7 * 3 + 0] = ogData[l++];
                colorBlock[7 * 3 + 1] = ogData[l++];
                colorBlock[7 * 3 + 2] = ogData[l++];
                l++;
                colorBlock[11 * 3 + 0] = ogData[l++];
                colorBlock[11 * 3 + 1] = ogData[l++];
                colorBlock[11 * 3 + 2] = ogData[l++];
                l++;
                colorBlock[15 * 3 + 0] = ogData[l++];
                colorBlock[15 * 3 + 1] = ogData[l++];
                colorBlock[15 * 3 + 2] = ogData[l++];

                ETC1Block main = ETC1BruteForce(colorBlock);

                for (int o = 0; o < 16; o++)
                {
                    if (o < 8)
                    {
                        if (main.big[o])
                        {
                            newData[i] += (byte)(1 << o);
                        }
                        if (main.sub[0])
                        {
                            newData[i + 2] += (byte)(1 << o);
                        }
                    }
                    else
                    {
                        if (main.big[o])
                        {
                            newData[i + 1] += (byte)(1 << (o - 8));
                        }
                        if (main.sub[o])
                        {
                            newData[i + 3] += (byte)(1 << (o - 8));
                        }
                    }
                }
                if (main.flip)
                {
                    newData[i + 4] += 1;
                }
                if (main.diff)
                {
                    newData[i + 4] += (1 << 1);
                }
                newData[i + 4] += (byte)(main.offsetset2 << 2);
                newData[i + 4] += (byte)(main.offsetset1 << 5);
                newData[i + 5] = main.blue;
                newData[i + 6] = main.green;
                newData[i + 7] = main.red;
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into ETC1A4 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw ETC1A4 data, with a CTT Header.</returns>
        public static byte[] ETC1A4pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.ETC1A4);
            byte[] newData = new byte[((ogData.Length - 0x80) / 4) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int l = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 8)
            {
                byte[] colorBlock = new byte[16 * 3];

                colorBlock[0 * 3 + 0] = ogData[l++];
                colorBlock[0 * 3 + 1] = ogData[l++];
                colorBlock[0 * 3 + 2] = ogData[l++];
                colorBlock[4 * 3 + 0] = ogData[l++];
                colorBlock[4 * 3 + 1] = ogData[l++];
                colorBlock[4 * 3 + 2] = ogData[l++];
                colorBlock[8 * 3 + 0] = ogData[l++];
                colorBlock[8 * 3 + 1] = ogData[l++];
                colorBlock[8 * 3 + 2] = ogData[l++];
                colorBlock[12 * 3 + 0] = ogData[l++];
                colorBlock[12 * 3 + 1] = ogData[l++];
                colorBlock[12 * 3 + 2] = ogData[l++];

                colorBlock[1 * 3 + 0] = ogData[l++];
                colorBlock[1 * 3 + 1] = ogData[l++];
                colorBlock[1 * 3 + 2] = ogData[l++];
                colorBlock[5 * 3 + 0] = ogData[l++];
                colorBlock[5 * 3 + 1] = ogData[l++];
                colorBlock[5 * 3 + 2] = ogData[l++];
                colorBlock[9 * 3 + 0] = ogData[l++];
                colorBlock[9 * 3 + 1] = ogData[l++];
                colorBlock[9 * 3 + 2] = ogData[l++];
                colorBlock[13 * 3 + 0] = ogData[l++];
                colorBlock[13 * 3 + 1] = ogData[l++];
                colorBlock[13 * 3 + 2] = ogData[l++];

                colorBlock[2 * 3 + 0] = ogData[l++];
                colorBlock[2 * 3 + 1] = ogData[l++];
                colorBlock[2 * 3 + 2] = ogData[l++];
                colorBlock[6 * 3 + 0] = ogData[l++];
                colorBlock[6 * 3 + 1] = ogData[l++];
                colorBlock[6 * 3 + 2] = ogData[l++];
                colorBlock[10 * 3 + 0] = ogData[l++];
                colorBlock[10 * 3 + 1] = ogData[l++];
                colorBlock[10 * 3 + 2] = ogData[l++];
                colorBlock[14 * 3 + 0] = ogData[l++];
                colorBlock[14 * 3 + 1] = ogData[l++];
                colorBlock[14 * 3 + 2] = ogData[l++];

                colorBlock[3 * 3 + 0] = ogData[l++];
                colorBlock[3 * 3 + 1] = ogData[l++];
                colorBlock[3 * 3 + 2] = ogData[l++];
                colorBlock[7 * 3 + 0] = ogData[l++];
                colorBlock[7 * 3 + 1] = ogData[l++];
                colorBlock[7 * 3 + 2] = ogData[l++];
                colorBlock[11 * 3 + 0] = ogData[l++];
                colorBlock[11 * 3 + 1] = ogData[l++];
                colorBlock[11 * 3 + 2] = ogData[l++];
                colorBlock[15 * 3 + 0] = ogData[l++];
                colorBlock[15 * 3 + 1] = ogData[l++];
                colorBlock[15 * 3 + 2] = ogData[l++];

                ETC1Block main = ETC1BruteForce(colorBlock);

                for (int o = 0; o < 16; o++)
                {
                    if (o < 8)
                    {
                        if (main.big[o])
                        {
                            newData[i] += (byte)(1 << o);
                        }
                        if (main.sub[0])
                        {
                            newData[i + 2] += (byte)(1 << o);
                        }
                    }
                    else
                    {
                        if (main.big[o])
                        {
                            newData[i + 1] += (byte)(1 << (o - 8));
                        }
                        if (main.sub[o])
                        {
                            newData[i + 3] += (byte)(1 << (o - 8));
                        }
                    }
                }
                if (main.flip)
                {
                    newData[i + 4] += 1;
                }
                if (main.diff)
                {
                    newData[i + 4] += (1 << 1);
                }
                newData[i + 4] += (byte)(main.offsetset2 << 2);
                newData[i + 4] += (byte)(main.offsetset1 << 5);
                newData[i + 5] = main.blue;
                newData[i + 6] = main.green;
                newData[i + 7] = main.red;
            }
            return newData;
        }



        public static ETC1Block ETC1BruteForce(byte[] ogData)
        {
            ETC1Block main = new ETC1Block();
            List<int> left_b = new List<int>();
            List<int> left_g = new List<int>();
            List<int> left_r = new List<int>();
            int[] left_avg = new int[3];
            List<int> right_b = new List<int>();
            List<int> right_g = new List<int>();
            List<int> right_r = new List<int>();
            int[] right_avg = new int[3];
            List<int> top_b = new List<int>();
            List<int> top_g = new List<int>();
            List<int> top_r = new List<int>();
            int[] top_avg = new int[3];
            List<int> bottom_b = new List<int>();
            List<int> bottom_g = new List<int>();
            List<int> bottom_r = new List<int>();
            int[] bottom_avg = new int[3];
            for (int i = 0; i < 16; i++)
            {
                int b = ogData[i * 3];
                int g = ogData[i * 3 + 1];
                int r = ogData[i * 3 + 2];
                if ((i >= 8))
                {
                    if (!(right_b.Contains(b) && right_g.Contains(g) && right_r.Contains(r)))
                    {
                        right_b.Add(b);
                        right_g.Add(g);
                        right_r.Add(r);
                    }
                }
                else
                {
                    if (!(left_b.Contains(b) && left_g.Contains(g) && left_r.Contains(r)))
                    {
                        left_b.Add(b);
                        left_g.Add(g);
                        left_r.Add(r);
                    }
                }
                if (i % 4 >= 2)
                {
                    if (!(bottom_b.Contains(b) && bottom_g.Contains(g) && bottom_r.Contains(r)))
                    {
                        bottom_b.Add(b);
                        bottom_g.Add(g);
                        bottom_r.Add(r);
                    }
                }
                else
                {
                    if (!(top_b.Contains(b) && top_g.Contains(g) && top_r.Contains(r)))
                    {
                        top_b.Add(b);
                        top_g.Add(g);
                        top_r.Add(r);
                    }
                }
            }

            for (int i = 0; i < left_b.Count; i++)
            {
                left_avg[0] += (left_b[i] / left_b.Count);
                left_avg[1] += (left_g[i] / left_b.Count);
                left_avg[2] += (left_r[i] / left_b.Count);
            }
            for (int i = 0; i < right_b.Count; i++)
            {
                right_avg[0] += (right_b[i] / right_b.Count);
                right_avg[1] += (right_g[i] / right_b.Count);
                right_avg[2] += (right_r[i] / right_b.Count);
            }
            for (int i = 0; i < top_b.Count; i++)
            {
                top_avg[0] += (top_b[i] / top_b.Count);
                top_avg[1] += (top_g[i] / top_b.Count);
                top_avg[2] += (top_r[i] / top_b.Count);
            }
            for (int i = 0; i < bottom_b.Count; i++)
            {
                bottom_avg[0] += (bottom_b[i] / bottom_b.Count);
                bottom_avg[1] += (bottom_g[i] / bottom_b.Count);
                bottom_avg[2] += (bottom_r[i] / bottom_b.Count);
            }

            int[] left_score = ETC1OffGrabber(left_b, left_g, left_r, left_avg);
            int[] right_score = ETC1OffGrabber(right_b, right_g, right_r, right_avg);
            int[] top_score = ETC1OffGrabber(top_b, top_g, top_r, top_avg);
            int[] bottom_score = ETC1OffGrabber(bottom_b, bottom_g, bottom_r, bottom_avg);

            byte new_b1 = 0;
            byte new_g1 = 0;
            byte new_r1 = 0;
            byte new_b2 = 0;
            byte new_g2 = 0;
            byte new_r2 = 0;

            if (left_score[1] + right_score[1] < top_score[1] + bottom_score[1])
            {
                main.flip = false;
                new_b2 = (byte)(left_avg[0] >> 3);
                new_g2 = (byte)(left_avg[1] >> 3);
                new_r2 = (byte)(left_avg[2] >> 3);
                new_b1 = (byte)(right_avg[0] >> 3);
                new_g1 = (byte)(right_avg[1] >> 3);
                new_r1 = (byte)(right_avg[2] >> 3);
                main.offsetset2 = left_score[0];
                main.offsetset1 = right_score[0];
            }
            else
            {
                main.flip = true;
                new_b2 = (byte)(top_avg[0] >> 3);
                new_g2 = (byte)(top_avg[1] >> 3);
                new_r2 = (byte)(top_avg[2] >> 3);
                new_b1 = (byte)(bottom_avg[0] >> 3);
                new_g1 = (byte)(bottom_avg[1] >> 3);
                new_r1 = (byte)(bottom_avg[2] >> 3);
                main.offsetset2 = top_score[0];
                main.offsetset1 = bottom_score[0];
            }
            if ((new_b2 >= new_b1 - 8 && new_b2 < new_b1 + 8) && (new_g2 >= new_g1 - 8 && new_g2 < new_g1 + 8) && (new_r2 >= new_r1 - 8 && new_r2 < new_r1 + 8))
            {
                main.diff = true;
                new_b2 = (byte)(new_b2 - new_b1);
                new_g2 = (byte)(new_g2 - new_g1);
                new_r2 = (byte)(new_r2 - new_r1);
                if (new_b2 < 0)
                {
                    new_b2 += 8;
                }
                if (new_g2 < 0)
                {
                    new_g2 += 8;
                }
                if (new_r2 < 0)
                {
                    new_r2 += 8;
                }

                //main.blue = (byte)((new_b2 << 5) + new_b1);
                //main.green = (byte)((new_g2 << 5) + new_g1);
                //main.red = (byte)((new_r2 << 5) + new_r1);
                main.blue = (byte)((new_b1 << 3) + new_b2);
                main.green = (byte)((new_g1 << 3) + new_g2);
                main.red = (byte)((new_r1 << 3) + new_r2);
            }
            else
            {
                main.diff = false;
                new_b1 = (byte)(new_b1 >> 1);
                new_g1 = (byte)(new_g1 >> 1);
                new_r1 = (byte)(new_r1 >> 1);
                new_b2 = (byte)(new_b2 >> 1);
                new_g2 = (byte)(new_g2 >> 1);
                new_r2 = (byte)(new_r2 >> 1);
                main.blue = (byte)((new_b2 << 4) + new_b1);
                main.green = (byte)((new_g2 << 4) + new_g1);
                main.red = (byte)((new_r2 << 4) + new_r1);
            }

            if (main.flip)
            {
                (main.sub, main.big) = ETC1OffAssigner(ogData, left_avg, right_avg, main);
            }
            else
            {
                (main.sub, main.big) = ETC1OffAssigner(ogData, top_avg, bottom_avg, main);
            }
            return main;
        }

        /// <summary>
        /// Finds the best fitting offset table value for a list of colors.
        /// </summary>
        /// <returns>Returns two intergers, [0] being the index of the offset table, [1] being the score of how well it fits.</returns>
        public static int[] ETC1OffGrabber(List<int> b, List<int> g, List<int> r, int[] avg)
        {
            int temp = 0;
            int split_index = 0;
            int small = 0;
            int big = 0;
            int[] score = new int[8];
            int best_score = Int32.MaxValue;
            int[] return_value = new int[2];
            b.Sort();
            g.Sort();
            r.Sort();
            for (int i = 0; i < b.Count - 1; i++)
            {
                int temp_diff = (int)(Math.Abs(b[i] - b[i + 1]) + Math.Abs(g[i] - g[i + 1]) + Math.Abs(r[i] - r[i + 1]));
                if (temp_diff > temp)
                {
                    temp = temp_diff;
                    split_index = i;
                }
            }

            int small_num = 0;
            int big_num = 0;
            for (int i = 0; i < b.Count; i++)
            {
                if (i <= split_index)
                {
                    small += (Math.Abs(b[i] - avg[0]) + Math.Abs(g[i] - avg[1]) + Math.Abs(r[i] - avg[2]));
                    small_num += 1;
                }
                else
                {
                    big += (Math.Abs(b[i] - avg[0]) + Math.Abs(g[i] - avg[1]) + Math.Abs(r[i] - avg[2]));
                    big_num += 1;
                }
            }

            if (small_num > 0)
            {
                small = small / small_num;
                score[0] += Math.Abs(small - 2);
                score[1] += Math.Abs(small - 5);
                score[2] += Math.Abs(small - 9);
                score[3] += Math.Abs(small - 13);
                score[4] += Math.Abs(small - 18);
                score[5] += Math.Abs(small - 24);
                score[6] += Math.Abs(small - 33);
                score[7] += Math.Abs(small - 47);
            }

            if (big_num > 0)
            {
                big = big / big_num;
                score[0] += Math.Abs(big - 8);
                score[1] += Math.Abs(big - 17);
                score[2] += Math.Abs(big - 29);
                score[3] += Math.Abs(big - 42);
                score[4] += Math.Abs(big - 60);
                score[5] += Math.Abs(big - 80);
                score[6] += Math.Abs(big - 106);
                score[7] += Math.Abs(big - 183);
            }

            for (int i = 0; i < score.Length; i++)
            {
                if (score[i] < best_score)
                {
                    return_value[0] = i;
                    best_score = score[i];
                }
            }

            for (int i = 0; i < b.Count - 1; i++)
            {
                if (i <= split_index)
                {
                    int add = Math.Abs(b[i] - avg[0] + ETC1OffTable(return_value[0])[0] + g[i] - avg[1] + ETC1OffTable(return_value[0])[0] + r[i] - avg[2] + ETC1OffTable(return_value[0])[0]);
                    int sub = Math.Abs(b[i] - avg[0] - ETC1OffTable(return_value[0])[0] + g[i] - avg[1] - ETC1OffTable(return_value[0])[0] + r[i] - avg[2] - ETC1OffTable(return_value[0])[0]);
                    if (add > sub)
                    {
                        return_value[1] += sub;
                    }
                    else
                    {
                        return_value[1] += add;
                    }
                }
                else
                {
                    int add = Math.Abs(b[i] - avg[0] + ETC1OffTable(return_value[0])[1] + g[i] - avg[1] + ETC1OffTable(return_value[0])[1] + r[i] - avg[2] + ETC1OffTable(return_value[0])[1]);
                    int sub = Math.Abs(b[i] - avg[0] - ETC1OffTable(return_value[0])[1] + g[i] - avg[1] - ETC1OffTable(return_value[0])[1] + r[i] - avg[2] - ETC1OffTable(return_value[0])[1]);
                    if (add > sub)
                    {
                        return_value[1] += sub;
                    }
                    else
                    {
                        return_value[1] += add;
                    }
                }
            }
            return return_value;
        }

        public static (bool[] sub, bool[] big) ETC1OffAssigner(byte[] colors, int[] avg1, int[] avg2, ETC1Block block)
        {
            int[] off1 = ETC1OffTable(block.offsetset1);
            int[] off2 = ETC1OffTable(block.offsetset2);
            bool[] sub = new bool[16];
            bool[] big = new bool[16];
            for (int o = 0; o < 16; o++)
            {
                if ((block.flip && (o % 4 >= 2)) || (!block.flip && (o >= 8)))
                {
                    int score1 = Math.Abs(colors[o] - (avg2[0] + off2[0])) + Math.Abs(colors[o + 1] - (avg2[1] + off2[0])) + Math.Abs(colors[o + 2] - (avg2[2] + off2[0]));
                    int score2 = Math.Abs(colors[o] - (avg2[0] - off2[0])) + Math.Abs(colors[o + 1] - (avg2[1] - off2[0])) + Math.Abs(colors[o + 2] - (avg2[2] - off2[0]));
                    int score3 = Math.Abs(colors[o] - (avg2[0] + off2[1])) + Math.Abs(colors[o + 1] - (avg2[1] + off2[1])) + Math.Abs(colors[o + 2] - (avg2[2] + off2[1]));
                    int score4 = Math.Abs(colors[o] - (avg2[0] - off2[1])) + Math.Abs(colors[o + 1] - (avg2[1] - off2[1])) + Math.Abs(colors[o + 2] - (avg2[2] - off2[1]));
                    int best = Math.Min(Math.Min(score1, score2), Math.Min(score3, score4));

                    if (best == score1)
                    {
                        sub[o] = false;
                        big[o] = false;
                    }
                    else if (best == score2)
                    {
                        sub[o] = true;
                        big[o] = false;
                    }
                    else if (best == score3)
                    {
                        sub[o] = false;
                        big[o] = true;
                    }
                    else
                    {
                        sub[o] = true;
                        big[o] = true;
                    }
                }
                else
                {
                    int score1 = (colors[o] - (avg1[0] + off1[0])) + (colors[o + 1] - (avg1[1] + off1[0])) + (colors[o + 2] - (avg1[2] + off1[0]));
                    int score2 = (colors[o] - (avg1[0] - off1[0])) + (colors[o + 1] - (avg1[1] - off1[0])) + (colors[o + 2] - (avg1[2] - off1[0]));
                    int score3 = (colors[o] - (avg1[0] + off1[1])) + (colors[o + 1] - (avg1[1] + off1[1])) + (colors[o + 2] - (avg1[2] + off1[1]));
                    int score4 = (colors[o] - (avg1[0] - off1[1])) + (colors[o + 1] - (avg1[1] - off1[1])) + (colors[o + 2] - (avg1[2] - off1[1]));
                    int best = Math.Min(Math.Min(score1, score2), Math.Min(score3, score4));

                    if (best == score1)
                    {
                        sub[o] = false;
                        big[o] = false;
                    }
                    else if (best == score2)
                    {
                        sub[o] = true;
                        big[o] = false;
                    }
                    else if (best == score3)
                    {
                        sub[o] = false;
                        big[o] = true;
                    }
                    else
                    {
                        sub[o] = true;
                        big[o] = true;
                    }
                }
            }

            return (sub, big);
        }
        */
    }
}