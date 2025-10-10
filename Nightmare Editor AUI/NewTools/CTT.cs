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
using Color = ExCSS.Color;


namespace Nightmare_Editor.NewTools
{
    /// <summary>
    /// Reimplemented CTT Encoding/Decoding.
    /// Use Decode to turn a CTT file into a PNG.
    /// Use Encode to turn a PNG and CTT file into a new CTT file.
    /// </summary>
    public static class CTT
    {
        public enum Format
        {
            RGBA8888 = 0,
            RGB888 = 1,
            RGBA5551 = 2, 
            RGB565 = 3,
            RGBA4444 = 4,
            LA8 = 5,
            HILO8 = 6,
            L8 = 7,
            A8 = 8,
            LA4 = 9,
            L4 = 10,
            A4 = 11,
            ETC1 = 12,
            ETC1A4 = 13,
        }

        public static int GetFormat(string file)
        {
            byte[] header;
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(0, SeekOrigin.Begin);
                header = new byte[0x80];
                fs.Read(header, 0, 0x80);
            }
            return header[0x1C];
        }

        public static int[] ETC1OffTable(int index)
        {
            if (index == 0)
            {
                return new int[] {2, 8};
            }
            if (index == 1)
            {
                return new int[] {5, 17};
            }
            if (index == 2)
            {
                return new int[] {9, 29};
            }
            if (index == 3)
            {
                return new int[] {13, 42};
            }
            if (index == 4)
            {
                return new int[] {18, 60};
            }
            if (index == 5)
            {
                return new int[] {24, 80};
            }
            if (index == 6)
            {
                return new int[] {33, 106};
            }
            else
            {
                return new int[] {47, 183};
            }
        }

