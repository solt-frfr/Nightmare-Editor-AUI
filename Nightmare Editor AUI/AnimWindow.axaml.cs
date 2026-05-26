using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Nightmare_Editor.NewTools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using static Nightmare_Editor.NewTools.TXA;

namespace Nightmare_Editor
{
    /// <summary>
    /// Interaction logic for AnimWindow.xaml
    /// There's quite a few times where widths and heights are flipped. I'm sorry. I don't feel like fixing it either though.
    /// </summary>
    public partial class AnimWindow : Window
    {
        private TXA.TXAFile main;

        private bool loaded = false;
        private TextBox selectedTextBox;
        private TextBox selectedTextBox2;
        private TextBox selectedTextBox3;
        private TextBox selectedTextBox4;

        private string openedFile;

        public AnimWindow()
        {
            InitializeComponent();
        }

        public AnimWindow(string file)
        {
            this.Opened += Window_Opened;
            openedFile = file;
            InitializeComponent();
        }

        private async void Window_Opened(object sender, EventArgs e)
        {
            if (openedFile.Contains(Misc.Paths.basePath))
            {
                var box2 = MessageBoxManager.GetMessageBoxStandard(
                    "Holdup!",
                    "Woah there! Let's not edit our base files, those are important.",
                    MsBox.Avalonia.Enums.ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Info
                );
                await box2.ShowAsPopupAsync(this);
            }
            else
            {
                TXA.TXAFile txa = TXA.Load(openedFile);
                main = txa;
            }
            Import();
            InfoWindow.IsVisible = false;
            loaded = true;
        }

        private void AddFile(string filename)
        {
            TextBox newTextBox = new TextBox
            {
                Text = filename,
                IsReadOnly = true,
                Width = 200,
                Height = 20,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#202020")),
                BorderBrush = new SolidColorBrush(Avalonia.Media.Color.Parse("#424242")),
                Foreground = new SolidColorBrush(Avalonia.Media.Color.Parse("#f2f2f2")),
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                Focusable = false,
            };
            newTextBox.PointerReleased += TextBox_Click;
            newTextBox.PointerPressed += TextBox_PreviewMouseLeftButtonDown;
            newTextBox.Classes.Add("NoHover");
            Files.Children.Add(newTextBox);
        }

        private void AddFile2(string filename)
        {
            string zeros = "";
            for (int i = 0; i < 4 - (Files2.Children.Count + 1).ToString().Length; i++)
            {
                zeros += "0";
            }
            TextBox newTextBox = new TextBox
            {
                Name = "file" + zeros + (Files2.Children.Count + 1).ToString(),
                Text = filename,
                IsReadOnly = true,
                Width = 200,
                Height = 20,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#202020")),
                BorderBrush = new SolidColorBrush(Avalonia.Media.Color.Parse("#424242")),
                Foreground = new SolidColorBrush(Avalonia.Media.Color.Parse("#f2f2f2")),
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                Focusable = false,
            };
            newTextBox.PointerReleased += TextBox2_Click;
            newTextBox.PointerPressed += TextBox2_PreviewMouseLeftButtonDown;
            newTextBox.Classes.Add("NoHover");
            Files2.Children.Add(newTextBox);
        }
        private void AddFile3(string filename)
        {
            string zeros = "";
            for (int i = 0; i < 4 - (Files3.Children.Count + 1).ToString().Length; i++)
            {
                zeros += "0";
            }
            TextBox newTextBox = new TextBox
            {
                Name = "file" + zeros + (Files3.Children.Count + 1).ToString().Length,
                Text = filename,
                IsReadOnly = true,
                Width = 200,
                Height = 20,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#202020")),
                BorderBrush = new SolidColorBrush(Avalonia.Media.Color.Parse("#424242")),
                Foreground = new SolidColorBrush(Avalonia.Media.Color.Parse("#f2f2f2")),
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                Focusable = false,
            };
            newTextBox.PointerReleased += TextBox3_Click;
            newTextBox.PointerPressed += TextBox3_PreviewMouseLeftButtonDown;
            newTextBox.Classes.Add("NoHover");
            Files3.Children.Add(newTextBox);
            if (Files3.Children.Count <= 1)
            {
                TextBox3_PreviewMouseLeftButtonDown(newTextBox, null);
                TextBox3_Click(newTextBox, null);
            }
        }

