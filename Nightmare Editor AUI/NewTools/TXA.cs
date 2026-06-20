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
using System.Text.Json.Serialization;
using SixLabors.ImageSharp.Processing;
using static Nightmare_Editor.NewTools.TXA;


namespace Nightmare_Editor.NewTools
{
    /// <summary>
    /// TXA Encoding/Decoding.
    /// There's quite a few times where widths and heights are flipped. I'm sorry. I don't feel like fixing it either though.
    /// </summary>
    public static class TXA
    {
        public class TXAFile
        {
            public readonly byte[] data = [0x54, 0x58, 0x41, 0x00, 0x01, 0x00];
            public string Name { get; set; }
            public string Path { get; set; }
            public List<AnimGroup> Groups { get; set; }
            public List<long> Adresses { get; set; }
            public List<Texture> Textures { get; set; }
            [JsonIgnore]
            public List<SixLabors.ImageSharp.Image> DecodedTextures { get; set; }
            public List<DestTexture> DestTextures { get; set; }
        }

        public class Anim
        {
            public string Name { get; set; }
            public List<Frame> Frames { get; set; }
        }

        public class AnimGroup
        {
            public string Name { get; set; }
            public string DestTexture { get; set; }
            public int DestHeight { get; set; }
            public int DestWidth { get; set; }
            public int Default { get; set; }
            public int Format { get; set; }
            public List<Anim> Anims { get; set; }
        }
        public class Frame
        {
            public long Texture { get; set; }
            public int Length { get; set; }
            public int Length2 { get; set; }
        }
        public class DestTexture
        {
            public string Name { get; set; }
            public byte[] Texture { get; set; }
        }
        public class Texture
        {
            public string DestTexture { get; set; }
            public byte[] Data { get; set; }
        }

        /// <summary>
        /// Create a TXAFile class from a TXA file.
        /// </summary>
        public static (TXAFile TXAFile, Misc.ErrorCode ErrorCode, string ErrorValue) Load(string file, string searchDir = "defaultDir/puttingstuffheretomakesurenooneusesthisexactstringofcharacters/hiitsmesolt11/balls")
        {
            TXAFile txa = new TXAFile();
            txa.Groups = new List<AnimGroup>();
            txa.Textures = new List<Texture>();
            txa.Adresses = new List<long>();
            txa.DecodedTextures = new List<SixLabors.ImageSharp.Image>();
            txa.DestTextures = new List<DestTexture>();
            txa.Name = Path.GetFileNameWithoutExtension(file);
            txa.Path = Path.GetDirectoryName(file);
            byte[] data = File.ReadAllBytes(file);
            int groupcount = data[0x6] + (data[0x7] * 0x100);
            int j = 0x10;
            for (int i = 0; i < groupcount; i++)
            {
                AnimGroup group = new AnimGroup();
                group.Anims = new List<Anim>();
                byte[] nameBytes = {data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++]};
                nameBytes = nameBytes.Where(b => b != 0).ToArray();
                group.Name = System.Text.Encoding.ASCII.GetString(nameBytes);
                byte[] textBytes = {data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++] };
                textBytes = textBytes.Where(b => b != 0).ToArray();
                group.DestTexture = System.Text.Encoding.ASCII.GetString(textBytes);
                string search = Path.Combine(Misc.Paths.work, Path.GetFileName(Path.GetDirectoryName(file)));
                if (searchDir != "defaultDir/puttingstuffheretomakesurenooneusesthisexactstringofcharacters/hiitsmesolt11/balls")
                    search = searchDir;
                string[] files2 = Directory.GetFiles(search, $"*{group.DestTexture}.ctt", SearchOption.AllDirectories);
                if (files2.Length == 0 && search == Path.Combine(Misc.Paths.work, Path.GetFileName(Path.GetDirectoryName(file))))
                {
                    search = Misc.Paths.work;
                    files2 = Directory.GetFiles(search, $"*{group.DestTexture}.ctt", SearchOption.AllDirectories);
                }
                if (files2.Length == 0)
                {
                    return (null, Misc.ErrorCode.FailedFileFind, "Could not find Destination Texture: " + group.DestTexture);
                }
                string file2 = files2[0];
                byte[] data2 = File.ReadAllBytes(file2);
                bool found = false;
                foreach (DestTexture dest in txa.DestTextures)
                {
                    if (dest.Name == group.DestTexture)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    txa.DestTextures.Add(new DestTexture() { Name = group.DestTexture, Texture = data2 });
                }
                int format = data2[0x1C];
                group.Format = format;
                int total = 0;