        /// <summary>
        /// Decode a CTT texture into a PNG file. Returns a raw image if needed.
        /// </summary>
        /// <param name="file">Filepath containing a CTT file.</param>
        public static Image Decode(string file, bool output = true)
        {
            byte[] header;
            byte[] data;

            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(0, SeekOrigin.Begin);
                header = new byte[0x80];
                fs.Read(header, 0, 0x80);
                fs.Seek(0x80, SeekOrigin.Begin);
                data = new byte[fs.Length - 0x80];
                fs.Read(data, 0, data.Length);
            }
            int height = header[0x22] + (header[0x23] * 0x100);
            int width = header[0x20] + (header[0x21] * 0x100);
            Format format1 = (Format)header[0x1C];
            string format = format1.ToString();
            var image = Deswizzle(data, width, height, (int)format1);
            if (output)
            {
                image.SaveAsPng(file + "." + format + ".png");
            }
            return image;
        }

        /// <summary>
        /// Encode a PNG file into a CTT texture.
        /// </summary>
        /// <param name="file">Filepath containing a CTT file to replace (must be a real file).</param>
        /// <param name="texture">Filepath containing a PNG texture to Encode into the CTT file.</param>
        public static void Encode(string file, string texture)
        {
            byte[] data = File.ReadAllBytes(texture);
            int formatByte = GetFormat(file);
            Format formatenum = (Format)formatByte;
            var image = Swizzle(data, (int)formatenum);
            File.WriteAllBytes(file, image);
            Decode(file);
        }
        public static Image Deswizzle(byte[] rawData, int width, int height, int format)
        {
            if (format == 0)
            {
                var image = Assemble(rawData, width, height, true);
                return image;
            }
            else if (format == 1)
            {
                var image = Assemble(rawData, width, height, false);
                return image;
            }
            else if (format == 2)
            {
                byte[] newData = RGBA5551unpack(rawData);
                var image = Assemble(newData, width, height, false);
                return image;
            }
            else if (format == 3)
            {
                byte[] newData = RGB565unpack(rawData);
                var image = Assemble(newData, width, height, false);
                return image;
            }
            else if (format == 4)
            {
                byte[] newData = RGBA4444unpack(rawData);
                var image = Assemble(newData, width, height, true);
                return image;
            }
            else if (format == 5)
            {
                byte[] newData = LA8unpack(rawData);
                var image = Assemble(newData, width, height, true);
                return image;
            }
            else if (format == 6)
            {
                byte[] newData = HILO8unpack(rawData);
                var image = Assemble(newData, width, height, false);
                return image;
            }
            else if (format == 7)
            {
                byte[] newData = L8unpack(rawData);
                var image = Assemble(newData, width, height, false);
                return image;
            }
            else if (format == 8)
            {
                byte[] newData = A8unpack(rawData);
                var image = Assemble(newData, width, height, true);
                return image;
            }
            else if (format == 9)
            {
                byte[] newData = LA4unpack(rawData);
                var image = Assemble(newData, width, height, true);
                return image;
            }
            else if (format == 10)
            {
                byte[] newData = L4unpack(rawData);
                var image = Assemble(newData, width, height, false);
                return image;
            }
            else if (format == 11)
            {
                byte[] newData = A4unpack(rawData);
                var image = Assemble(newData, width, height, true);
                return image;
            }
            else if (format == 12)
            {
                byte[] newData = ETC1unpack(rawData);
                var image = Assemble(newData, width, height, false, true);
                return image;
            }
            else if (format == 13)
            {
                byte[] newData = ETC1A4unpack(rawData);
                var image = Assemble(newData, width, height, true, true);
                return image;
            }
            else
            {
                return null;
            }
        }

        public static byte[] Swizzle(byte[] rawData, int format)
        {
            byte[] image = Dissasemble(rawData);
            byte[] newData;
            if (format == 1)
            {
                newData = RGB888pack(image);
            }
            else if (format == 2)
            {
                newData = RGBA5551pack(image);
            }
            else if (format == 3)
            {
                newData = RGB565pack(image);
            }
            else if (format == 4)
            {
                newData = RGBA4444pack(image);
            }
            else if (format == 5)
            {
                newData = LA8pack(image);
            }
            else if (format == 6)
            {
                newData = HILO8pack(image);
            }
            else if (format == 7)
            {
                newData = L8pack(image);
            }
            else if (format == 8)
            {
                newData = A8pack(image);
            }
            else if (format == 9)
            {
                newData = LA4pack(image);
            }
            else if (format == 10)
            {
                newData = L4pack(image);
            }
            else if (format == 11)
            {
                newData = A4pack(image);
            }
            else if (format == 12)
            {
                newData = ETC1pack(image);
            }
            else if (format == 13)
            {
                newData = ETC1A4pack(image);
            }
            else
            {
                newData = image;
            }
            return newData;
        }
        
        public static Image Assemble(byte[] rawData, int width, int height, bool alpha, bool isETC = false)
        {
            const int tileSize = 8;
            const int subtiles = 2;
            int minitiles = 2;
            if (isETC)
            {
                minitiles = 4;
            }
            int miniSize = tileSize / subtiles / minitiles;  // 2
            int bytesPerPixel = 3;
            if (alpha)
            {
                bytesPerPixel = 4;
            }

            int tilesPerRow = (width + tileSize - 1) / tileSize;
            int tilesPerCol = (height + tileSize - 1) / tileSize;

            var image32 = new Image<Rgba32>(width, height);
            var image24 = new Image<Rgb24>(width, height);

            int tileSizeInPixels = tileSize * tileSize;
            int tileSizeInBytes = tileSizeInPixels * bytesPerPixel;

            int tileIndex = 0;

            for (int tileY = 0; tileY < tilesPerCol; tileY++)
            for (int tileX = 0; tileX < tilesPerRow; tileX++)
            {
                int tileBaseOffset = tileIndex * tileSizeInBytes;

                for (int subY = 0; subY < subtiles; subY++)
                for (int subX = 0; subX < subtiles; subX++)
                for (int miniY = 0; miniY < minitiles; miniY++)
                for (int miniX = 0; miniX < minitiles; miniX++)
                for (int py = 0; py < miniSize; py++)
                for (int px = 0; px < miniSize; px++)
                {
                    // Compute relative pixel position in tile
                    int localX = subX * (minitiles * miniSize) + miniX * miniSize + px;
                    int localY = subY * (minitiles * miniSize) + miniY * miniSize + py;

                    int imgX = tileX * tileSize + localX;
                    int imgY = tileY * tileSize + localY;

                    if (imgX >= width || imgY >= height)
                        continue;

                    int pixelIndexInTile =
                        (((subY * subtiles + subX) * minitiles * minitiles) +
                         (miniY * minitiles + miniX)) * (miniSize * miniSize)
                        + (py * miniSize + px);

                    int byteOffset = tileBaseOffset + pixelIndexInTile * bytesPerPixel;

                    byte r = 0x00;
                    byte g = 0x00;
                    byte b = 0x00;
                    byte a = 0x00;
                    try
                    {
                        if (alpha)
                        {
                            r = rawData[byteOffset + 3];
                            g = rawData[byteOffset + 2];
                            b = rawData[byteOffset + 1];
                            a = rawData[byteOffset + 0];
                            image32.DangerousGetPixelRowMemory(imgY).Span[imgX] = new Rgba32(r, g, b, a);
                        }
                        else
                        {
                            r = rawData[byteOffset + 2];
                            g = rawData[byteOffset + 1];
                            b = rawData[byteOffset + 0];
                            image24.DangerousGetPixelRowMemory(imgY).Span[imgX] = new Rgb24(r, g, b);
                        }
                    }
                    catch
                    {
                        if (alpha)
                        {
                            return image32;
                        }
                        else
                        {
                            return image24;
                        }
                    }
                }
                tileIndex++;
            }
            if (alpha)
            {
                return image32;
            }
            else
            {
                return image24;
            }
        }
        
        public static byte[] CTTHeader(int width, int height, int format)
        {
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
            else if (format >= 12 && format <= 13)
            {
                total = (height / 4) * (width / 4) * 8;
            }
            byte[] header = new byte[0x80];

            header[0x00] = 0x43;
            header[0x01] = 0x54;
            header[0x02] = 0x52;
            header[0x03] = 0x54;
            header[0x08] = 0x10;
            header[0x0C] = 0x80;
            header[0x14] = (byte)(total & 0xFF);
            header[0x15] = (byte)((total >> 8) & 0xFF);
            header[0x16] = (byte)((total >> 16) & 0xFF);
            header[0x17] = (byte)((total >> 24) & 0xFF);
            header[0x1C] = (byte)format;
            header[0x20] = (byte)(width & 0xFF);
            header[0x21] = (byte)((width >> 8) & 0xFF);
            header[0x22] = (byte)(height & 0xFF);
            header[0x23] = (byte)((height >> 8) & 0xFF);
            return header;
        }

        public static byte[] Dissasemble(byte[] rawData, bool isETC = false)
        {
            const int tileSize = 8;
            const int subtiles = 2;
            int minitiles = 2;
            if (isETC)
            {
                minitiles = 4;
            }
            int miniSize = tileSize / subtiles / minitiles;  // 2

            var image = Image.Load(rawData);
            var image32 = image.CloneAs<Rgba32>();

            int height = image.Height;
            int width = image.Width;

            int bytesPerPixel = 4;

            int bytes = width * height * bytesPerPixel;

            int tilesPerRow = (width + tileSize - 1) / tileSize;
            int tilesPerCol = (height + tileSize - 1) / tileSize;

            byte[] newData = new byte[bytes + 0x80];
            byte[] header = CTTHeader(width, height, (int)Format.RGBA8888);

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int count = 0x80;
            int tileSizeInPixels = tileSize * tileSize;
            int tileSizeInBytes = tileSizeInPixels * bytesPerPixel;

            int tileIndex = 0;

            for (int tileY = 0; tileY < tilesPerCol; tileY++)
            for (int tileX = 0; tileX < tilesPerRow; tileX++)
            {
                int tileBaseOffset = tileIndex * tileSizeInBytes;

                for (int subY = 0; subY < subtiles; subY++)
                for (int subX = 0; subX < subtiles; subX++)
                for (int miniY = 0; miniY < minitiles; miniY++)
                for (int miniX = 0; miniX < minitiles; miniX++)
                for (int py = 0; py < miniSize; py++)
                for (int px = 0; px < miniSize; px++)
                {
                    // Compute relative pixel position in tile
                    int localX = subX * (minitiles * miniSize) + miniX * miniSize + px;
                    int localY = subY * (minitiles * miniSize) + miniY * miniSize + py;

                    int imgX = tileX * tileSize + localX;
                    int imgY = tileY * tileSize + localY;

                    if (imgX >= width || imgY >= height)
                        continue;

                    int pixelIndexInTile =
                        (((subY * subtiles + subX) * minitiles * minitiles) +
                         (miniY * minitiles + miniX)) * (miniSize * miniSize)
                        + (py * miniSize + px);

                    int byteOffset = tileBaseOffset + pixelIndexInTile * bytesPerPixel;

                    Rgba32 rgba = new Rgba32();
                    rgba = image32.DangerousGetPixelRowMemory(imgY).Span[imgX];
                    newData[count + 3] = rgba.R;
                    newData[count + 2] = rgba.G;
                    newData[count + 1] = rgba.B;
                    newData[count + 0] = rgba.A;
                    count += 4;

                }
                tileIndex++;
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA5551 bytes into RGBA8888 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA5551 byte array.</param>
        /// <returns>Returns a byte array containing raw RGBA8888 data.</returns>
        public static byte[] RGBA5551unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 2];

            int j = 0;
            for (int i = 0; i < ogData.Length; i += 2)
            {
                ushort pixel = (ushort)(ogData[i] | (ogData[i + 1] << 8));

                int r5 = (pixel >> 11) & 0x1F;
                int g5 = (pixel >> 6) & 0x1F;
                int b5 = (pixel >> 1) & 0x1F;
                int a1 = pixel & 0x1;

                byte r8 = (byte)((r5 << 3) | (r5 >> 2));
                byte g8 = (byte)((g5 << 3) | (g5 >> 2));
                byte b8 = (byte)((b5 << 3) | (b5 >> 2));
                byte a8 = 0;
                if (a1 == 1)
                {
                    a8 = 0xFF;
                }

                newData[j++] = (byte)a8;
                newData[j++] = (byte)b8;
                newData[j++] = (byte)g8;
                newData[j++] = (byte)r8;
            }
            return newData;
        }

        /// <summary>
        /// Converts RGB565 bytes into RGB888 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGB565 byte array.</param>
        /// <returns>Returns a byte array containing raw RGB888 data.</returns>
        public static byte[] RGB565unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 3 / 2];

            int j = 0;
            for (int i = 0; i < ogData.Length; i += 2)
            {
                ushort pixel = (ushort)(ogData[i] | (ogData[i + 1] << 8));

                int r5 = (pixel >> 11) & 0x1F;
                int g6 = (pixel >> 5) & 0x3F;
                int b5 = pixel & 0x1F;

                byte r8 = (byte)((r5 << 3) | (r5 >> 2));
                byte g8 = (byte)((g6 << 2) | (g6 >> 4));
                byte b8 = (byte)((b5 << 3) | (b5 >> 2));

                newData[j++] = (byte)b8;
                newData[j++] = (byte)g8;
                newData[j++] = (byte)r8;
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA4444 bytes into RGBA8888 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA4444 byte array.</param>
        /// <returns>Returns a byte array containing raw RGBA8888 data.</returns>
        public static byte[] RGBA4444unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 2];

            int j = 0;
            for (int i = 0; i < ogData.Length; i += 2)
            {
                ushort pixel = (ushort)(ogData[i] | (ogData[i + 1] << 8));

                int r4 = (pixel >> 12) & 0xF;
                int g4 = (pixel >> 8) & 0xF;
                int b4 = (pixel >> 4) & 0xF;
                int a4 = pixel & 0xF;

                byte r8 = (byte)(r4 << 4 | r4);
                byte g8 = (byte)(g4 << 4 | g4);
                byte b8 = (byte)(b4 << 4 | b4);
                byte a8 = (byte)(a4 << 4 | a4);

                newData[j++] = (byte)a8;
                newData[j++] = (byte)b8;
                newData[j++] = (byte)g8;
                newData[j++] = (byte)r8;
            }
            return newData;
        }

        /// <summary>
        /// Converts LA8 bytes into RGBA8888 bytes.
        /// </summary>
        /// <param name="ogData">Raw LA8 byte array.</param>
        /// <returns>Returns a byte array containing raw RGBA8888 data.</returns>
        public static byte[] LA8unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 2];

            int j = 0;
            for (int i = 0; i < ogData.Length; i += 2)
            {
                newData[j++] = ogData[i + 1];
                newData[j++] = ogData[i];
                newData[j++] = ogData[i];
                newData[j++] = ogData[i];
            }
            return newData;
        }

        /// <summary>
        /// Converts HILO8 bytes into RGB888 bytes.
        /// </summary>
        /// <param name="ogData">Raw HILO8 byte array.</param>
        /// <returns>Returns a byte array containing raw RGB888 data.
        /// The red channel represents the Hi channel.
        /// The blue channel represents the Lo channel.</returns>
        public static byte[] HILO8unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 3 / 2];

            int j = 0;
            for (int i = 0; i < ogData.Length; i += 2)
            {
                newData[j++] = ogData[i + 1];
                j++;
                newData[j++] = ogData[i];
            }
            return newData;
        }

        /// <summary>
        /// Converts L8 bytes into RGB888 bytes.
        /// </summary>
        /// <param name="ogData">Raw L8 byte array.</param>
        /// <returns>Returns a byte array containing raw RGB888 data.</returns>
        public static byte[] L8unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 3];

            int j = 0;
            for (int i = 0; i < ogData.Length; i++)
            {
                newData[j++] = ogData[i];
                newData[j++] = ogData[i];
                newData[j++] = ogData[i];
            }
            return newData;
        }

        /// <summary>
        /// Converts A8 bytes into RGBA8888 bytes.
        /// </summary>
        /// <param name="ogData">Raw A8 byte array.</param>
        /// <returns>Returns a byte array containing raw RGBA8888 data.</returns>
        public static byte[] A8unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 4];

            int j = 0;
            for (int i = 0; i < ogData.Length; i++)
            {
                newData[j++] = ogData[i];
                newData[j++] = 0x00;
                newData[j++] = 0x00;
                newData[j++] = 0x00;
            }
            return newData;
        }

        /// <summary>
        /// Converts LA4 bytes into RGBA8888 bytes.
        /// </summary>
        /// <param name="ogData">Raw LA4 byte array.</param>
        /// <returns>Returns a byte array containing raw RGBA8888 data.</returns>
        public static byte[] LA4unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 4];

            int j = 0;
            for (int i = 0; i < ogData.Length; i++)
            {
                int a4 = (ogData[i] >> 4) & 0xF;
                int gray4 = ogData[i] & 0xF;

                byte a8 = (byte)(a4 << 4 | a4);
                byte gray8 = (byte)(gray4 << 4 | gray4);

                newData[j++] = a8;
                newData[j++] = gray8;
                newData[j++] = gray8;
                newData[j++] = gray8;
            }
            return newData;
        }

        /// <summary>
        /// Converts L4 bytes into RGB888 bytes.
        /// </summary>
        /// <param name="ogData">Raw L4 byte array.</param>
        /// <returns>Returns a byte array containing raw RGB888 data.</returns>
        public static byte[] L4unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 6];

            int j = 0;
            for (int i = 0; i < ogData.Length; i++)
            {
                int gray4_1 = (ogData[i] >> 4) & 0xF;
                int gray4_2 = ogData[i] & 0xF;

                byte gray8_1 = (byte)(gray4_1 << 4 | gray4_1);
                byte gray8_2 = (byte)(gray4_2 << 4 | gray4_2);

                newData[j++] = gray8_1;
                newData[j++] = gray8_1;
                newData[j++] = gray8_1;
                newData[j++] = gray8_2;
                newData[j++] = gray8_2;
                newData[j++] = gray8_2;
            }
            return newData;
        }

        /// <summary>
        /// Converts A4 bytes into RGBA8888 bytes.
        /// </summary>
        /// <param name="ogData">Raw A4 byte array.</param>
        /// <returns>Returns a byte array containing raw RGBA8888 data.</returns>
        public static byte[] A4unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 8];

            int j = 0;
            for (int i = 0; i < ogData.Length; i++)
            {
                int a4_1 = (ogData[i] >> 4) & 0xF;
                int a4_2 = ogData[i] & 0xF;

                byte a8_1 = (byte)(a4_1 << 4 | a4_1);
                byte a8_2 = (byte)(a4_2 << 4 | a4_2);

                newData[j++] = a8_1;
                j += 3;
                newData[j++] = a8_2;
                j += 3;
            }
            return newData;
        }

        /// <summary>
        /// Converts ETC1A4 bytes into RGBA8888 bytes.
        /// </summary>
        /// <param name="ogData">Raw ETC1A4 byte array.</param>
        /// <returns>Returns a byte array containing raw RGBA8888 data.</returns>
        public static byte[] ETC1unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 6];
            int j = 0;
            int l = 0;
            for (int i = 0; i < ogData.Length; i+=8)
            {
                byte[] colorBlock = new byte[16 * 3];
                bool flip = (ogData[i + 4] & 0x1) == 1;
                bool diff = ((ogData[i + 4] >> 1) & 0x1) == 1;
                int r1 = 0;
                int r2 = 0;
                int g1 = 0;
                int g2 = 0;
                int b1 = 0;
                int b2 = 0;
                int[] off1 = ETC1OffTable((ogData[i + 4] >> 5) & 0x7);
                int[] off2 = ETC1OffTable((ogData[i + 4] >> 2) & 0x7);
                bool[] big = new bool[16];
                bool[] sub = new bool[16];
                for (int o = 0; o < 16; o++)
                {
                    if (o < 8)
                    {
                        big[o] = ((ogData[i] >> o) & 0x1) == 1;
                        sub[o] = ((ogData[i + 2] >> o) & 0x1) == 1;
                    }
                    else
                    {
                        big[o] = ((ogData[i + 1] >> (o - 8)) & 0x1) == 1;
                        sub[o] = ((ogData[i + 3] >> (o - 8)) & 0x1) == 1;
                    }
                }

                if (diff)
                {
                    b1 = (ogData[i + 5] >> 3) & 0x1F;
                    g1 = (ogData[i + 6] >> 3) & 0x1F;
                    r1 = (ogData[i + 7] >> 3) & 0x1F;
                    b2 = b1;
                    g2 = g1;
                    r2 = r1;
                    b2 += (ogData[i + 5]) & 0x7;
                    g2 += (ogData[i + 6]) & 0x7;
                    r2 += (ogData[i + 7]) & 0x7;
                    if ((ogData[i + 5] & 0x4) != 0)
                    {
                        b2 -= 8;
                    }
                    if ((ogData[i + 6] & 0x4) != 0)
                    {
                        g2 -= 8;
                    }
                    if ((ogData[i + 7] & 0x4) != 0)
                    {
                        r2 -= 8;
                    }
                    b2 = Math.Clamp(b2, 0, 0x1F);
                    g2 = Math.Clamp(g2, 0, 0x1F);
                    r2 = Math.Clamp(r2, 0, 0x1F);
                }
                else
                {
                    b1 = (ogData[i + 5] >> 4) & 0xF;
                    g1 = (ogData[i + 6] >> 4) & 0xF;
                    r1 = (ogData[i + 7] >> 4) & 0xF;
                    b2 = (ogData[i + 5]) & 0xF;
                    g2 = (ogData[i + 6]) & 0xF;
                    r2 = (ogData[i + 7]) & 0xF;
                }
                for (int o = 0; o < 16; o++)
                {
                    int _big = 0;
                    int _sub = 1;
                    if (big[o])
                    {
                        _big = 1;
                    }
                    if (sub[o])
                    {
                        _sub = -1;
                    }
                    if ((flip && (o % 4 >= 2)) || (!flip && (o >= 8)))
                    {
                        if (diff)
                        {
                            int b = ((b2 << 3) + (b2 >> 2) + (off2[_big] * _sub));
                            int g = ((g2 << 3) + (g2 >> 2) + (off2[_big] * _sub));
                            int r = ((r2 << 3) + (r2 >> 2) + (off2[_big] * _sub));
                            b = Math.Clamp(b, 0, 255);
                            g = Math.Clamp(g, 0, 255);
                            r = Math.Clamp(r, 0, 255);
                            colorBlock[j++] = (byte)b;
                            colorBlock[j++] = (byte)g;
                            colorBlock[j++] = (byte)r;
                        }
                        else
                        {
                            int b = (((b2) << 4) + ((b2)) + (off2[_big] * _sub));
                            int g = (((g2) << 4) + ((g2)) + (off2[_big] * _sub));
                            int r = (((r2) << 4) + ((r2)) + (off2[_big] * _sub)); 
                            b = Math.Clamp(b, 0, 255);
                            g = Math.Clamp(g, 0, 255);
                            r = Math.Clamp(r, 0, 255);
                            colorBlock[j++] = (byte)b;
                            colorBlock[j++] = (byte)g;
                            colorBlock[j++] = (byte)r; 
                        }
                    }
                    else
                    {
                        if (diff)
                        {
                            int b = ((b1 << 3) + (b1 >> 2) + (off1[_big] * _sub));
                            int g = ((g1 << 3) + (g1 >> 2) + (off1[_big] * _sub));
                            int r = ((r1 << 3) + (r1 >> 2) + (off1[_big] * _sub));
                            b = Math.Clamp(b, 0, 255);
                            g = Math.Clamp(g, 0, 255);
                            r = Math.Clamp(r, 0, 255);
                            colorBlock[j++] = (byte)b;
                            colorBlock[j++] = (byte)g;
                            colorBlock[j++] = (byte)r;
                        }
                        else
                        {
                            int b = (((b1) << 4) + ((b1)) + (off1[_big] * _sub));
                            int g = (((g1) << 4) + ((g1)) + (off1[_big] * _sub));
                            int r = (((r1) << 4) + ((r1)) + (off1[_big] * _sub)); 
                            b = Math.Clamp(b, 0, 255);
                            g = Math.Clamp(g, 0, 255);
                            r = Math.Clamp(r, 0, 255);
                            colorBlock[j++] = (byte)b;
                            colorBlock[j++] = (byte)g;
                            colorBlock[j++] = (byte)r;
                        }
                    }
                }
                
                j = 0;
                
                newData[l++] = colorBlock[0 * 3 + 0];
                newData[l++] = colorBlock[0 * 3 + 1];
                newData[l++] = colorBlock[0 * 3 + 2];
                newData[l++] = colorBlock[4 * 3 + 0];
                newData[l++] = colorBlock[4 * 3 + 1];
                newData[l++] = colorBlock[4 * 3 + 2];
                newData[l++] = colorBlock[8 * 3 + 0];
                newData[l++] = colorBlock[8 * 3 + 1];
                newData[l++] = colorBlock[8 * 3 + 2];
                newData[l++] = colorBlock[12 * 3 + 0];
                newData[l++] = colorBlock[12 * 3 + 1];
                newData[l++] = colorBlock[12 * 3 + 2];
                
                newData[l++] = colorBlock[1 * 3 + 0];
                newData[l++] = colorBlock[1 * 3 + 1];
                newData[l++] = colorBlock[1 * 3 + 2];
                newData[l++] = colorBlock[5 * 3 + 0];
                newData[l++] = colorBlock[5 * 3 + 1];
                newData[l++] = colorBlock[5 * 3 + 2];
                newData[l++] = colorBlock[9 * 3 + 0];
                newData[l++] = colorBlock[9 * 3 + 1];
                newData[l++] = colorBlock[9 * 3 + 2];
                newData[l++] = colorBlock[13 * 3 + 0];
                newData[l++] = colorBlock[13 * 3 + 1];
                newData[l++] = colorBlock[13 * 3 + 2];
                
                newData[l++] = colorBlock[2 * 3 + 0];
                newData[l++] = colorBlock[2 * 3 + 1];
                newData[l++] = colorBlock[2 * 3 + 2];
                newData[l++] = colorBlock[6 * 3 + 0];
                newData[l++] = colorBlock[6 * 3 + 1];
                newData[l++] = colorBlock[6 * 3 + 2];
                newData[l++] = colorBlock[10 * 3 + 0];
                newData[l++] = colorBlock[10 * 3 + 1];
                newData[l++] = colorBlock[10 * 3 + 2];
                newData[l++] = colorBlock[14 * 3 + 0];
                newData[l++] = colorBlock[14 * 3 + 1];
                newData[l++] = colorBlock[14 * 3 + 2];
                
                newData[l++] = colorBlock[3 * 3 + 0];
                newData[l++] = colorBlock[3 * 3 + 1];
                newData[l++] = colorBlock[3 * 3 + 2];
                newData[l++] = colorBlock[7 * 3 + 0];
                newData[l++] = colorBlock[7 * 3 + 1];
                newData[l++] = colorBlock[7 * 3 + 2];
                newData[l++] = colorBlock[11 * 3 + 0];
                newData[l++] = colorBlock[11 * 3 + 1];
                newData[l++] = colorBlock[11 * 3 + 2];
                newData[l++] = colorBlock[15 * 3 + 0];
                newData[l++] = colorBlock[15 * 3 + 1];
                newData[l++] = colorBlock[15 * 3 + 2];

            }
            return newData;
        }
        
        
        /// <summary>
        /// Converts ETC1A4 bytes into RGBA8888 bytes.
        /// </summary>
        /// <param name="ogData">Raw ETC1A4 byte array.</param>
        /// <returns>Returns a byte array containing raw RGBA8888 data.</returns>
        public static byte[] ETC1A4unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 4];
            int j = 0;
            int l = 0; 
            int m = 0;
            for (int i = 0; i < ogData.Length; i+=16)
            {
                byte[] alphaBlock = new byte[16];
                for (int o = 0; o < alphaBlock.Length / 2; o++)
                {
                    int a4_1 = (ogData[i + o] >> 4) & 0xF;
                    int a4_2 = ogData[i + o] & 0xF;

                    byte a8_1 = (byte)(a4_1 << 4 | a4_1);
                    byte a8_2 = (byte)(a4_2 << 4 | a4_2);

                    alphaBlock[m++] = a8_2;
                    alphaBlock[m++] = a8_1;
                }
                
                
                
                
                
                byte[] colorBlock = new byte[16 * 3];
                bool flip = (ogData[i + 12] & 0x1) == 1;
                bool diff = ((ogData[i + 12] >> 1) & 0x1) == 1;
                int r1 = 0;
                int r2 = 0;
                int g1 = 0;
                int g2 = 0;
                int b1 = 0;
                int b2 = 0;
                int[] off1 = ETC1OffTable((ogData[i + 12] >> 5) & 0x7);
                int[] off2 = ETC1OffTable((ogData[i + 12] >> 2) & 0x7);
                bool[] big = new bool[16];
                bool[] sub = new bool[16];
                for (int o = 0; o < 16; o++)
                {
                    if (o < 8)
                    {
                        big[o] = ((ogData[i + 8] >> o) & 0x1) == 1;
                        sub[o] = ((ogData[i + 10] >> o) & 0x1) == 1;
                    }
                    else
                    {
                        big[o] = ((ogData[i + 9] >> (o - 8)) & 0x1) == 1;
                        sub[o] = ((ogData[i + 11] >> (o - 8)) & 0x1) == 1;
                    }
                }

                if (diff)
                {
                    b1 = (ogData[i + 13] >> 3) & 0x1F;
                    g1 = (ogData[i + 14] >> 3) & 0x1F;
                    r1 = (ogData[i + 15] >> 3) & 0x1F;
                    b2 = b1;
                    g2 = g1;
                    r2 = r1;
                    b2 += (ogData[i + 13]) & 0x7;
                    g2 += (ogData[i + 14]) & 0x7;
                    r2 += (ogData[i + 15]) & 0x7;
                    if ((ogData[i + 13] & 0x4) != 0)
                    {
                        b2 -= 8;
                    }
                    if ((ogData[i + 14] & 0x4) != 0)
                    {
                        g2 -= 8;
                    }
                    if ((ogData[i + 15] & 0x4) != 0)
                    {
                        r2 -= 8;
                    }
                    b2 = Math.Clamp(b2, 0, 0x1F);
                    g2 = Math.Clamp(g2, 0, 0x1F);
                    r2 = Math.Clamp(r2, 0, 0x1F);
                }
                else
                {
                    b1 = (ogData[i + 13] >> 4) & 0xF;
                    g1 = (ogData[i + 14] >> 4) & 0xF;
                    r1 = (ogData[i + 15] >> 4) & 0xF;
                    b2 = (ogData[i + 13]) & 0xF;
                    g2 = (ogData[i + 14]) & 0xF;
                    r2 = (ogData[i + 15]) & 0xF;
                }
                for (int o = 0; o < 16; o++)
                {
                    int _big = 0;
                    int _sub = 1;
                    if (big[o])
                    {
                        _big = 1;
                    }
                    if (sub[o])
                    {
                        _sub = -1;
                    }
                    if ((flip && (o % 4 >= 2)) || (!flip && (o >= 8)))
                    {
                        if (diff)
                        {
                            int b = ((b2 << 3) + (b2 >> 2) + (off2[_big] * _sub));
                            int g = ((g2 << 3) + (g2 >> 2) + (off2[_big] * _sub));
                            int r = ((r2 << 3) + (r2 >> 2) + (off2[_big] * _sub));
                            b = Math.Clamp(b, 0, 255);
                            g = Math.Clamp(g, 0, 255);
                            r = Math.Clamp(r, 0, 255);
                            colorBlock[l++] = (byte)b;
                            colorBlock[l++] = (byte)g;
                            colorBlock[l++] = (byte)r;
                        }
                        else
                        {
                            int b = (((b2) << 4) + ((b2)) + (off2[_big] * _sub));
                            int g = (((g2) << 4) + ((g2)) + (off2[_big] * _sub));
                            int r = (((r2) << 4) + ((r2)) + (off2[_big] * _sub)); 
                            b = Math.Clamp(b, 0, 255);
                            g = Math.Clamp(g, 0, 255);
                            r = Math.Clamp(r, 0, 255);
                            colorBlock[l++] = (byte)b;
                            colorBlock[l++] = (byte)g;
                            colorBlock[l++] = (byte)r; 
                        }
                    }
                    else
                    {
                        if (diff)
                        {
                            int b = ((b1 << 3) + (b1 >> 2) + (off1[_big] * _sub));
                            int g = ((g1 << 3) + (g1 >> 2) + (off1[_big] * _sub));
                            int r = ((r1 << 3) + (r1 >> 2) + (off1[_big] * _sub));
                            b = Math.Clamp(b, 0, 255);
                            g = Math.Clamp(g, 0, 255);
                            r = Math.Clamp(r, 0, 255);
                            colorBlock[l++] = (byte)b;
                            colorBlock[l++] = (byte)g;
                            colorBlock[l++] = (byte)r;
                        }
                        else
                        {
                            int b = (((b1) << 4) + ((b1)) + (off1[_big] * _sub));
                            int g = (((g1) << 4) + ((g1)) + (off1[_big] * _sub));
                            int r = (((r1) << 4) + ((r1)) + (off1[_big] * _sub)); 
                            b = Math.Clamp(b, 0, 255);
                            g = Math.Clamp(g, 0, 255);
                            r = Math.Clamp(r, 0, 255);
                            colorBlock[l++] = (byte)b;
                            colorBlock[l++] = (byte)g;
                            colorBlock[l++] = (byte)r;
                        }
                    }
                }
                
                l = 0;
                m = 0;

                newData[j++] = alphaBlock[0];
                newData[j++] = colorBlock[0 * 3 + 0];
                newData[j++] = colorBlock[0 * 3 + 1];
                newData[j++] = colorBlock[0 * 3 + 2];
                newData[j++] = alphaBlock[4];
                newData[j++] = colorBlock[4 * 3 + 0];
                newData[j++] = colorBlock[4 * 3 + 1];
                newData[j++] = colorBlock[4 * 3 + 2];
                newData[j++] = alphaBlock[8];
                newData[j++] = colorBlock[8 * 3 + 0];
                newData[j++] = colorBlock[8 * 3 + 1];
                newData[j++] = colorBlock[8 * 3 + 2];
                newData[j++] = alphaBlock[12];
                newData[j++] = colorBlock[12 * 3 + 0];
                newData[j++] = colorBlock[12 * 3 + 1];
                newData[j++] = colorBlock[12 * 3 + 2];
                
                newData[j++] = alphaBlock[1];
                newData[j++] = colorBlock[1 * 3 + 0];
                newData[j++] = colorBlock[1 * 3 + 1];
                newData[j++] = colorBlock[1 * 3 + 2];
                newData[j++] = alphaBlock[5];
                newData[j++] = colorBlock[5 * 3 + 0];
                newData[j++] = colorBlock[5 * 3 + 1];
                newData[j++] = colorBlock[5 * 3 + 2];
                newData[j++] = alphaBlock[9];
                newData[j++] = colorBlock[9 * 3 + 0];
                newData[j++] = colorBlock[9 * 3 + 1];
                newData[j++] = colorBlock[9 * 3 + 2];
                newData[j++] = alphaBlock[13];
                newData[j++] = colorBlock[13 * 3 + 0];
                newData[j++] = colorBlock[13 * 3 + 1];
                newData[j++] = colorBlock[13 * 3 + 2];
                    
                newData[j++] = alphaBlock[2];
                newData[j++] = colorBlock[2 * 3 + 0];
                newData[j++] = colorBlock[2 * 3 + 1];
                newData[j++] = colorBlock[2 * 3 + 2];
                newData[j++] = alphaBlock[6];
                newData[j++] = colorBlock[6 * 3 + 0];
                newData[j++] = colorBlock[6 * 3 + 1];
                newData[j++] = colorBlock[6 * 3 + 2];
                newData[j++] = alphaBlock[10];
                newData[j++] = colorBlock[10 * 3 + 0];
                newData[j++] = colorBlock[10 * 3 + 1];
                newData[j++] = colorBlock[10 * 3 + 2];
                newData[j++] = alphaBlock[14];
                newData[j++] = colorBlock[14 * 3 + 0];
                newData[j++] = colorBlock[14 * 3 + 1];
                newData[j++] = colorBlock[14 * 3 + 2];
                
                newData[j++] = alphaBlock[3];
                newData[j++] = colorBlock[3 * 3 + 0];
                newData[j++] = colorBlock[3 * 3 + 1];
                newData[j++] = colorBlock[3 * 3 + 2];
                newData[j++] = alphaBlock[7];
                newData[j++] = colorBlock[7 * 3 + 0];
                newData[j++] = colorBlock[7 * 3 + 1];
                newData[j++] = colorBlock[7 * 3 + 2];
                newData[j++] = alphaBlock[11];
                newData[j++] = colorBlock[11 * 3 + 0];
                newData[j++] = colorBlock[11 * 3 + 1];
                newData[j++] = colorBlock[11 * 3 + 2];
                newData[j++] = alphaBlock[15];
                newData[j++] = colorBlock[15 * 3 + 0];
                newData[j++] = colorBlock[15 * 3 + 1];
                newData[j++] = colorBlock[15 * 3 + 2];
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into RGB888 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw RGB888 data, with a CTT Header.</returns>
        public static byte[] RGB888pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.RGB888);
            byte[] newData = new byte[((ogData.Length - 0x80) * 3 / 4) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }
            int j = 0x80;
            for (int i = 0x81; i < ogData.Length; i++)
            {
                newData[j++] = ogData[i++];
                newData[j++] = ogData[i++];
                newData[j++] = ogData[i++];
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into RGBA5551 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw RGBA5551 data, with a CTT Header.</returns>
        public static byte[] RGBA5551pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.RGBA5551);
            byte[] newData = new byte[((ogData.Length - 0x80) / 2) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 4)
            {
                int a1 = ogData[i + 0] >> 7;
                int b5 = ogData[i + 1] >> 3;
                int g5 = ogData[i + 2] >> 3;
                int r5 = ogData[i + 3] >> 3;

                ushort bytes = (ushort)((r5 << 11) | (g5 << 6) | (b5 << 1) | a1);

                newData[j++] = (byte)(bytes & 0xFF);
                newData[j++] = (byte)((bytes >> 8) & 0xFF);
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into RGB565 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw RGB565 data, with a CTT Header.</returns>
        public static byte[] RGB565pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.RGB565);
            byte[] newData = new byte[((ogData.Length - 0x80) / 2) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 4)
            {
                int b5 = ogData[i + 1] >> 3;
                int g6 = ogData[i + 2] >> 2;
                int r5 = ogData[i + 3] >> 3;

                ushort bytes = (ushort)((r5 << 11) | (g6 << 5) | b5);

                newData[j++] = (byte)(bytes & 0xFF);
                newData[j++] = (byte)((bytes >> 8) & 0xFF);
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into RGBA4444 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw RGBA4444 data, with a CTT Header.</returns>
        public static byte[] RGBA4444pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.RGBA4444);
            byte[] newData = new byte[((ogData.Length - 0x80) / 2) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 4)
            {
                int a4 = ogData[i + 0] >> 4;
                int b4 = ogData[i + 1] >> 4;
                int g4 = ogData[i + 2] >> 4;
                int r4 = ogData[i + 3] >> 4;

                ushort bytes = (ushort)((r4 << 12) | (g4 << 8) | (b4 << 4) | a4);

                newData[j++] = (byte)(bytes & 0xFF);
                newData[j++] = (byte)((bytes >> 8) & 0xFF);
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into LA8 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw LA8 data, with a CTT Header.</returns>
        public static byte[] LA8pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.LA8);
            byte[] newData = new byte[((ogData.Length - 0x80) / 2) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 4)
            {
                int a8 = ogData[i];
                int r = ogData[i + 3];
                int g = ogData[i + 2];
                int b = ogData[i + 1];
                byte gray8 = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
                ushort bytes = (ushort)((gray8 << 8) | a8);

                newData[j++] = (byte)(bytes & 0xFF);
                newData[j++] = (byte)((bytes >> 8) & 0xFF);
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into HILO8 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw HILO8 data, with a CTT Header.</returns>
        public static byte[] HILO8pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.HILO8);
            byte[] newData = new byte[((ogData.Length - 0x80) / 3 * 2) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 4)
            {
                int lo = ogData[i + 1];
                int hi = ogData[i + 3];
                ushort bytes = (ushort)((hi << 8) | lo);

                newData[j++] = (byte)(bytes & 0xFF);
                newData[j++] = (byte)((bytes >> 8) & 0xFF);
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into L8 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw L8 data, with a CTT Header.</returns>
        public static byte[] L8pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.L8);
            byte[] newData = new byte[((ogData.Length - 0x80) / 4) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 4)
            {
                int r = ogData[i + 3];
                int g = ogData[i + 2];
                int b = ogData[i + 1];
                byte gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
                newData[j++] = gray;
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into A8 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw A8 data, with a CTT Header.</returns>
        public static byte[] A8pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.A8);
            byte[] newData = new byte[((ogData.Length - 0x80) / 4) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 4)
            {
                newData[j++] = ogData[i];
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into LA4 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw LA4 data, with a CTT Header.</returns>
        public static byte[] LA4pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.LA4);
            byte[] newData = new byte[((ogData.Length - 0x80) / 4) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 4)
            {
                int a4 = ogData[i] >> 4;
                int r = ogData[i + 3];
                int g = ogData[i + 2];
                int b = ogData[i + 1];
                int gray4 = (int)(0.299 * r + 0.587 * g + 0.114 * b) >> 4;
                byte pixel = (byte)((a4 << 4) | gray4);

                newData[j++] = pixel;
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into L4 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw L4 data, with a CTT Header.</returns>
        public static byte[] L4pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.L4);
            byte[] newData = new byte[((ogData.Length - 0x80) / 8) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 8)
            {
                int r_1 = ogData[i + 3];
                int g_1 = ogData[i + 2];
                int b_1 = ogData[i + 1];
                int r_2 = ogData[i + 7];
                int g_2 = ogData[i + 6];
                int b_2 = ogData[i + 5];
                byte gray4_1 = (byte)((int)(0.299 * r_1 + 0.587 * g_1 + 0.114 * b_1) >> 4);
                byte gray4_2 = (byte)((int)(0.299 * r_2 + 0.587 * g_2 + 0.114 * b_2) >> 4);
                byte pixels = (byte)((gray4_1 << 4) | gray4_2);

                newData[j++] = pixels;
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into A4 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw A4 data, with a CTT Header.</returns>
        public static byte[] A4pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.A4);
            byte[] newData = new byte[((ogData.Length - 0x80) / 8) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 4)
            {
                int a4_1 = ogData[i] >> 4;
                int a4_2 = ogData[i + 4] >> 4;
                byte pixels = (byte)((a4_1 << 4) | a4_2);

                newData[j++] = pixels;
            }
            return newData;
        }
        
        /// <summary>
        /// Converts RGBA8888 bytes into ETC1 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw ETC1 data, with a CTT Header.</returns>
        public static byte[] ETC1pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.ETC1);
            byte[] newData = new byte[((ogData.Length - 0x80) / 6) + 0x80];

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
                new_b1 = (byte)(left_avg[0] >> 3);
                new_g1 = (byte)(left_avg[1] >> 3);
                new_r1 = (byte)(left_avg[2] >> 3);
                new_b2 = (byte)(right_avg[0] >> 3);
                new_g2 = (byte)(right_avg[1] >> 3);
                new_r2 = (byte)(right_avg[2] >> 3);
                left_score[0] = main.offsetset1;
                right_score[0] = main.offsetset2;
            }
            else
            {
                main.flip = true;
                new_b1 = (byte)(top_avg[0] >> 3);
                new_g1 = (byte)(top_avg[1] >> 3);
                new_r1 = (byte)(top_avg[2] >> 3);
                new_b2 = (byte)(bottom_avg[0] >> 3);
                new_g2 = (byte)(bottom_avg[1] >> 3);
                new_r2 = (byte)(bottom_avg[2] >> 3);
                top_score[0] = main.offsetset1;
                bottom_score[0] = main.offsetset2;
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
                    
                main.blue = (byte)(new_b2 << 5 + new_b1);
                main.green = (byte)(new_g2 << 5 + new_g1);
                main.red = (byte)(new_r2 << 5 + new_r1);
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
                main.blue = (byte)(new_b2 << 4 + new_b1);
                main.green = (byte)(new_g2 << 4 + new_g1);
                main.red = (byte)(new_r2 << 4 + new_r1);
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
            for (int i = 0; i < b.Count - 1; i++)
            {
                int temp_diff = (int)(Math.Abs(b[i] - b[i + 1]) + Math.Abs(g[i] - g[i + 1]) + Math.Abs(r[i] - r[i + 1]));
                if (temp_diff > temp)
                {
                    temp = temp_diff;
                    split_index = i;
                }
            }
            for (int i = 0; i < b.Count; i++)
            {
                if (i <= split_index)
                {
                    small += (Math.Abs(b[i] - avg[0]) + Math.Abs(g[i] - avg[1]) + Math.Abs(r[i] - avg[2])) / (split_index + 1);
                }
                else
                {
                    big += (Math.Abs(b[i] - avg[0]) + Math.Abs(g[i] - avg[1]) + Math.Abs(r[i] - avg[2])) / (b.Count - (split_index + 1));
                }
            }

            score[0] = Math.Abs(small - 2) + Math.Abs(big - 8);
            score[1] = Math.Abs(small - 5) + Math.Abs(big - 17);
            score[2] = Math.Abs(small - 9) + Math.Abs(big - 29);
            score[3] = Math.Abs(small - 13) + Math.Abs(big - 42);
            score[4] = Math.Abs(small - 18) + Math.Abs(big - 60);
            score[5] = Math.Abs(small - 24) + Math.Abs(big - 80);
            score[6] = Math.Abs(small - 33) + Math.Abs(big - 106);
            score[7] = Math.Abs(small - 47) + Math.Abs(big - 183);
            for (int i = 0; i < score.Length; i++)
            {
                if (score[i] < best_score)
                {
                    i = return_value[0];
                    score[i] = best_score;
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
                    int score1 = (colors[o] - (avg2[0] + off2[0])) + (colors[o + 1] - (avg2[1] + off2[0])) + (colors[o + 2] - (avg2[2] + off2[0]));
                    int score2 = (colors[o] - (avg2[0] - off2[0])) + (colors[o + 1] - (avg2[1] - off2[0])) + (colors[o + 2] - (avg2[2] - off2[0]));
                    int score3 = (colors[o] - (avg2[0] + off2[1])) + (colors[o + 1] - (avg2[1] + off2[1])) + (colors[o + 2] - (avg2[2] + off2[1]));
                    int score4 = (colors[o] - (avg2[0] - off2[1])) + (colors[o + 1] - (avg2[1] - off2[1])) + (colors[o + 2] - (avg2[2] - off2[1]));
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
    }
}
