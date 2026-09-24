using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Nightmare_Editor.NewTools;
using SixLabors.ImageSharp;
using static Nightmare_Editor.NewTools.TXA;

namespace Nightmare_Editor
{
    /// <summary>
    /// Interaction logic for PickText.xaml
    /// </summary>
    public partial class PickText : Window
    {
        public Action<int> OnPicked;
        public List<DestTexture> textures;
        public PickText(TXA.TXAFile txa)
        {
            InitializeComponent();
            textures = txa.DestTextures;
            foreach (TXA.DestTexture text in textures)
            {
                Drop.Items.Add(text.Name);
            }
        }
        
        public PickText()
        {
            InitializeComponent();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (Drop.SelectedIndex < 0)
            {
                Error.Foreground = new SolidColorBrush(Avalonia.Media.Color.Parse("#FFFFFF"));
            }
            else
            {
                OnPicked?.Invoke(Drop.SelectedIndex);
                Close();
            }
        }

        private Bitmap ConvertToImageSource(SixLabors.ImageSharp.Image image)
        {
            using (var memoryStream = new MemoryStream())
            {
                image.SaveAsPng(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var bitmap = new Bitmap(memoryStream);
                return bitmap;
            }
        }

        private void Drop_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int height = textures[Drop.SelectedIndex].Texture[0x22] + (textures[Drop.SelectedIndex].Texture[0x23] * 0x100);
            int width = textures[Drop.SelectedIndex].Texture[0x20] + (textures[Drop.SelectedIndex].Texture[0x21] * 0x100);
            int format = textures[Drop.SelectedIndex].Texture[0x1C];

            byte[] text = CTT.SplitHeader(textures[Drop.SelectedIndex].Texture).data;

            Texture.Source = ConvertToImageSource(CTT.Deswizzle(text, width, height, format));
        }
    }
}
