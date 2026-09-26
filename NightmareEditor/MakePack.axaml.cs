using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System.Diagnostics;
using NightmareEditor;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using NightmareLibrary;
using Avalonia.Platform.Storage;

namespace NightmareEditor
{
    /// <summary>
    /// Interaction logic for MakePack.xaml
    /// </summary>
    public partial class MakePack : Window
    {
        public Meta ModMeta { get; private set; }

        public MakePack()
        {
            InitializeComponent();
        }

        public MakePack(Meta sender)
        {
            InitializeComponent();
            this.Topmost = true;
            ModMeta = sender;
            try
            {
                if (sender.Name != null)
                {
                    Title = $"Edit {sender.Name}";
                    NameBox.Text = sender.Name;
                    DescBox.Text = sender.Description;
                    AuthorBox.Text = sender.Authors;
                    LinkBox.Text = sender.Link;
                    PrefixBox.Text = sender.Prefix;
                    if (Avalonia.Media.Color.TryParse(sender.Color, out Avalonia.Media.Color color))
                    {
                        ColorPick.Color = color;
                    }

                    if (!string.IsNullOrWhiteSpace(sender.Folder))
                    {
                        FolderGrid.Opacity = 0.5;
                        FolderGrid.IsEnabled = false;
                        FolderBox.Text = sender.Folder;
                    }
                    OpenButton.IsEnabled = !sender.ArchiveImage;
                }
            }
            catch
            {
                Close();
            }
        }
        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Preview",
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Preview Image")
                    {
                        Patterns = new List<string> { "*.*" }
                    }
                },
                AllowMultiple = false
            });
            if (files != null)
            {
                PreviewBox.Text = files[0].Path.LocalPath;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            string folder = Path.Combine(Paths.Folders.mods, FolderBox.Text);

            if (Directory.Exists(folder) && FolderGrid.IsEnabled)
            {
                Confirm.Content = "Folder In Use";
                return;
            }
            ModMeta.Name = NameBox.Text;
            ModMeta.Description = DescBox.Text;
            ModMeta.Authors = AuthorBox.Text;
            ModMeta.Link = LinkBox.Text;
            ModMeta.Folder = FolderBox.Text;
            ModMeta.Prefix = PrefixBox.Text;
            Avalonia.Media.Color color = ColorPick.HsvColor.ToRgb();
            ModMeta.Color = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            if (!string.IsNullOrWhiteSpace(ModMeta.Folder))
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = JsonSerializer.Serialize(ModMeta, jsonoptions);
                string filepath = Path.Combine(folder, "meta.json");
                Directory.CreateDirectory(folder);
                File.WriteAllText(filepath, jsonString);
                File.WriteAllText(Paths.Jsons.temp, jsonString);
                filepath = Path.Combine(folder, "preview.webp");
                if (File.Exists(PreviewBox.Text))
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(PreviewBox.Text))
                    {
                        image.Save(filepath, new WebpEncoder());
                    }
                }
                Close();
            }
        }

        private void FolderChanged(object sender, TextChangedEventArgs e)
        {
            char[] charArray = FolderBox.Text.Trim().ToCharArray();
            string folderText = "";
            for (int i = 0; i < charArray.Length; i++)
            {
                if (!char.IsLetterOrDigit(charArray[i]) || !char.IsPunctuation(charArray[i]))
                    folderText += charArray[i];
            }
            FolderBox.Text = folderText;
        }
    }
}
