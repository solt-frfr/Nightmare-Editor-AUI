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
    
    public class MusicEntry
    {
        public string Track { get; set; }
        public string Description { get; set; }
        public string Filename { get; set; }
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
        public bool Emulator { get; set; }
        public int Region { get; set; }
        public int ETC1Encoder { get; set; }
    }
    
    
    public class PixelWidth : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d && double.Parse(parameter.ToString()) is double width)
                return d * width / 240.0;

            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class PixelMargin : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                var parts = parameter.ToString().Split('.');
                if (parts.Length == 1)
                {
                    return new Thickness(d * double.Parse(parts[0]) / 240.0);
                }
                else if (parts.Length == 2)
                {
                    return new Thickness(d * double.Parse(parts[0]) / 240.0, d * double.Parse(parts[1]) / 240.0);
                }
                else if (parts.Length == 4)
                {
                    return new Thickness(d * double.Parse(parts[0]) / 240.0, d * double.Parse(parts[1]) / 240.0, d * double.Parse(parts[2]) / 240.0, d * double.Parse(parts[3]) / 240.0);
                }
            }

            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
