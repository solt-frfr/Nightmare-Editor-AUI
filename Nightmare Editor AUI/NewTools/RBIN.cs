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
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using OpenKh.Ddd.Utils;

namespace Nightmare_Editor.NewTools;

public class RBIN
{
    public class RBINFile
    {
        public static readonly byte[] data = [0x43, 0x52, 0x41, 0x52, 0x01, 0x00];
        public int ReadEntryCount { get; set; }
        public string MountPoint { get; set; }
        public List<Entry> Entries { get; set; }
        public string Path { get; set; }
    }

    public class Entry
    {
        public byte[] Hash { get; set; }
        [JsonIgnore]
        public byte[] Data { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public int ReadSize { get; set; }
        public bool Compressed { get; set; }
    }

    /// <summary>
    /// Unfinished and unplanned.
    /// </summary>
    public static (string hashString, bool handled) HashPath(string rbin, string filename)
    {
        string output = "";
        if (rbin.StartsWith("_"))
        {
            output = rbin;
        }
        else
        {
            output = Misc.ReplaceFirst(rbin, '_', '/');
        }
        bool handled = true;
        string extention = Path.GetExtension(filename);
        string file = Path.GetFileNameWithoutExtension(filename);
        if (extention == ".pmo" || extention == ".txa" || extention == ".fsm" || extention == ".bcd")
        {
            if (rbin == "chara_d_obj" && filename.StartsWith("d_di") && !filename.StartsWith("d_di0"))
            {
                output = output + "/d_di21/mig/0/bin/" + filename;
            }
            else if (filename == "bin.pmo")
            {
                output = output + "/g_eh80/mig/s/bin/" + filename;
            }
            else if (file.Length < 7)
            {
                output = output + "/" + file[..6] + "/mig/0/bin/" + filename;
            }
            else
            {
                output = output + "/" + file[..6] + "/mig/" + file[6] + "/bin/" + filename;
            }
        }
        else
        {
            handled = false;
        }
        
        return (output, handled);
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
            Directory.CreateDirectory(realoutput);
            Directory.Delete(realoutput, true);
            Directory.CreateDirectory(realoutput);
            RBIN.Load(input, Misc.Paths.basePath, false);
        }
        else
        {
            realoutput = Path.Combine(output, Path.GetFileNameWithoutExtension(input));
            Directory.CreateDirectory(realoutput);
            Directory.Delete(realoutput, true);
            Directory.CreateDirectory(realoutput);
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
            File.WriteAllBytes(Path.Combine(realoutput, i.ToString() + "-" + entry.Name), entry.Data);
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
            string[] files = Directory.GetFiles(realoutput, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if (Containers.IsArc(file))
                {
                    Containers.Generic.Unpack(file);
                }
            }
        }
        return rbin;
    }
    
