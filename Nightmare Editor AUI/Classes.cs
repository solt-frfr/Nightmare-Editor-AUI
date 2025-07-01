using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Nightmare_Editor
{
    public class TextureList
    {
        public List<string[]> Textures { get; set; }
    }

    public class MusicList
    {
        public List<string[]> Music { get; set; }
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
        public string LinkImage { get; set; }
        [JsonIgnore]
        public bool ArchiveImage { get; set; }
    }
    public class Settings
    {
        public string DeployPath { get; set; }
        public int DefaultImage { get; set; }
    }
}
