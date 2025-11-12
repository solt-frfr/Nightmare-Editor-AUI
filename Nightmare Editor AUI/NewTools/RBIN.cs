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
using System.Text.Json;
using SixLabors.ImageSharp.ColorSpaces;
using System.Text.Json.Serialization;
using OpenKh.Ddd.Utils;

namespace Nightmare_Editor.NewTools;

public class RBIN
{
    public class RBINFile
    {
        public readonly byte[] data = [0x43, 0x52, 0x41, 0x52, 0x01, 0x00];
        public int ReadEntryCount { get; set; }
        public string MountPoint { get; set; }
        public List<Entry> Entries { get; set; }
        public string Path { get; set; }
    }

    public class Entry
    {
        [JsonIgnore]
        public byte[] Hash { get; set; }
        [JsonIgnore]
        public byte[] Data { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public int ReadSize { get; set; }
        public bool Compressed { get; set; }
    }
    
    /// <summary>
    /// Create an RBINFile class from an RBIN file.
    /// </summary>
    public static RBINFile Load(string input, string output = null, bool recursive = true)
    {
        List<Entry> json = new List<Entry>();
        string realoutput;
        if (string.IsNullOrWhiteSpace(output))
        {
            realoutput = Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(input));
            Directory.Delete(realoutput, true);
            Directory.CreateDirectory(realoutput);
            RBIN.Load(input, Misc.Paths.basePath, false);
        }
        else
        {
            realoutput = Path.Combine(output, Path.GetFileNameWithoutExtension(input));
        }
        byte[] data = File.ReadAllBytes(input);
        RBINFile rbin = new RBINFile();
        rbin.Entries = new List<Entry>();
        rbin.Path = Path.GetFileName(input);
        rbin.ReadEntryCount = (data[0x6] + (data[0x7] * 0x100));
        int j = 0x10;
        byte[] mountBytes = {data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++], data[j++]};
        mountBytes = mountBytes.Where(b => b != 0).ToArray();
        rbin.MountPoint = System.Text.Encoding.ASCII.GetString(mountBytes);
        int groupcount = data[0x6] + (data[0x7] * 0x100);
        for (int i = 0; i < rbin.ReadEntryCount; i++)
        {
            Entry entry = new Entry();
            List<byte> nameBytes = new List<byte>();
            byte[] hash = {data[j++], data[j++], data[j++], data[j++]};
            uint offset = (uint)j;
            offset += (uint)(data[j++] + (data[j++] * 0x100) + (data[j++] * 0x10000) + (data[j++] * 0x1000000));
            entry.Hash = hash.ToArray();
            while (data[offset] != 0x00)
            {
                nameBytes.Add(data[offset]);
                offset++;
            }
            byte[] name = nameBytes.ToArray();
            entry.Name = System.Text.Encoding.ASCII.GetString(name);
            entry.ReadSize = data[j++] + (data[j++] * 0x100) + (data[j++] * 0x10000) + (data[j++] * 0x1000000);
            if (entry.ReadSize < 0)
            {
                entry.ReadSize = (entry.ReadSize << 1) >> 1;
                entry.Compressed = true;
            }
            else
            {
                entry.Compressed = false;
            }
            offset = (uint)(data[j++] + (data[j++] * 0x100) + (data[j++] * 0x10000) + (data[j++] * 0x1000000));
            entry.Data = new byte[entry.ReadSize];
            using (FileStream fs = new FileStream(input, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(offset, SeekOrigin.Begin);
                fs.Read(entry.Data, 0, entry.ReadSize);
            }
            rbin.Entries.Add(entry);
            Directory.CreateDirectory(realoutput);
            if (entry.Compressed)
            {
                MemoryStream ms = new MemoryStream();
                ms.Write(entry.Data);
                ms.Seek(0, SeekOrigin.Begin);
                entry.Data = BLZ.Uncompress(ms, entry.Data.Length);
            }
            File.WriteAllBytes(Path.Combine(realoutput, entry.Name), entry.Data);
            json.Add(entry);
        }
        var jsonoptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        string jsonString = JsonSerializer.Serialize<List<Entry>>(json, jsonoptions);
        File.WriteAllText(Path.Combine(realoutput, "info.json"), jsonString);
        if (recursive)
        {
            string[] files = Directory.GetFiles(Path.GetDirectoryName(realoutput), "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if (Path.GetExtension(file) == ".pmo")
                {
                    PMO.ExtractAllTextures(file);
                }
                if (Misc.IsArc(file))
                {
                    // salalala sheeeeesh
                }
            }
        }
        return rbin;
    }
}