    public async static void Pack(string filename, bool tooutput, Window parent, string output = null)
    {
        List<int> offsets_values = new List<int>();
        List<int> offsets = new List<int>();
        List<int> name_offsets_values = new List<int>();
        List<int> name_offsets = new List<int>();
        int offset_to_start = 0;
        
        var jsonoptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        string jsonString = File.ReadAllText(Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename), "info.json"));
        List<Entry> list = JsonSerializer.Deserialize<List<Entry>>(jsonString, jsonoptions);
        List<byte> data = new List<byte>();
        
        data.AddRange(RBINFile.data);
        data.Add((byte)(list.Count & 0xFF));
        data.Add((byte)((list.Count >> 8) & 0xFF));
        data.AddRange(new byte[8]);
        char[] chars = Path.GetFileNameWithoutExtension(filename).ToCharArray();

        for (int i = 0; i < 0x10; i++)
        {
            if (i < chars.Length)
            {
                if (chars[i] == '_')
                {
                    chars[i] = '/';
                }
                data.Add(Encoding.ASCII.GetBytes(new[] { chars[i] })[0]);
            }
            else
            {
                data.Add(0);
            }
        }
        
        for (int i = 0; i < list.Count; i++)
        {
            Entry file = list[i];
            string path = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename), i.ToString() + "-" + file.Name);
            data.AddRange(file.Hash);
            name_offsets.Add(data.Count);
            int fileLength = File.ReadAllBytes(path).Length;
            // This is the name offset. Since I obviously don't know where it is right now, I'm filling it with blank bytes.
            data.AddRange(new byte[4]);
            data.Add((byte)((fileLength) & 0xFF));
            data.Add((byte)((fileLength >> 8) & 0xFF));
            data.Add((byte)((fileLength >> 16) & 0xFF));
            int _compress = 0;
            if (file.Compressed)
            {
                // I don't have a compressor. Otherwise, change this to 128.
                // _compress = 128;
            }
            data.Add((byte)(((fileLength >> 24) & 0xFF) + _compress));
            // This is the data offset. I'm filling it with blank bytes as well.
            offsets.Add(data.Count);
            data.AddRange(new byte[4]);
        }
        
        foreach (Entry file in list)
        {
            name_offsets_values.Add(data.Count);
            char[] name_chars = file.Name.ToCharArray();
            for (int i = 0; i < name_chars.Length; i++)
            {
                data.Add(Encoding.ASCII.GetBytes(new[] { name_chars[i] })[0]);
            }
            data.Add(0);

            while (data.Count % 4 != 0)
            {
                data.Add(0);
            }
        }

        while (data.Count % 0x10 != 0)
        {
            data.Add(0);
        }

        for (int i = 0; i < list.Count; i++)
        {
            Entry file = list[i];
            if (offset_to_start == 0)
            {
                offset_to_start = data.Count;
            }
            string path = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename), i.ToString() + "-" + file.Name);
            offsets_values.Add(data.Count);
            data.AddRange(File.ReadAllBytes(path));
        }

        data[0x08] = (byte)((offset_to_start) & 0xFF);
        data[0x09] = (byte)((offset_to_start >> 8) & 0xFF);
        data[0x0A] = (byte)((offset_to_start >> 16) & 0xFF);
        data[0x0B] = (byte)((offset_to_start >> 24) & 0xFF);

        for (int i = 0; i < name_offsets.Count; i++)
        {
            data[name_offsets[i]] = (byte)((name_offsets_values[i] - name_offsets[i]) & 0xFF);
            data[name_offsets[i] + 1] = (byte)(((name_offsets_values[i] - name_offsets[i]) >> 8) & 0xFF);
            data[name_offsets[i] + 2] = (byte)(((name_offsets_values[i] - name_offsets[i]) >> 16) & 0xFF);
            data[name_offsets[i] + 3] = (byte)(((name_offsets_values[i] - name_offsets[i]) >> 24) & 0xFF);
            
        }
        
        for (int i = 0; i < offsets.Count; i++)
        {
            data[offsets[i]] = (byte)(offsets_values[i] & 0xFF);
            data[offsets[i] + 1] = (byte)((offsets_values[i] >> 8) & 0xFF);
            data[offsets[i] + 2] = (byte)((offsets_values[i] >> 16) & 0xFF);
            data[offsets[i] + 3] = (byte)((offsets_values[i] >> 24) & 0xFF);
        }
        
        
        if (!tooutput)
        {
            var save = await parent.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save packed file...",
                FileTypeChoices = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Game Archive File")
                    {
                        Patterns = new List<string> { "*.rbin" }
                    }
                }
            });
            if (!string.IsNullOrWhiteSpace(save.Path.LocalPath))
            {
                File.WriteAllBytes(save.Path.LocalPath, data.ToArray());
            }
        }
        else if (string.IsNullOrWhiteSpace(output))
        {
            string settingspath = Misc.Jsons.settings;
            jsonString = System.IO.File.ReadAllText(settingspath);
            Settings settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
            if (settings.Emulator)
            {
                Directory.CreateDirectory(Path.Combine(settings.DeployPath, "mods", Manager.GetTitleIDFromRegion(settings.Region), "romfs"));
                File.WriteAllBytes(Path.Combine(settings.DeployPath, "mods", Manager.GetTitleIDFromRegion(settings.Region), "romfs", Path.GetFileName(filename)), data.ToArray());
            }
            else
            {
                File.WriteAllBytes(Path.Combine(settings.DeployPath, Path.GetFileName(filename)), data.ToArray());
            }
        }
        else
        {
            File.WriteAllBytes(output, data.ToArray());
        }
    }
}