        private void AddFile4()
        {
            string zeros = "";
            for (int i = 0; i < 4 - (Textures.Children.Count + 1).ToString().Length; i++)
            {
                zeros += "0";
            }
            TextBox newTextBox = new TextBox
            {
                Name = "Texture" + zeros + (Textures.Children.Count + 1).ToString().Length,
                Text = $@"Texture {Textures.Children.Count + 1}",
                IsReadOnly = true,
                Width = 200,
                Height = 20,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#202020")),
                BorderBrush = new SolidColorBrush(Avalonia.Media.Color.Parse("#424242")),
                Foreground = new SolidColorBrush(Avalonia.Media.Color.Parse("#f2f2f2")),
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                Focusable = false,
            };
            newTextBox.PointerReleased += TextBox4_Click;
            newTextBox.PointerPressed += TextBox4_PreviewMouseLeftButtonDown;
            newTextBox.Classes.Add("NoHover");
            Textures.Children.Add(newTextBox);
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, PointerPressedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                selectedTextBox = tb;
                tb.Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#F04080"));
            }
            foreach (TextBox textbox in Files.Children)
            {
                if (textbox != selectedTextBox)
                    textbox.Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#202020"));
            }
        }

        private void TextBox_Click(object sender, PointerReleasedEventArgs e)
        {
            TextBox_PreviewMouseLeftButtonDown(sender, null);
            try
            {
                Files2_Scroll.ScrollToHome();
            }
            catch { }
            InfoWindow.IsVisible = false;
            foreach (var child in Files.Children)
            {
                Files2.Children.Clear();
                if (child is TextBox textBox && textBox == selectedTextBox)
                {
                    foreach (AnimGroup group in main.Groups)
                    {
                        if (selectedTextBox.Text == group.Name)
                        {
                            foreach (Anim anim in group.Anims)
                            {
                                AddFile2(anim.Name);
                            }
                        }
                    }
                    break;
                }
            }
            var sorted = Files2.Children
                .OfType<TextBox>()
                .OrderBy(tb => tb.Name)
                .ToList();

            Files2.Children.Clear();

            foreach (var textBox in sorted)
            {
                Files2.Children.Add(textBox);
            }
        }
        private void TextBox2_PreviewMouseLeftButtonDown(object sender, PointerPressedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                selectedTextBox2 = tb;
                tb.Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#F04080"));
            }
            foreach (TextBox textbox in Files2.Children)
            {
                if (textbox != selectedTextBox2)
                    textbox.Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#202020"));
            }
        }

        private void TextBox2_Click(object sender, PointerReleasedEventArgs e)
        {
            TextBox2_PreviewMouseLeftButtonDown(sender, null);
            try
            {
                FileName.Items.Clear();
            }
            catch { }
            FileName.Items.Add($@"Destination Texture");
            for (int i = 0; i < main.Textures.Count; i++)
            {
                FileName.Items.Add($@"Texture {i + 1}");
            }
            try
            {
                Files3_Scroll.ScrollToHome();
            }
            catch { }
            InfoWindow.IsVisible = false;
            foreach (var child in Files2.Children)
            {
                Files3.Children.Clear();
                if (child is TextBox textBox && textBox == selectedTextBox2)
                {
                    foreach (AnimGroup group in main.Groups)
                    {
                        if (selectedTextBox.Text == group.Name)
                        {
                            foreach (Anim anim in group.Anims)
                            {
                                if (selectedTextBox2.Text == anim.Name)
                                {
                                    for (int i = 1; i <= anim.Frames.Count; i++)
                                    {
                                        AddFile3($"Frame {i}");
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    break;
                }
            }
        }

        private void TextBox3_PreviewMouseLeftButtonDown(object sender, PointerPressedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                selectedTextBox3 = tb;
                tb.Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#F04080"));
            }
            foreach (TextBox textbox in Files3.Children)
            {
                if (textbox != selectedTextBox3)
                    textbox.Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#202020"));
            }
        }

        private void TextBox3_Click(object sender, PointerReleasedEventArgs e)
        {
            TextBox3_PreviewMouseLeftButtonDown(sender, null);
            for (int i = 0; i < Files3.Children.Count; i++)
            {
                InfoWindow.IsVisible = false;
                if (Files3.Children[i] is TextBox textBox && textBox == selectedTextBox3)
                {
                    AssignImage(i, 3);
                    break;
                }
            }
        }

        private void TextBox4_PreviewMouseLeftButtonDown(object sender, PointerPressedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                selectedTextBox4 = tb;
                tb.Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#F04080"));
            }
            foreach (TextBox textbox in Textures.Children)
            {
                if (textbox != selectedTextBox4)
                    textbox.Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#202020"));
            }
        }

        private void TextBox4_Click(object sender, PointerReleasedEventArgs e)
        {
            TextBox4_PreviewMouseLeftButtonDown(sender, null);
            for (int i = 0; i < Textures.Children.Count; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    InfoWindow.IsVisible = true;
                });
                if (Textures.Children[i] is TextBox textBox && textBox == selectedTextBox4)
                {
                    AssignImage(i, 2);
                    break;
                }
            }
            Files3.Children.Clear();
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

        private async void AssignImage(int input, int from)
        {
            long texture = 0;
            bool foundimage = false;
            if (from == 2)
            {
                texture = input;
                InfoWindow.IsVisible = true;
                FileInfo.IsVisible = false;
                TexInfo.IsVisible = true;
                AddFrame.IsVisible = false;
                Texture.Source = ConvertToImageSource(main.DecodedTextures[(int)texture]);
                int j = 0;
                for (int k = 0; k < main.Textures.Count; k++)
                {
                    if (main.Textures[(int)texture].DestTexture == main.DestTextures[k].Name)
                    {
                        j = k;
                        break;
                    }
                }
                Tex.Text = main.Textures[(int)texture].DestTexture;
                int height = main.DestTextures[j].Texture[0x22] + (main.DestTextures[j].Texture[0x23] * 0x100);
                int width = main.DestTextures[j].Texture[0x20] + (main.DestTextures[j].Texture[0x21] * 0x100);
                for (int i = 0; i < main.Groups.Count; i++)
                {
                    if (main.Groups[i].DestTexture == main.DestTextures[j].Name)
                    {
                        height = main.Groups[i].DestHeight;
                        width = main.Groups[i].DestWidth;
                    }
                }
                TexSize.Text = $@"{width}x{height}";
                TexFormat.Text = ((CTT.Format)main.DestTextures[j].Texture[0x1C]).ToString();
            }
            else if (from == 3)
            {
                InfoWindow.IsVisible = true;
                FileInfo.IsVisible = true;
                TexInfo.IsVisible = false;
                AddFrame.IsVisible = true;
                foreach (var child in Files3.Children)
                {
                    if (child is TextBox textBox && textBox == selectedTextBox3)
                    {
                        foreach (AnimGroup group in main.Groups)
                        {
                            if (selectedTextBox.Text == group.Name)
                            {
                                Destination.Text = group.DestTexture;
                                foreach (Anim anim in group.Anims)
                                {
                                    if (selectedTextBox2.Text == anim.Name)
                                    {
                                        Length.Text = anim.Frames[input].Length.ToString();
                                        Length2.Text = anim.Frames[input].Length2.ToString();
                                        if (anim.Frames[input].Texture == 0)
                                        {
                                            for (int i = 0; i < main.DestTextures.Count; i++)
                                            {
                                                if (group.DestTexture == main.DestTextures[i].Name)
                                                {
                                                    foundimage = true;
                                                    byte[] source = new byte[main.DestTextures[i].Texture.Length - 0x80];
                                                    for (int j = 0; j < source.Length; j++)
                                                    {
                                                        source[j] = main.DestTextures[i].Texture[j + 0x80];
                                                    }
                                                    Texture.Source = ConvertToImageSource(CTT.Deswizzle(source, group.DestWidth, group.DestHeight, main.DestTextures[i].Texture[0x1C]));
                                                    FileName.SelectedIndex = 0;
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            texture = anim.Frames[input].Texture;
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
                if (!foundimage)
                {
                    for (int i = 0; i < main.Adresses.Count; i++)
                    {
                        if (main.Adresses[i] == texture)
                        {
                            foundimage = true;
                            Texture.Source = ConvertToImageSource(main.DecodedTextures[i]);
                            FileName.SelectedIndex = i + 1;
                            break;
                        }
                    }
                }
                if (!foundimage)
                {
                    var box2 = MessageBoxManager.GetMessageBoxStandard(
                        "Texture Remove Error",
                        "The texture this frame used was removed.\nReverting to destination texture.",
                        MsBox.Avalonia.Enums.ButtonEnum.Ok,
                        MsBox.Avalonia.Enums.Icon.Info
                    );
                    await box2.ShowAsPopupAsync(this);
                    foreach (var child in Files3.Children)
                    {
                        if (child is TextBox textBox && textBox == selectedTextBox3)
                        {
                            foreach (AnimGroup group in main.Groups)
                            {
                                if (selectedTextBox.Text == group.Name)
                                {
                                    foreach (Anim anim in group.Anims)
                                    {
                                        if (selectedTextBox2.Text == anim.Name)
                                        {
                                            anim.Frames[input].Texture = 0;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    }
                    AssignImage(input, from);
                }
            }
        }


        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Help hw = new Help();
            hw.Show();
        }

        private async void Load_Click(object sender, PointerReleasedEventArgs e)
        {
            var open = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select a file to open...",
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Nightmare Animation Project")
                    {
                        Patterns = new List<string> { "*.json" }
                    }
                },
                AllowMultiple = false
            });
            if (open != null)
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = System.IO.File.ReadAllText(open[0].Path.LocalPath);
                main = JsonSerializer.Deserialize<TXAFile>(jsonString, jsonoptions);
                main.DecodedTextures = new List<SixLabors.ImageSharp.Image>();
                foreach (Texture texture in main.Textures)
                {
                    byte[] dest = Array.Empty<byte>();
                    for (int i = 0; i < main.DestTextures.Count; i++)
                    {
                        if (main.DestTextures[i].Name == texture.DestTexture)
                        {
                            dest = main.DestTextures[i].Texture;
                        }
                    }
                    int height = dest[0x22] + (dest[0x23] * 0x100);
                    int width = dest[0x20] + (dest[0x21] * 0x100);
                    SixLabors.ImageSharp.Image decode = CTT.Deswizzle(texture.Data, width, height, dest[0x1C]);
                    main.DecodedTextures.Add(decode);
                }
                Import();
            }
        }

        private async void Import_Click(object sender, PointerReleasedEventArgs e)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                $"Import new TXA",
                "Are you sure you want to quit?\nAny unsaved progress will be lost.",
                ButtonEnum.YesNo,
                MsBox.Avalonia.Enums.Icon.Question
            );
            var result = await box.ShowAsPopupAsync(this);
            if (result == ButtonResult.Yes)
            {
                var open = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select TXA File",
                    FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Texture Animation")
                    {
                        Patterns = new List<string> { "*.txa" }
                    }
                },
                    AllowMultiple = false
                });
                if (open != null)
                {
                    if (open[0].Path.LocalPath.Contains(Misc.Paths.basePath))
                    {
                        var box2 = MessageBoxManager.GetMessageBoxStandard(
                    "Holdup!",
                    "Woah there! Let's not edit our base files, those are important.",
                    MsBox.Avalonia.Enums.ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Info
                );
                        await box2.ShowAsPopupAsync(this);
                    }
                    else
                    {
                        TXA.TXAFile txa = TXA.Load(open[0].Path.LocalPath);
                        main = txa;
                    }
                    Import();
                }
            }
        }

        private async void Save_Click(object sender, PointerReleasedEventArgs e)
        {
            var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Project...",
                FileTypeChoices = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Nightmare Animation Project")
                    {
                        Patterns = new List<string> { "*.json" }
                    }
                }
            });
            if (!string.IsNullOrWhiteSpace(file.Path.LocalPath))
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = JsonSerializer.Serialize<TXAFile>(main, jsonoptions);
                System.IO.File.WriteAllText(file.Path.LocalPath, jsonString);
            }   
        }

        private async void Export_Click(object sender, PointerReleasedEventArgs e)
        {
            byte[] file = TXA.Create(main);

            var save = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save TXA...",
                FileTypeChoices = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Texture Animation")
                    {
                        Patterns = new List<string> { "*.txa" }
                    }
                }
            });
            if (!string.IsNullOrWhiteSpace(save.Path.LocalPath))
            {
                System.IO.File.WriteAllBytes(save.Path.LocalPath, file);
            }
        }

        private async void Reverse_Rebirth(object sender, RoutedEventArgs e)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                $"Exit",
                "Are you sure you want to quit?\nAny unsaved progress will be lost.",
                ButtonEnum.YesNo,
                MsBox.Avalonia.Enums.Icon.Question
            );
            var result = await box.ShowAsPopupAsync(this);
            if (result == ButtonResult.Yes)
            {
                Close();
            }
        }

        private async void RemoveFile3(object sender, RoutedEventArgs e)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                $"Remove Frame",
                "Are you sure you want to remove this frame?\nIt may cause problems.",
                ButtonEnum.YesNo,
                MsBox.Avalonia.Enums.Icon.Question
            );
            var result = await box.ShowAsPopupAsync(this);
            if (result == ButtonResult.Yes)
            {
                int i = Files.Children.IndexOf(selectedTextBox);
                int j = Files2.Children.IndexOf(selectedTextBox2);
                int k = Files3.Children.IndexOf(selectedTextBox3);
                if (main.Groups[i].Anims[j].Frames.Count <= 1)
                {
                    var box2 = MessageBoxManager.GetMessageBoxStandard(
                        "Holdup!",
                        "You can't remove the only frame.",
                        MsBox.Avalonia.Enums.ButtonEnum.Ok,
                        MsBox.Avalonia.Enums.Icon.Info
                    );
                    await box2.ShowAsPopupAsync(this);
                }
                else
                {
                    main.Groups[i].Anims[j].Frames.Remove(main.Groups[i].Anims[j].Frames[k]);
                    Files3.Children.Clear();
                    for (int l = 0; l < main.Groups[i].Anims[j].Frames.Count; l++)
                    {
                        AddFile3($"Frame {l + 1}");
                    }
                    var box2 = MessageBoxManager.GetMessageBoxStandard(
                        "Done",
                        "Removed.",
                        MsBox.Avalonia.Enums.ButtonEnum.Ok,
                        MsBox.Avalonia.Enums.Icon.Info
                    );
                    await box2.ShowAsPopupAsync(this);
                }
            }
        }

        private async void RemoveTex(object sender, RoutedEventArgs e)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                $"Remove Texture",
                "Are you sure you want to remove this texture?\nAll frames using this texture will revert to their Destination Texture.",
                ButtonEnum.YesNo,
                MsBox.Avalonia.Enums.Icon.Question
            );
            var result = await box.ShowAsPopupAsync(this);
            if (result == ButtonResult.Yes)
            {
                int i = Textures.Children.IndexOf(selectedTextBox4);
                long texture = main.Adresses[i];
                main.Adresses.Remove(main.Adresses[i]);
                main.Textures.Remove(main.Textures[i]);
                main.DecodedTextures.Remove(main.DecodedTextures[i]);
                Textures.Children.Clear();
                foreach (long adress in main.Adresses)
                {
                    AddFile4();
                }
                foreach (AnimGroup group in main.Groups)
                {
                    foreach (Anim anim in group.Anims)
                    {
                        for (int j = 0; j < anim.Frames.Count; j++)
                        {
                            if (anim.Frames[j].Texture == texture)
                            {
                                anim.Frames[j].Texture = 0;
                            }
                        }
                    }
                }
                var box2 = MessageBoxManager.GetMessageBoxStandard(
                    "Done",
                    "Removed.",
                    MsBox.Avalonia.Enums.ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Info
                );
                await box2.ShowAsPopupAsync(this);
            }
        }

        private async void Link_Click(object sender, RoutedEventArgs e)
        {
            int i = Textures.Children.IndexOf(selectedTextBox4);
            int j = 0;
            for (int k = 0; k < main.Textures.Count; k++)
            {
                if (main.Textures[i].DestTexture == main.DestTextures[k].Name)
                {
                    j = k;
                    break;
                }
            }
            byte[] dest = main.DestTextures[j].Texture;
            var box = MessageBoxManager.GetMessageBoxStandard(
                "New Texture Warning",
                "Please select a texture with the same width and height as the original.\nAn image with a different width and height may not be encoded correctly.",
                MsBox.Avalonia.Enums.ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Info
            );
            await box.ShowAsPopupAsync(this);
            try
            {
                var open = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select New Texture",
                    FileTypeFilter = new List<FilePickerFileType>
                    {
                        new FilePickerFileType("Texture")
                        {
                            Patterns = new List<string> { "*.*" }
                        }
                    },
                    AllowMultiple = false
                });
                if (open != null)
                {
                    if (System.IO.File.Exists(open[0].Path.LocalPath))
                    {
                        byte[] image = System.IO.File.ReadAllBytes(open[0].Path.LocalPath);
                        byte[] textwheader = CTT.Swizzle(image, main.DestTextures[j].Texture[0x1C]);
                        byte[] text = new byte[textwheader.Length - 0x80];
                        for (int k = 0; k < text.Length; k++)
                        {
                            text[k] = textwheader[k + 0x80];
                        }
                        main.Textures[i].Data = text;
                        main.DecodedTextures[i] = SixLabors.ImageSharp.Image.Load(image);
                    }
                    AssignImage(i, 2);
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async void SaveTex_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save extracted texture...",
                    FileTypeChoices = new List<FilePickerFileType>
                    {
                        new FilePickerFileType("Texture File")
                        {
                            Patterns = new List<string> { "*.png" }
                        }
                    }
                });

                if (!string.IsNullOrWhiteSpace(file.Path.LocalPath))
                {
                    bool foundimage = false;
                    int i = Textures.Children.IndexOf(selectedTextBox4);
                    InfoWindow.IsVisible = true;
                    FileInfo.IsVisible = false;
                    TexInfo.IsVisible = true;
                    main.DecodedTextures[i].SaveAsPng(file.Path.LocalPath);
                    int j = 0;
                }
            }
            catch (Exception exception)
            {
            }
        }

        private void MNN_Click(object sender, PointerReleasedEventArgs e)
        {
            RenderOptions.SetBitmapInterpolationMode(Texture, BitmapInterpolationMode.None);
            Texture.InvalidateVisual();
        }

        private void MLS_Click(object sender, PointerReleasedEventArgs e)
        {
            RenderOptions.SetBitmapInterpolationMode(Texture, BitmapInterpolationMode.HighQuality);
            Texture.InvalidateVisual();
        }

        private void Import()
        {
            try
            {
                Files.Children.Clear();
                Files2.Children.Clear();
                Files3.Children.Clear();
                Textures.Children.Clear();
            }
            catch { }
            File.Text = main.Name;
            foreach (TXA.AnimGroup group in main.Groups)
            {
                AddFile(group.Name);
            }
            foreach (long adress in main.Adresses)
            {
                AddFile4();
            }
        }

        private void AddText_Click(object sender, PointerReleasedEventArgs e)
        {
            PickText aw = new PickText(main);
            aw.OnPicked = (int i) =>
            {
                byte[] dest = main.DestTextures[i].Texture;
                byte[] text = new byte[dest.Length - 0x80];
                for (int j = 0; j < text.Length; j++)
                {
                    text[j] = dest[j + 0x80];
                }
                int height = dest[0x22] + (dest[0x23] * 0x100);
                int width = dest[0x20] + (dest[0x21] * 0x100);
                TXA.Texture texture = new TXA.Texture
                {
                    DestTexture = main.DestTextures[i].Name,
                    Data = text
                };
                main.Textures.Add(texture);
                main.DecodedTextures.Add(CTT.Deswizzle(text, width, height, dest[0x1C]));
                AddFile4();
                for (int j = 1; j < 0x3FFFFFFF; j++)
                {
                    if (!main.Adresses.Contains(j))
                    {
                        main.Adresses.Add(j);
                        break;
                    }
                }
                if (Textures.Children[Textures.Children.Count - 1] is TextBox tb)
                {
                    selectedTextBox4 = tb;
                    tb.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#F04080");
                    foreach (TextBox textbox in Textures.Children)
                    {
                        if (textbox != selectedTextBox4)
                            textbox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#202020");
                    }
                    TextBox4_Click(null, null);
                }
            };
            aw.ShowDialog(this);
        }

        private void BadApple(object sender, PointerReleasedEventArgs e)
        {
            PickText aw = new PickText(main);
            aw.OnPicked = (int i) =>
            {
                byte[] dest = main.DestTextures[i].Texture;
                byte[] text = new byte[dest.Length - 0x80];
                for (int j = 0; j < text.Length; j++)
                {
                    text[j] = dest[j + 0x80];
                }
                int height = dest[0x22] + (dest[0x23] * 0x100);
                int width = dest[0x20] + (dest[0x21] * 0x100);
                TXA.Texture texture = new TXA.Texture
                {
                    DestTexture = main.DestTextures[i].Name,
                    Data = text
                };
                string[] files2 = Directory.GetFiles($@"/home/solt/Downloads/frames/downscaled/", $"*.png", SearchOption.AllDirectories);
                for (int k = 0; k < files2.Length; k++)
                {
                    if (System.IO.File.Exists(files2[k]))
                    {
                        byte[] image = System.IO.File.ReadAllBytes(files2[k]);
                        byte[] textwheader = CTT.Swizzle(image, dest[0x1C]);
                        byte[] text2 = new byte[textwheader.Length - 0x80];
                        for (int l = 0; l < text2.Length; l++)
                        {
                            text2[l] = textwheader[l + 0x80];
                        }
                        main.Textures.Add(new Texture { Data = text2, DestTexture = main.DestTextures[i].Name });
                        main.DecodedTextures.Add(SixLabors.ImageSharp.Image.Load(image));
                        AddFile4();
                        for (int j = 1; j < 0x3FFFFFFF; j++)
                        {
                            if (!main.Adresses.Contains(j))
                            {
                                main.Adresses.Add(j);
                                break;
                            }
                        }
                    }
                }
                if (Textures.Children[Textures.Children.Count - 1] is TextBox tb)
                {
                    selectedTextBox4 = tb;
                    tb.Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#F04080"));
                    foreach (TextBox textbox in Textures.Children)
                    {
                        if (textbox != selectedTextBox4)
                            textbox.Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#202020"));
                    }
                    TextBox4_Click(null, null);
                }
            };
            aw.ShowDialog(this);
        }

        private void AddFrame_Click(object sender, PointerReleasedEventArgs e)
        {
            int i = Files.Children.IndexOf(selectedTextBox);
            int j = Files2.Children.IndexOf(selectedTextBox2);
            TXA.Frame frame = new TXA.Frame
            {
                Length = 0,
                Length2 = 0,
                Texture = 0
            };
            main.Groups[i].Anims[j].Frames.Add(frame);
            Files3.Children.Clear();
            for (int k = 0; k < main.Groups[i].Anims[j].Frames.Count; k++)
            {
                AddFile3($"Frame {k + 1}");
            }
        }

        private void FileName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int j = 0;
            if (FileName.SelectedIndex < 0)
            {
                return;
            }
            foreach (var child in Files3.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox3)
                {
                    foreach (AnimGroup group in main.Groups)
                    {
                        if (selectedTextBox.Text == group.Name)
                        {
                            foreach (Anim anim in group.Anims)
                            {
                                if (selectedTextBox2.Text == anim.Name)
                                {
                                    for (int i = 0; i < Files3.Children.Count; i++)
                                    {
                                        if (i == Files3.Children.IndexOf(selectedTextBox3))
                                        {
                                            if (FileName.SelectedIndex == 0)
                                            {
                                                anim.Frames[i].Texture = 0;
                                            }
                                            else
                                            {
                                                anim.Frames[i].Texture = main.Adresses[FileName.SelectedIndex - 1];
                                            }
                                            j = i;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
            }
            AssignImage(j, 3);
        }

        private void Length_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!loaded)
            {
                return;
            }
            try
            {
                if (int.Parse(Length.Text) >= 65535)
                {
                    Length.Text = 65535.ToString();
                }
            }
            catch { }
            int i = Files.Children.IndexOf(selectedTextBox);
            int j = Files2.Children.IndexOf(selectedTextBox2);
            int k = Files3.Children.IndexOf(selectedTextBox3);
            try
            {
                main.Groups[i].Anims[j].Frames[k].Length = int.Parse(Length.Text);
            }
            catch { }
        }

        private void Length2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!loaded)
            {
                return;
            }
            try
            {
                if (int.Parse(Length2.Text) >= 65535)
                {
                    Length2.Text = 65535.ToString();
                }
            }
            catch { }
            int i = Files.Children.IndexOf(selectedTextBox);
            int j = Files2.Children.IndexOf(selectedTextBox2);
            int k = Files3.Children.IndexOf(selectedTextBox3);
            try
            {
                main.Groups[i].Anims[j].Frames[k].Length2 = int.Parse(Length2.Text);
            }
            catch { }
        }

        private void NumbersOnly(object sender, TextInputEventArgs e)
        {
            if (int.TryParse(e.Text, out int value))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }
    }
}
