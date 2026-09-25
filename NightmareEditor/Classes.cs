using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace NightmareEditor
{
    public class TextureList
    {
        public List<string[]> Textures { get; set; }
    }

    public class MusicList
    {
        public List<MusicEntry[]> Music { get; set; }
    }
    
    public class MusicEntry
    {
        public string Track { get; set; }
        public string Description { get; set; }
        public string Filename { get; set; }
        public bool IsInternalFile { get; set; }
    }

    public class Meta
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Authors { get; set; }
        public string Link { get; set; }
        public string ID { get; set; }
        [JsonIgnore]
        public bool IsChecked { get; set; }
        [JsonIgnore]
        public Bitmap LinkImage { get; set; }
        [JsonIgnore]
        public bool ArchiveImage { get; set; }
        public string Prefix { get; set; }
        public string Color { get; set; }
    }
    public class Settings
    {
        public string DeployPath { get; set; }
        public int DefaultImage { get; set; }
        public bool Emulator { get; set; }
        public int Region { get; set; }
        public int ETC1Encoder { get; set; }
        public int UI { get; set; }
        public bool UseMusicReplacements { get; set; }
    }
}