                j += 4;
                group.DestWidth = data[j++] + (data[j++] * 0x100);
                group.DestHeight = data[j++] + (data[j++] * 0x100);
                if (format == 0)
                {
                    total = group.DestHeight * group.DestWidth * 4;
                }
                else if (format == 1)
                {
                    total = group.DestHeight * group.DestWidth * 3;
                }
                else if (format >= 2 && format <= 6)
                {
                    total = group.DestHeight * group.DestWidth * 2;
                }
                else if (format >= 7 && format <= 9)
                {
                    total = group.DestHeight * group.DestWidth * 1;
                }
                else if (format >= 10 && format <= 11)
                {
                    total = group.DestHeight * group.DestWidth / 2;
                }
                else if (format >= 12 && format < 13)
                {
                    total = (group.DestHeight / 4) * (group.DestWidth / 4) * 8;
                }
                else if (format <= 13)
                {
                    total = (group.DestHeight / 4) * (group.DestWidth / 4) * 8 * 2;
                }
                int animcount = data[j++] + (data[j++] * 0x100);
                group.Default = data[j++] + (data[j++] * 0x100);
                int o = data[j++] + (data[j++] * 0x100) + (data[j++] * 0x10000) + (data[j++] * 0x1000000);
                for (int k = 0; k < animcount; k++)
                {
                    Anim anim = new Anim();
                    anim.Frames = new List<Frame>();
                    byte[] nameBytes2 = { data[o++], data[o++], data[o++], data[o++], data[o++], data[o++], data[o++], data[o++], data[o++], data[o++], data[o++], data[o++], data[o++], data[o++], data[o++], data[o++]};
                    nameBytes2 = nameBytes2.Where(b => b != 0).ToArray();
                    anim.Name = System.Text.Encoding.ASCII.GetString(nameBytes2);
                    o += 2;
                    int framecount = data[o++] + (data[o++] * 0x100);
                    int m = data[o++] + (data[o++] * 0x100) + (data[o++] * 0x10000) + (data[o++] * 0x1000000);
                    for (int l = 0; l < framecount; l++)
                    {
                        Frame frame = new Frame();
                        long p = data[m++] + (data[m++] * 0x100) + (data[m++] * 0x10000) + (data[m++] * 0x1000000);
                        uint q = (uint)p;
                        frame.Texture = q;
                        frame.Length = data[m++] + (data[m++] * 0x100);
                        frame.Length2 = data[m++] + (data[m++] * 0x100);
                        m += 4;
                        string zeros = "";
                        byte[] image = new byte[total];
                        if (q != 0 && !txa.Adresses.Contains(q))
                        {
                            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                            {
                                fs.Seek(q, SeekOrigin.Begin);
                                fs.Read(image, 0, image.Length);
                            }
                            txa.Adresses.Add(q);
                            TXA.Texture text = new TXA.Texture
                            {
                                DestTexture = group.DestTexture,
                                Data = image
                            };
                            txa.Textures.Add(text);
                            for (int z = 0; z < 4 - txa.Adresses.Count().ToString().Length; z++)
                            {
                                zeros += "0";
                            }
                            SixLabors.ImageSharp.Image decode = CTT.Deswizzle(image, group.DestWidth, group.DestHeight, format);
                            txa.DecodedTextures.Add(decode);
                        }
                        anim.Frames.Add(frame);
                        string c = "";
                    }
                    group.Anims.Add(anim);
                }
                txa.Groups.Add(group);
            }
            return (txa, Misc.ErrorCode.Success, "");
        }

        private static readonly byte[] paddingx4 = { 0x00, 0x00, 0x00, 0x00 };
        private static readonly byte[] paddingx8 = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        private static readonly byte[] paddingx16 = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        private static readonly byte[] paddingx24 = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public static byte[] Create(TXAFile txa)
        {
            List<long> animOffsets = new List<long>();
            List<long> animOffsetsData = new List<long>();
            List<long> frameOffsets = new List<long>();
            List<long> frameOffsetsData = new List<long>();
            List<long> texOffsets = new List<long>();
            List<long> texOffsetsData = new List<long>();
            List<long> adressSort = new List<long>();
            List<Texture> textureSort = new List<Texture>();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(txa.data, 0, txa.data.Length);
                byte[] groupcount = {(byte)(txa.Groups.Count & 0xFF), (byte)((txa.Groups.Count >> 8) & 0xFF)};
                memoryStream.Write(groupcount, 0, groupcount.Length);
                memoryStream.Write(paddingx8, 0, paddingx8.Length);
                foreach (AnimGroup group in txa.Groups)
                {
                    byte[] name = Encoding.ASCII.GetBytes(group.Name);
                    byte[] namebytes = new byte[paddingx16.Length];
                    for (int i = 0; i < name.Length; i++)
                    {
                        namebytes[i] = name[i];
                    }
                    memoryStream.Write(namebytes, 0, namebytes.Length);
                    byte[] textname = Encoding.ASCII.GetBytes(group.DestTexture);
                    byte[] textnamebytes = new byte[paddingx24.Length];
                    for (int i = 0; i < textname.Length; i++)
                    {
                        textnamebytes[i] = textname[i];
                    }
                    memoryStream.Write(textnamebytes, 0, textnamebytes.Length);
                    memoryStream.Write(paddingx4, 0, paddingx4.Length);
                    byte[] header = new byte[0x80];
                    for (int i = 0; i < txa.DestTextures.Count; i++)
                    {
                        if (txa.DestTextures[i].Name == group.DestTexture)
                        {
                            for (int j = 0; j < 0x80; j++)
                            {
                                header[j] = txa.DestTextures[i].Texture[j];
                            }
                            break;
                        }
                    }
                    byte[] size = { (byte)(group.DestHeight & 0xFF), (byte)((group.DestHeight >> 8) & 0xFF), (byte)(group.DestWidth & 0xFF), (byte)((group.DestWidth >> 8) & 0xFF) };
                    memoryStream.Write(size, 0, size.Length);
                    byte[] animcount = { (byte)(group.Anims.Count & 0xFF), (byte)((group.Anims.Count >> 8) & 0xFF) };
                    memoryStream.Write(animcount, 0, animcount.Length);
                    byte[] def = { (byte)(group.Default & 0xFF), (byte)((group.Default >> 8) & 0xFF) };
                    memoryStream.Write(def, 0, def.Length);
                    animOffsets.Add(memoryStream.Length);
                    memoryStream.Write(paddingx4, 0, paddingx4.Length);
                }
                foreach (AnimGroup group in txa.Groups)
                {
                    animOffsetsData.Add(memoryStream.Length);
                    foreach (Anim anim in group.Anims)
                    {
                        byte[] animname = Encoding.ASCII.GetBytes(anim.Name);
                        byte[] animnamebytes = new byte[paddingx16.Length];
                        for (int i = 0; i < animname.Length; i++)
                        {
                            animnamebytes[i] = animname[i];
                        }
                        memoryStream.Write(animnamebytes, 0, animnamebytes.Length);
                        int inv = 0x10000 - anim.Frames.Count;
                        byte[] framecount = { (byte)(anim.Frames.Count & 0xFF), (byte)((anim.Frames.Count >> 8) & 0xFF) };
                        byte[] invcount = { (byte)(inv & 0xFF), (byte)((inv >> 8) & 0xFF) };

                        memoryStream.Write(invcount, 0, invcount.Length);
                        memoryStream.Write(framecount, 0, framecount.Length);
                        frameOffsets.Add(memoryStream.Length);
                        memoryStream.Write(paddingx4, 0, paddingx4.Length);
                    }
                    foreach (Anim anim in group.Anims)
                    {
                        frameOffsetsData.Add(memoryStream.Length);
                        foreach (Frame frame in anim.Frames)
                        {
                            texOffsets.Add(memoryStream.Length);
                            memoryStream.Write(paddingx4, 0, paddingx4.Length);
                            byte[] length = { (byte)(frame.Length & 0xFF), (byte)((frame.Length >> 8) & 0xFF), (byte)(frame.Length2 & 0xFF), (byte)((frame.Length2 >> 8) & 0xFF) };
                            memoryStream.Write(length, 0, length.Length);
                            memoryStream.Write(paddingx4, 0, paddingx4.Length);
                        }
                    }
                }
                while (!(memoryStream.Length % 0x80 == 0))
                {
                    memoryStream.Write(new byte[] { 0x58 }, 0, 1);
                }
                foreach (DestTexture dest in txa.DestTextures)
                {
                    for (int i = 0; i < txa.Textures.Count; i++)
                    {
                        if (txa.Textures[i].DestTexture == dest.Name)
                        {
                            adressSort.Add(txa.Adresses[i]);
                            textureSort.Add(txa.Textures[i]);
                            texOffsetsData.Add(memoryStream.Length);
                            memoryStream.Write(txa.Textures[i].Data, 0, txa.Textures[i].Data.Length);
                        }
                    }
                }
                for (int i = 0; i < animOffsets.Count; i++)
                {
                    memoryStream.Seek(animOffsets[i], SeekOrigin.Begin);
                    byte[] offsetBytes = [(byte)(animOffsetsData[i] & 0xFF), (byte)((animOffsetsData[i] >> 8) & 0xFF), (byte)((animOffsetsData[i] >> 16) & 0xFF), (byte)((animOffsetsData[i] >> 24) & 0xFF)];
                    memoryStream.Write(offsetBytes, 0, offsetBytes.Length);
                }
                for (int i = 0; i < frameOffsets.Count; i++)
                {
                    memoryStream.Seek(frameOffsets[i], SeekOrigin.Begin);
                    byte[] offsetBytes = [(byte)(frameOffsetsData[i] & 0xFF), (byte)((frameOffsetsData[i] >> 8) & 0xFF), (byte)((frameOffsetsData[i] >> 16) & 0xFF), (byte)((frameOffsetsData[i] >> 24) & 0xFF)];
                    memoryStream.Write(offsetBytes, 0, offsetBytes.Length);
                }
                List<Frame> allFrames = new List<Frame>();
                foreach (AnimGroup group in txa.Groups)
                {
                    foreach (Anim anim in group.Anims)
                    {
                        foreach (Frame frame in anim.Frames)
                        {
                            allFrames.Add(frame);
                        }
                    }
                }
                for (int i = 0; i < texOffsets.Count; i++)
                {
                    int j = adressSort.IndexOf(allFrames[i].Texture);
                    if (j >= 0)
                    {
                        memoryStream.Seek(texOffsets[i], SeekOrigin.Begin);
                        byte[] offsetBytes = [(byte)(texOffsetsData[j] & 0xFF), (byte)((texOffsetsData[j] >> 8) & 0xFF), (byte)((texOffsetsData[j] >> 16) & 0xFF), (byte)((texOffsetsData[j] >> 24) & 0xFF)];
                        memoryStream.Write(offsetBytes, 0, offsetBytes.Length);
                    }
                }
                byte[] result = memoryStream.ToArray();
                return result;
            }
        }
        
        public static Image Atlas(TXAFile txa, int destTextureIndex)
        {
            AnimGroup group = null;
            int y = 0;
            for (int i = 0; i < txa.Groups.Count; i++)
            {
                if (txa.Groups[i].DestTexture == txa.DestTextures[destTextureIndex].Name)
                {
                    group = txa.Groups[i];
                    break;
                }
            }
            if (group == null)
            {
                return null;
            }

            int height = 0;
            
            for (int i = 0; i < txa.Textures.Count; i++)
            {
                if (txa.Textures[i].DestTexture == txa.DestTextures[destTextureIndex].Name)
                {
                    height += group.DestHeight;
                }
            }

            Image<Rgb24> largerCanvas = new Image<Rgb24>(group.DestWidth, height);
            for (int i = 0; i < txa.Textures.Count; i++)
            {
                if (txa.Textures[i].DestTexture == txa.DestTextures[destTextureIndex].Name)
                {
                    largerCanvas.Mutate<Rgb24>(ctx => ctx.DrawImage(
                        CTT.Deswizzle(txa.Textures[i].Data, group.DestWidth, group.DestHeight, group.Format),
                        new Point(0, y), 1f)
                        );
                    y += group.DestHeight;
                }
            }
            return largerCanvas;
        }
        
        public static TXAFile ImportAtlas(TXAFile txa, string atlasfile, int destTextureIndex)
        {
            Image imgsharpimg = Image.Load(atlasfile);
            int groupIndex = -1;
            int y = 0;
            for (int i = 0; i < txa.Groups.Count; i++)
            {
                if (txa.Groups[i].DestTexture == txa.DestTextures[destTextureIndex].Name)
                {
                    groupIndex = i;
                    break;
                }
            }
            if (groupIndex == -1)
            {
                return txa;
            }
            for (int i = 0; i < txa.Textures.Count; i++)
            {
                if (txa.Textures[i].DestTexture != txa.DestTextures[destTextureIndex].Name)
                    continue;
                
                using (var memoryStream = new MemoryStream())
                {
                    byte[] image = System.IO.File.ReadAllBytes(atlasfile);
                    Image imageSlice = imgsharpimg.Clone(ipc => ipc.Crop(new Rectangle(0, y, txa.Groups[groupIndex].DestWidth, txa.Groups[groupIndex].DestHeight)));
                    imageSlice.SaveAsPng(memoryStream);
                    byte[] rawImageSlice = memoryStream.ToArray();
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    
                    y += txa.Groups[groupIndex].DestHeight;
                    
                    byte[] textwheader = CTT.Swizzle(rawImageSlice, txa.DestTextures[destTextureIndex].Texture[0x1C]);

                    txa.Textures[i].Data = CTT.SplitHeader(textwheader).data;;
                    txa.DecodedTextures[i] = SixLabors.ImageSharp.Image.Load(rawImageSlice);
                }
            }
            return txa;
        }
    }
}
