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
using Nightmare_Editor.NewTools.L2D;
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
                var result = TXA.Load(openedFile);
                TXAFile txa = result.TXAFile;
                if (result.ErrorCode == Misc.ErrorCode.FailedFileFind)
                {
                    Console.WriteLine(result.ErrorValue);
                    var box2 = MessageBoxManager.GetMessageBoxStandard(
                        $"Error",
                        result.ErrorValue,
                        ButtonEnum.Ok,
                        MsBox.Avalonia.Enums.Icon.Question
                    );
                    await box2.ShowAsPopupAsync(this);
                    Close();
                    return;
                }
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
            Files2.Children.Clear();
            foreach (var child in Files.Children)
            {
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

                            GroupInfoPanel.IsVisible = true;
                            Group_TextureBox.Text = group.DestTexture;
                            Group_WidthBox.Text = group.DestWidth.ToString();
                            Group_HeightBox.Text = group.DestHeight.ToString();
                            Group_AnimCountBox.Text = group.Anims.Count.ToString();
                            break;
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
                    GroupInfoPanel.IsVisible = true;
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
                    GroupInfoPanel.IsVisible = false;
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
                for (int k = 0; k < main.DestTextures.Count; k++)
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
                                                    byte[] source = CTT.SplitHeader(main.DestTextures[i].Texture).data;
                                                    
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
                        var txaresult = TXA.Load(open[0].Path.LocalPath);
                        TXA.TXAFile txa = txaresult.TXAFile;
                        if (txaresult.ErrorCode == Misc.ErrorCode.FailedFileFind)
                        {
                            Console.WriteLine(txaresult.ErrorValue);
                            var box2 = MessageBoxManager.GetMessageBoxStandard(
                                $"Error",
                                txaresult.ErrorValue,
                                ButtonEnum.Ok,
                                MsBox.Avalonia.Enums.Icon.Question
                            );
                            await box2.ShowAsPopupAsync(this);
                        }
                        else
                        {
                            main = txa;
                        }
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
            if (file == null)
                return;
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
            var startFolder = await this.StorageProvider.TryGetFolderFromPathAsync(
                Misc.Paths.work
            );

            var save = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save TXA...",
                FileTypeChoices = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Texture Animation")
                    {
                        Patterns = new List<string> { "*.txa" }
                    }
                },
                SuggestedStartLocation = startFolder,
                SuggestedFileName = File.Text
            });
            if (save == null)
                return;
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
                        byte[] text = CTT.SplitHeader(textwheader).data;
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
                var split = CTT.SplitHeader(dest);
                var attrib = CTT.GetAttributesFromHeader(split.header);
                TXA.Texture texture = new TXA.Texture
                {
                    DestTexture = main.DestTextures[i].Name,
                    Data = split.data
                };
                main.Textures.Add(texture);
                main.DecodedTextures.Add(CTT.Deswizzle(split.data, attrib.width, attrib.height, (int)attrib.format));
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

        private async void MakeAtlas_Click(object sender, RoutedEventArgs e)
        {
            PickText aw = new PickText(main);
            int index = -1;
            aw.OnPicked = (int i) =>
            {
                index = i;
            };
            await aw.ShowDialog(this);
            if (index == -1)
                return;
            var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Atlas...",
                FileTypeChoices = new List<FilePickerFileType>
                {
                    new FilePickerFileType("TXA Texture Atlas")
                    {
                        Patterns = new List<string> { "*.png" }
                    }
                }
            });
            if (file == null)
                return;
            if (!string.IsNullOrWhiteSpace(file.Path.LocalPath))
            {
                TXA.Atlas(main, index).SaveAsPng(file.Path.LocalPath);
            }   
        }


        private async void ImportAtlas_Click(object sender, RoutedEventArgs e)
        {
            PickText aw = new PickText(main);
            int index = -1;
            aw.OnPicked = (int i) =>
            {
                index = i;
            };
            await aw.ShowDialog(this);
            if (index == -1)
                return;
            var file = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Import Atlas...",
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("TXA Texture Atlas")
                    {
                        Patterns = new List<string> { "*.png" }
                    }
                }
            });
            if (file == null || file.Count < 1)
                return;
            if (!string.IsNullOrWhiteSpace(file[0].Path.LocalPath))
            {
                main = TXA.ImportAtlas(main, file[0].Path.LocalPath, index);
            }   
        }

        private async void Group_TextureBox_OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = Files.Children.IndexOf(selectedTextBox);
                var box = MessageBoxManager.GetMessageBoxStandard(
                    $"Change Destination Texture",
                    $"Do you want to change this group's Destination Texture from {main.Groups[i].DestTexture} to {Group_TextureBox.Text}?",
                    MsBox.Avalonia.Enums.ButtonEnum.YesNo,
                    MsBox.Avalonia.Enums.Icon.Info
                );
                var result = await box.ShowAsPopupAsync(this);

                if (result == ButtonResult.Yes)
                {
                    string[] files2 = Directory.GetFiles(Misc.Paths.work, $"*{Group_TextureBox.Text}.ctt", SearchOption.AllDirectories);
                    if (files2.Length == 0)
                    {
                        var box2 = MessageBoxManager.GetMessageBoxStandard(
                            $"Missing texture",
                            $"{Group_TextureBox.Text} could not be found.",
                            MsBox.Avalonia.Enums.ButtonEnum.Ok,
                            MsBox.Avalonia.Enums.Icon.Info
                        );
                        await box2.ShowAsPopupAsync(this);
                        return;
                    }
                    string file2 = files2[0];
                    
                    var attrib = CTT.GetAttributesFromFile(file2);
                    var resizebox = MessageBoxManager.GetMessageBoxStandard(
                        $"Resize Texture",
                        $"Do you want to resize this group's Textures to match? ({main.Groups[i].DestWidth}x{main.Groups[i].DestHeight} to {attrib.width}x{attrib.height}) (It's recommended to do it.)",
                        MsBox.Avalonia.Enums.ButtonEnum.YesNo,
                        MsBox.Avalonia.Enums.Icon.Info
                    );
                    var resizeresult = await resizebox.ShowAsPopupAsync(this);

                    if (resizeresult == ButtonResult.Yes)
                    {
                        int dest_text_index_resize = -1;
                        for (int j = 0; j < main.DestTextures.Count; j++)
                        {
                            if (main.Groups[i].DestTexture == main.DestTextures[j].Name)
                            {
                                dest_text_index_resize = j;
                            }
                        }
                        for (int o = 0; o < main.Textures.Count; o++)
                        {
                            if (main.Textures[o].DestTexture == main.DestTextures[dest_text_index_resize].Name)
                            {
                                main.Textures[o].Data = CTT.SplitHeader(CTT.CropCTT(CTT.AddHeader(main.Textures[o].Data, main.Groups[i].DestWidth, main.Groups[i].DestHeight, (CTT.Format)main.Groups[i].Format), attrib.width, attrib.height)).data;
                            }
                        }
                        main.Groups[i].DestWidth = attrib.width;
                        main.Groups[i].DestHeight = attrib.height;
                        Group_WidthBox.Text = attrib.width.ToString();
                        Group_HeightBox.Text = attrib.height.ToString();
                        InfoWindow.IsVisible = false;
                        main = TXA.ReDecodeTextures(main);
                    }
                    
                    int dest_text_index = -1;
                    for (int j = 0; j < main.DestTextures.Count; j++)
                    {
                        if (main.Groups[i].DestTexture == main.DestTextures[j].Name)
                        {
                            dest_text_index = j;
                        }
                    }
                    if (dest_text_index == -1)
                    {
                        var box2 = MessageBoxManager.GetMessageBoxStandard(
                            $"Index out of bounds",
                            $"Somehow, the original destination texture was not found inside the TXA.",
                            MsBox.Avalonia.Enums.ButtonEnum.Ok,
                            MsBox.Avalonia.Enums.Icon.Info
                        );
                        await box2.ShowAsPopupAsync(this);
                        return;
                    }
                    if (main.Groups[i].DestTexture == main.DestTextures[dest_text_index].Name)
                    {
                        string tex_name = Path.GetFileNameWithoutExtension(file2);
                        for (int o = 0; o < main.Textures.Count; o++)
                        {
                            if (main.Textures[o].DestTexture == main.DestTextures[dest_text_index].Name)
                            {
                                main.Textures[o].DestTexture = tex_name;
                            }
                        }
                        main.DestTextures[dest_text_index] = new DestTexture
                        {
                            Name = tex_name,
                            Texture = System.IO.File.ReadAllBytes(file2)
                        };
                        main.Groups[i].DestTexture = tex_name;
                        if (main.Groups[i].Format != (int)attrib.format)
                        {
                            var convertbox = MessageBoxManager.GetMessageBoxStandard(
                                $"Convert Texture",
                                $"Do you want to convert this group's Textures to match the new format? ({((CTT.Format)main.Groups[i].Format).ToString()} -> {attrib.format.ToString()}) (It's recommended to do it.)",
                                MsBox.Avalonia.Enums.ButtonEnum.YesNo,
                                MsBox.Avalonia.Enums.Icon.Info
                            );
                            var convertresult = await convertbox.ShowAsPopupAsync(this);
                            if (convertresult == ButtonResult.Yes)
                            {
                                int dest_text_index_convert = -1;
                                for (int j = 0; j < main.DestTextures.Count; j++)
                                {
                                    if (main.Groups[i].DestTexture == main.DestTextures[j].Name)
                                    {
                                        dest_text_index_convert = j;
                                    }
                                }
                                for (int o = 0; o < main.Textures.Count; o++)
                                {
                                    if (main.Textures[o].DestTexture == main.DestTextures[dest_text_index_convert].Name)
                                    {
                                        MemoryStream ms = new MemoryStream();
                                        CTT.Deswizzle(main.Textures[o].Data, main.Groups[i].DestWidth,
                                            main.Groups[i].DestHeight, main.Groups[i].Format).SaveAsPng(ms);
                                        byte[] data = ms.ToArray();
                                        main.Textures[o].Data = CTT.SplitHeader(CTT.Swizzle(data, (int)attrib.format)).data;
                                    }
                                }
                                InfoWindow.IsVisible = false;
                                main = TXA.ReDecodeTextures(main);
                            }
                        }
                        main.Groups[i].Format = (int)CTT.GetAttributesFromFile(file2).format;
                        Group_TextureBox.Text = tex_name;
                    }
                    main = TXA.ReDecodeTextures(main);
                    InfoWindow.IsVisible = false;
                }
            }
        }

        private async void Group_WidthBox_OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = Files.Children.IndexOf(selectedTextBox);
                if (int.TryParse(Group_WidthBox.Text, out int new_width))
                {
                    if (new_width == main.Groups[i].DestWidth)
                    {
                        return;
                    }
                    if (new_width <= 0)
                    {
                        var invalidbox = MessageBoxManager.GetMessageBoxStandard(
                            $"Cannot change to given width.",
                            $"The new width must be greater than zero.",
                            MsBox.Avalonia.Enums.ButtonEnum.Ok,
                            MsBox.Avalonia.Enums.Icon.Error
                        );
                        await invalidbox.ShowAsPopupAsync(this);
                        return;
                    }
                    if (new_width % 8 != 0)
                    {
                        var noteightbox = MessageBoxManager.GetMessageBoxStandard(
                            $"Cannot change to given width.",
                            $"The new width is not divisible by 8.",
                            MsBox.Avalonia.Enums.ButtonEnum.Ok,
                            MsBox.Avalonia.Enums.Icon.Error
                        );
                        await noteightbox.ShowAsPopupAsync(this);
                        return;
                    }
                }
                var box = MessageBoxManager.GetMessageBoxStandard(
                    $"Resize Group",
                    $"Do you want to change this group's width from {main.Groups[i].DestWidth} to {new_width}?",
                    MsBox.Avalonia.Enums.ButtonEnum.YesNo,
                    MsBox.Avalonia.Enums.Icon.Info
                );
                var result = await box.ShowAsPopupAsync(this);

                if (result == ButtonResult.Yes)
                {
                    int dest_text_index = -1;
                    for (int j = 0; j < main.DestTextures.Count; j++)
                    {
                        if (main.Groups[i].DestTexture == main.DestTextures[j].Name)
                        {
                            dest_text_index = j;
                        }
                    }
                    for (int o = 0; o < main.Textures.Count; o++)
                    {
                        if (main.Textures[o].DestTexture == main.DestTextures[dest_text_index].Name)
                        {
                            main.Textures[o].Data = CTT.SplitHeader(CTT.CropCTT(CTT.AddHeader(main.Textures[o].Data, main.Groups[i].DestWidth, main.Groups[i].DestHeight, (CTT.Format)main.Groups[i].Format), main.Groups[i].DestWidth, new_width)).data;
                        }
                    }
                    main.Groups[i].DestWidth = new_width;
                    InfoWindow.IsVisible = false;
                }
                main = TXA.ReDecodeTextures(main);
            }
        }

        private async void Group_HeightBox_OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = Files.Children.IndexOf(selectedTextBox);
                if (int.TryParse(Group_HeightBox.Text, out int new_height))
                {
                    if (new_height == main.Groups[i].DestHeight)
                    {
                        return;
                    }
                    if (new_height <= 0)
                    {
                        var invalidbox = MessageBoxManager.GetMessageBoxStandard(
                            $"Cannot change to given height.",
                            $"The new height must be greater than zero.",
                            MsBox.Avalonia.Enums.ButtonEnum.Ok,
                            MsBox.Avalonia.Enums.Icon.Error
                        );
                        await invalidbox.ShowAsPopupAsync(this);
                        return;
                    }
                    if (new_height % 8 != 0)
                    {
                        var noteightbox = MessageBoxManager.GetMessageBoxStandard(
                            $"Cannot change to given height.",
                            $"The new height is not divisible by 8.",
                            MsBox.Avalonia.Enums.ButtonEnum.Ok,
                            MsBox.Avalonia.Enums.Icon.Error
                        );
                        await noteightbox.ShowAsPopupAsync(this);
                        return;
                    }
                }
                var box = MessageBoxManager.GetMessageBoxStandard(
                    $"Resize Group",
                    $"Do you want to change this group's height from {main.Groups[i].DestHeight} to {new_height}?",
                    MsBox.Avalonia.Enums.ButtonEnum.YesNo,
                    MsBox.Avalonia.Enums.Icon.Info
                );
                var result = await box.ShowAsPopupAsync(this);

                if (result == ButtonResult.Yes)
                {
                    int dest_text_index = -1;
                    for (int j = 0; j < main.DestTextures.Count; j++)
                    {
                        if (main.Groups[i].DestTexture == main.DestTextures[j].Name)
                        {
                            dest_text_index = j;
                        }
                    }
                    for (int o = 0; o < main.Textures.Count; o++)
                    {
                        if (main.Textures[o].DestTexture == main.DestTextures[dest_text_index].Name)
                        {
                            main.Textures[o].Data = CTT.SplitHeader(CTT.CropCTT(CTT.AddHeader(main.Textures[o].Data, main.Groups[i].DestWidth, main.Groups[i].DestHeight, (CTT.Format)main.Groups[i].Format), main.Groups[i].DestWidth, new_height)).data;
                        }
                    }
                    main.Groups[i].DestHeight = new_height;
                    InfoWindow.IsVisible = false;
                }
                main = TXA.ReDecodeTextures(main);
            }
        }
    }
}
