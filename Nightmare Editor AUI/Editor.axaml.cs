using System.Text;
using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Nightmare_Editor.NewTools;
using System.Text.Json;
using System.Collections.Generic;
using System.ComponentModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Layout;
using SharpCompress.Archives;
using SharpCompress.Common;
using Avalonia.Media;
using Avalonia.Input;
using System.Linq;
using Avalonia.Media.Imaging;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Avalonia.Platform;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input.Platform;
using Nightmare_Editor_AUI;


namespace Nightmare_Editor
{
    /// <summary>
    /// Interaction logic for Editor.axaml
    /// </summary>
    public partial class Editor : Window
    {
        // "Game Archive files (*.rbin)|*.rbin|Texture files(*.ctt)|*.ctt|Layout 2D files(*.l2d)|*.l2d|Effect Files(*.fep)|*.fep|Model files(*.pmo)|*.pmo|Map files(*.pmp)|*.pmp|All files (*.*)|*.*";
        private TextBox selectedTextBox;
        private TextBox selectedTextBox2;
        private TextBox selectedTextBox3;

        private List<string> flaggedFiles = new List<string>();
        private List<string> flaggedFiles2 = new List<string>();
        private List<string> flaggedFiles3 = new List<string>();

        private bool windowSwap = false;
        private bool windowStore = false;

        private bool textureSwap = false;

        private List<string> allfiles = new List<string>();

        private List<string[]> textureLinks = new List<string[]>();

        private readonly string linkPath = Misc.Jsons.textures;

        private List<TextBox> unfiltered = new List<TextBox>();
        private List<TextBox> filtered = new List<TextBox>();

        public bool rexIsVisible = true;
        public bool replaceIsVisible = true;
        public bool removeIsVisible = true;
        public bool remove2IsVisible = true;
        public bool flag2IsVisible = true;
        public bool flag3IsVisible = true;
        

        public Editor()
        {
            InitializeComponent();
            InfoWindow.IsVisible = false;
            HexWindow.IsVisible = false;
            List<string> Paths = new List<string>();
            Directory.CreateDirectory(Misc.Paths.current);
            Directory.CreateDirectory(Misc.Paths.work);
            Directory.CreateDirectory(Path.Combine(Misc.Paths.work, "User-Added"));
            Directory.CreateDirectory(Misc.Paths.basePath);
            Directory.CreateDirectory(Path.Combine(Misc.Paths.basePath, "User-Added"));
            string[] files = Directory.GetFiles(Misc.Paths.current, "*.*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string filetrim = file.Replace(Misc.Paths.current + Path.DirectorySeparatorChar, "");
                AddFile(filetrim);
            }
            if (!File.Exists(linkPath))
            {
                QuickJson(true);
            }
            QuickJson(false);
        }

        private void QuickJson(bool write)
        {
            if (write)
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                TextureList texturelist = new TextureList();
                texturelist.Textures = textureLinks;
                string jsonString = JsonSerializer.Serialize<TextureList>(texturelist, jsonoptions);
                File.WriteAllText(linkPath, jsonString);
            }
            else
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = File.ReadAllText(linkPath);
                textureLinks = JsonSerializer.Deserialize<TextureList>(jsonString, jsonoptions).Textures;
            }
        }

        public static void BetterDirCopy(string sourceDir, string destDir, bool delete, bool ow = true)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                try
                {
                    File.Copy(file, destFile, overwrite: ow);
                }
                catch (Exception e)
                {
                    if (ow || !File.Exists(destFile))
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
                BetterDirCopy(dir, destSubDir, false, ow);
            }
            if (delete)
            {
                Directory.Delete(sourceDir, true);
            }
        }

        private async void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(Misc.Paths.current);
                var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select Game File",
                    AllowMultiple = false,
                    FileTypeFilter = Misc.FileFilters.all,
                });
                Path.Combine(Misc.Paths.basePath, "User-Added");
                Directory.CreateDirectory(Path.Combine(Misc.Paths.basePath, "User-Added"));
                Directory.CreateDirectory(Path.Combine(Misc.Paths.work, "User-Added"));
                File.WriteAllText(Path.Combine(Misc.Paths.current, "User-Added.rbin"), "");
                if (!string.IsNullOrWhiteSpace(files[0].Path.LocalPath))
                {
                    if (Path.GetExtension(files[0].Path.LocalPath) == ".rbin")
                    {
                        try
                        {
                            File.Copy(files[0].Path.LocalPath, Path.Combine(Misc.Paths.current, Path.GetFileName(files[0].Path.LocalPath)));
                            AddFile(Path.GetFileName(files[0].Path.LocalPath));
                            RBIN.Load(files[0].Path.LocalPath);
                        }
                        catch
                        {
                            Log.Text = "You attempted to open a file that already exists. Use \"Replace\" if this was your intention.";
                        }
                    }
                    else if (Path.GetExtension(files[0].Path.LocalPath) == ".ctt")
                    {
                        string[] files2 = Directory.GetFiles(Path.Combine(Misc.Paths.work, "User-Added"), "*.*", SearchOption.AllDirectories);
                        File.Copy(files[0].Path.LocalPath, Path.Combine(Misc.Paths.work, "User-Added", $"{files2.Length}-{Path.GetFileName(files[0].Path.LocalPath)}"), true);
                        CTT.Decode(Path.Combine(Misc.Paths.work, "User-Added", $"{files2.Length}-{Path.GetFileName(files[0].Path.LocalPath)}"));
                    }
                    else if (Containers.IsArc(files[0].Path.LocalPath))
                    {
                        string[] files2 = Directory.GetFiles(Path.Combine(Misc.Paths.work, "User-Added"), "*.*", SearchOption.AllDirectories);
                        File.Copy(files[0].Path.LocalPath, Path.Combine(Misc.Paths.work, "User-Added", $"{files2.Length}-{Path.GetFileName(files[0].Path.LocalPath)}"), true);
                        Containers.Generic.Unpack(Path.Combine(Misc.Paths.work, "User-Added", $"{files2.Length}-{Path.GetFileName(files[0].Path.LocalPath)}"));
                    }
                    else
                    {
                        string[] files2 = Directory.GetFiles(Path.Combine(Misc.Paths.work, "User-Added"), "*.*", SearchOption.AllDirectories);
                        File.Copy(files[0].Path.LocalPath, Path.Combine(Misc.Paths.work, "User-Added", $"{files2.Length}-{Path.GetFileName(files[0].Path.LocalPath)}"), true);
                    }
                    if (Path.GetExtension(files[0].Path.LocalPath) != ".rbin" && !File.Exists(Path.Combine(Misc.Paths.current, "User-Added.rbin")))
                    {
                        File.WriteAllText(Path.Combine(Misc.Paths.current, "User-Added.rbin"), "");
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private ContextMenu Cont1()
        {
            var contextMenu = new ContextMenu();
            var open = new MenuItem
            {
                Header = "Open Folder",
            };
            open.Click += OpenFolder_Click;
            contextMenu.Items.Add(open);

            var rex = new MenuItem
            {
                Header = "Re-extract",
            };
            rex.Bind(Avalonia.Controls.MenuItem.IsVisibleProperty, new Avalonia.Data.Binding
            {
                Source = this,
                Path = "rexIsVisible",
                Mode = Avalonia.Data.BindingMode.OneWay
            });
            rex.Click += Again_Click;
            contextMenu.Items.Add(rex);

            var replace = new MenuItem
            {
                Header = "Replace",
            };
            replace.Bind(Avalonia.Controls.MenuItem.IsVisibleProperty, new Avalonia.Data.Binding
            {
                Source = this,
                Path = "replaceIsVisible",
                Mode = Avalonia.Data.BindingMode.OneWay
            });
            replace.Click += Replace_Click;
            contextMenu.Items.Add(replace);

            var remove = new MenuItem
            {
                Header = "Remove File",
            };
            remove.Bind(Avalonia.Controls.MenuItem.IsVisibleProperty, new Avalonia.Data.Binding
            {
                Source = this,
                Path = "removeIsVisible",
                Mode = Avalonia.Data.BindingMode.OneWay
            });
            remove.Click += RemoveFile;
            contextMenu.Items.Add(remove);

            return contextMenu;
        }

        private ContextMenu Cont2()
        {
            var contextMenu = new ContextMenu();
            var open = new MenuItem()
            {
                Header = "Open Folder"
            };
            open.Click += OpenFolder2_Click;
            contextMenu.Items.Add(open);

            var replace = new MenuItem()
            {
                Header = "Replace"
            };
            replace.Click += Replace2_Click;
            contextMenu.Items.Add(replace);

            var pack = new MenuItem()
            {
                Header = "Pack"
            };
            pack.Click += Pack2_Click;
            contextMenu.Items.Add(pack);

            var remove = new MenuItem()
            {
                Header = "Remove File"
            };
            remove.Click += RemoveFile2;
            remove.Bind(Avalonia.Controls.MenuItem.IsVisibleProperty, new Avalonia.Data.Binding
            {
                Source = this,
                Path = "remove2IsVisible",
                Mode = Avalonia.Data.BindingMode.OneWay
            });
            contextMenu.Items.Add(remove);

            var flag = new MenuItem()
            {
                Header = "Queue/Unqueue Pack"
            };
            flag.Click += Flag2;
            flag.Bind(Avalonia.Controls.MenuItem.IsVisibleProperty, new Avalonia.Data.Binding
            {
                Source = this,
                Path = "flag2IsVisible",
                Mode = Avalonia.Data.BindingMode.OneWay
            });
            contextMenu.Items.Add(flag);

            return contextMenu;
        }

        private ContextMenu Cont3()
        {
            var contextMenu = new ContextMenu();
            var open = new MenuItem()
            {
                Header = "Open Folder"
            };
            open.Click += OpenFolder3_Click;
            contextMenu.Items.Add(open);

            var replace = new MenuItem()
            {
                Header = "Replace"
            };
            replace.Click += Replace3_Click;
            contextMenu.Items.Add(replace);

            var pack = new MenuItem()
            {
                Header = "Pack"
            };
            pack.Click += Pack3_Click;
            contextMenu.Items.Add(pack);

            var flag = new MenuItem()
            {
                Header = "Queue/Unqueue Pack"
            };
            flag.Click += Flag3;
            flag.Bind(Avalonia.Controls.MenuItem.IsVisibleProperty, new Avalonia.Data.Binding
            {
                Source = this,
                Path = "flag3IsVisible",
                Mode = Avalonia.Data.BindingMode.OneWay
            });
            contextMenu.Items.Add(flag);
            return contextMenu;
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
            
            var contextMenu = Cont1();
            contextMenu.Opened += (s, e) => {
            TextBox_PreviewMouseLeftButtonDown(newTextBox, null);
            };
            newTextBox.ContextMenu = contextMenu; 
            newTextBox.Classes.Add("NoHover");
            newTextBox.PointerReleased += TextBox_Click;
            newTextBox.PointerPressed += TextBox_PreviewMouseLeftButtonDown;
            Files.Children.Add(newTextBox);
        }

        private void AddFile2(string filename)
        {
            TextBox newTextBox = new TextBox
            {
                Name = filename,
                Text = Misc.RemoveAtFirst(filename, '-'),
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
            var contextMenu = Cont2();
            contextMenu.Opened += (s, e) => {
                TextBox2_PreviewMouseLeftButtonDown(newTextBox, null);
            };
            newTextBox.ContextMenu = contextMenu;
            newTextBox.Classes.Add("NoHover");
            newTextBox.PointerReleased += TextBox2_Click;
            newTextBox.PointerPressed += TextBox2_PreviewMouseLeftButtonDown;
            Files2.Children.Add(newTextBox);
        }
        private void AddFile3(string filename)
        {
            TextBox newTextBox = new TextBox
            {
                Name = filename,
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
            var contextMenu = Cont3();
            contextMenu.Opened += (s, e) => {
                TextBox3_PreviewMouseLeftButtonDown(newTextBox, null);
            };
            newTextBox.ContextMenu = contextMenu;
            newTextBox.Classes.Add("NoHover");
            newTextBox.PointerReleased += TextBox3_Click;
            newTextBox.PointerPressed += TextBox3_PreviewMouseLeftButtonDown;
            Files3.Children.Add(newTextBox);
            if (Files3.Children.Count == 1)
            {
                TextBox3_PreviewMouseLeftButtonDown(newTextBox, null);
                TextBox3_Click(newTextBox, null);
            }
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                selectedTextBox = tb;
                tb.Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#F04080"));
                Log.Text = $"Selected: {tb.Text}";
                if (tb.Text == "User-Added.rbin")
                {
                    rexIsVisible = false;
                    removeIsVisible = false;
                    replaceIsVisible = false;
                    flag2IsVisible = false;
                    flag3IsVisible = false;
                    remove2IsVisible = true;
                }
                else
                {
                    rexIsVisible = true;
                    removeIsVisible = true;
                    replaceIsVisible = true;
                    flag2IsVisible = true;
                    flag3IsVisible = true;
                    remove2IsVisible = false;
                }
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
            Sort.SelectedIndex = 0;
            InfoWindow.IsVisible = false;
            HexWindow.IsVisible = false;
            foreach (var child in Files.Children)
            {
                Files2.Children.Clear();
                Log.Text = "Loading...";
                if (child is TextBox textBox && textBox == selectedTextBox)
                {
                    if (Directory.Exists(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(textBox.Text))))
                    {
                        Log.Text = $"Files in {textBox.Text}";
                        List<string> Paths = new List<string>();
                        // Get all files in the folder
                        string[] files = Directory.GetFiles(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(textBox.Text)), "*.*", SearchOption.AllDirectories);

                        // Iterate and print each file path
                        foreach (string file in files)
                        {
                            string filetrim = file.Replace(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(textBox.Text)) + Path.DirectorySeparatorChar, "");
                            if (!filetrim.Contains(Path.DirectorySeparatorChar) && 
                                !filetrim.Contains(".bmp") && 
                                !filetrim.Contains(".png") && 
                                !filetrim.Contains(".txt") && 
                                !filetrim.Contains(".json") && 
                                !filetrim.Contains(".pnt"))
                            {
                                AddFile2(filetrim);
                            }
                        }
                    }
                    break;
                }
            }

            if (!(sender is TextBox tb1 && tb1.Text == "User-Added.rbin"))
            {
                var sorted = Files2.Children
                    .OfType<TextBox>()
                    .OrderBy(tb => int.Parse(tb.Name.Split('-')[0]))
                    .ToList();

                Files2.Children.Clear();
                unfiltered.Clear();

                foreach (var textBox in sorted)
                {
                    Files2.Children.Add(textBox);
                    unfiltered.Add(textBox);
                }
            }
            else
            {
                var sorted = Files2.Children
                    .OfType<TextBox>()
                    .OrderBy(tb => tb.Name)
                    .ToList();

                Files2.Children.Clear();
                unfiltered.Clear();

                foreach (var textBox in sorted)
                {
                    Files2.Children.Add(textBox);
                    unfiltered.Add(textBox);
                }
            }
        }
        private void TextBox2_PreviewMouseLeftButtonDown(object sender, PointerPressedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                selectedTextBox2 = tb;
                tb.Background = new SolidColorBrush(Avalonia.Media.Color.Parse("#F04080"));
                Log.Text = $"Selected: {tb.Text}";
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
                Files3_Scroll.ScrollToHome();
            }
            catch { }
            InfoWindow.IsVisible = false;
            HexWindow.IsVisible = false;
            foreach (var child in Files2.Children)
            {
                Files3.Children.Clear();
                Log.Text = "Loading...";
                if (child is TextBox textBox && textBox == selectedTextBox2)
                {
                    string filepath = Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text) + Path.DirectorySeparatorChar, textBox.Name);
                    if (!textBox.Name.Contains(Path.DirectorySeparatorChar) && textBox.Name.EndsWith(".ctt"))
                    {
                        Log.Text = $"Displaying {textBox.Text}";
                        AssignImage(filepath, 2);
                    }
                    else if (textBox.Name.EndsWith(".txa"))
                    {
                        AnimWindow anim = new AnimWindow(filepath);
                        anim.Show();
                    }
                    else if (Directory.Exists(Path.Combine(Path.GetDirectoryName(filepath), Path.GetFileNameWithoutExtension(filepath))))
                    {
                        Log.Text = $"Files in {textBox.Name}";
                        List<string> Paths = new List<string>();
                        // Get all files in the folder
                        string[] files = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(filepath), Path.GetFileNameWithoutExtension(filepath)), "*.*", SearchOption.AllDirectories);
                        Array.Sort(files);
                        // Iterate and print each file path
                        foreach (string file in files)
                        {
                            string filetrim = file.Replace(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), Path.GetFileNameWithoutExtension(textBox.Name) + Path.DirectorySeparatorChar), "");
                            if (!filetrim.Contains(Path.DirectorySeparatorChar) && 
                                !filetrim.Contains(".bmp") && 
                                !filetrim.Contains(".png") && 
                                !filetrim.Contains(".txt") && 
                                !filetrim.Contains(".json") && 
                                !filetrim.Contains(".pnt"))
                            {
                                AddFile3(filetrim);
                            }
                        }
                        var sorted = Files3.Children
                            .OfType<TextBox>()
                            .OrderBy(tb => tb.Name)
                            .ToList();

                        Files3.Children.Clear();
                        foreach (var textBox2 in sorted)
                        {
                            Files3.Children.Add(textBox2);
                        }
                        InfoWindow.IsVisible = true;
                    }
                    else
                    {
                        Log.Text = $"The file \"{textBox.Name}\" cannot be displayed.";
                        byte[] fileData = File.ReadAllBytes(filepath);

                        HexBytePanel.Foreground = new SolidColorBrush(Avalonia.Media.Color.Parse($"#F04080"));
                        HexBytePanel.FontFamily = new FontFamily("avares://Nightmare Editor AUI/Assets/fonts#JetBrains Mono");
                        HexBytePanel.Margin = new Thickness(5, 0);
                        for (int i = 0; i < fileData.Length; i++)
                        {
                            if (i != 0 && i % 16 == 0)
                            {
                                HexBytePanel.Text += "\n";
                            }

                            string text = fileData[i].ToString("X");
                            if (text.Length < 2)
                            {
                                text = "0" + text;
                            }
                            HexBytePanel.Text += text;
                            if (i % 16 != 15)
                            {
                                HexBytePanel.Text += " ";
                            }
                        }
                        HexFileName.Text = filepath.Replace(Misc.Paths.work + Path.DirectorySeparatorChar, "");
                        HexWindow.IsVisible = true;
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
                Log.Text = $"Selected: {tb.Text}";
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
            foreach (var child in Files3.Children)
            {
                InfoWindow.IsVisible = false;
                Log.Text = "Loading...";
                if (child is TextBox textBox && textBox == selectedTextBox3)
                {
                    string filepath = Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), Path.GetFileNameWithoutExtension(selectedTextBox2.Name));
                    Log.Text = $"The file \"{textBox.Text}\" cannot be displayed.";
                    if (Directory.Exists(filepath))
                    {
                        List<string> Paths = new List<string>();
                        // Get all files in the folder
                        string[] files = Directory.GetFiles(filepath, "*.*", SearchOption.AllDirectories);

                        // Iterate and print each file path
                        foreach (string file in files)
                        {
                            string filetrim = file.Replace(filepath + Path.DirectorySeparatorChar, "");
                            if (!filetrim.Contains(Path.DirectorySeparatorChar) && filetrim.EndsWith(".ctt"))
                            {
                                Log.Text = $"Displaying {textBox.Text}";
                                AssignImage(file, 3);
                            }
                        }
                    }
                    break;
                }
            }
        }
        
        private async void AssignImage(string file, int from)
        {
            string path = ""; 
            HexWindow.IsVisible = false;
            if (Path.GetFileName(file) == selectedTextBox2.Name)
            {
                path = Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), file);
            }
            else if (Path.GetFileName(file) == selectedTextBox3.Text)
            {
                path = Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text),
                    Path.GetFileNameWithoutExtension(selectedTextBox2.Name), file);
            }
            else
            {
                return;
            }
            FileFormat.Text = ((CTT.Format)File.ReadAllBytes(path)[0x1C]).ToString();;
            MemoryStream ms = new MemoryStream();
            NewTools.CTT.Decode(file, false).SaveAsPng(ms);
            ms.Seek(0, SeekOrigin.Begin);
            Bitmap bitmap = new Bitmap(ms);
            if (textureSwap)
            {
                Texture.Source = bitmap;
            }
            else
            {
                TextureSmall.Source = bitmap;
            }
            FileName.Text = path.Replace(Misc.Paths.work + Path.DirectorySeparatorChar, "");
            FileLink.Text = path;
            FileSize.Text = bitmap.PixelSize.Width.ToString() + "x" + bitmap.PixelSize.Height.ToString();
            bool found = false;
            if (!(textureLinks.Count <= 0))
            {
                foreach (var arr in textureLinks)
                {
                    if (arr.Length >= 2 && arr[0] == FileName.Text)
                    {
                        if (!File.Exists(arr[1]))
                        {
                            var box2 = MessageBoxManager.GetMessageBoxStandard(
                                $"Missing texture",
                                $"{arr[1]} could not be found. Removing from the texture list.",
                                MsBox.Avalonia.Enums.ButtonEnum.Ok,
                                MsBox.Avalonia.Enums.Icon.Info
                            );
                            await box2.ShowAsPopupAsync(this);
                            textureLinks.Remove(arr);
                            QuickJson(true);
                            FileLink.Text = path;
                            break;
                        }
                        else
                        {
                            Bitmap bitmap2 = new Bitmap(File.OpenRead(arr[1]));
                            if (textureSwap)
                            {
                                TextureSmall.Source = bitmap2;
                            }
                            else
                            {
                                Texture.Source = bitmap2;
                            }

                            FileLink.Text = arr[1];
                            found = true;
                            break;
                        }
                    }
                }
            }
            if (!found)
            {
                if (textureSwap)
                {
                    TextureSmall.Source = bitmap;
                }
                else
                {
                    Texture.Source = bitmap;
                }
            }
            InfoWindow.IsVisible = true;
        }

        private async void RemoveFile(object sender, RoutedEventArgs e)
        {
            foreach (var child in Files.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard(
                        $"Delete {textBox.Text}",
                        $"Do you wish to delete {textBox.Text}?",
                        ButtonEnum.YesNo,
                        MsBox.Avalonia.Enums.Icon.Question
                    );
                    var result = await box.ShowAsPopupAsync(this);
                    if (result == ButtonResult.Yes)
                    {
                        try
                        {
                            Directory.Delete(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(textBox.Text)), true);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                        }
                        try
                        {
                            Directory.Delete(Path.Combine(Misc.Paths.basePath, Path.GetFileNameWithoutExtension(textBox.Text)), true);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                        }
                        try
                        {
                            File.Delete(Path.Combine(Misc.Paths.current, textBox.Text));
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                        }
                        try
                        {
                            Files.Children.Remove(textBox);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                        }
                        break;
                    }
                }
            }
        }
        private async void RemoveFile2(object sender, RoutedEventArgs e)
        {
            foreach (var child in Files2.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox2)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard(
                        $"Delete {textBox.Text}",
                        $"Do you wish to delete {textBox.Text}?",
                        ButtonEnum.YesNo,
                        MsBox.Avalonia.Enums.Icon.Question
                    );
                    var result = await box.ShowAsPopupAsync(this);
                    if (result == ButtonResult.Yes)
                    {
                        if (Directory.Exists(Path.Combine(Misc.Paths.work, "User-Added", Path.GetFileNameWithoutExtension(textBox.Text))));
                            Directory.Delete(Path.Combine(Misc.Paths.work, "User-Added", Path.GetFileNameWithoutExtension(textBox.Text)), true);
                        File.Delete(Path.Combine(Misc.Paths.work, "User-Added", textBox.Text));
                        Files2.Children.Remove(textBox);
                        break;
                    }
                }
            }
        }

        private async void Again_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in Files.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard(
                        $"Re-Extract {textBox.Text}",
                        $"Do wish to extract {textBox.Text} again? This will replace all files inside.",
                        ButtonEnum.YesNo,
                        MsBox.Avalonia.Enums.Icon.Question
                    );
                    var result = await box.ShowAsPopupAsync(this);
                    if (result == ButtonResult.Yes)
                    {
                        RBIN.Load(Path.Combine(Misc.Paths.current, textBox.Text));
                    }
                    break;
                }
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Path.Combine(Misc.Paths.current, selectedTextBox.Text)))
            {
                ProcessStartInfo StartInformation = new ProcessStartInfo();
                StartInformation.FileName = Misc.Paths.current;
                StartInformation.UseShellExecute = true;
                Process process = Process.Start(StartInformation);
            }
        }

        private void OpenFolder2_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), selectedTextBox2.Name)))
            {
                string file = Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), selectedTextBox2.Name);
                ProcessStartInfo StartInformation = new ProcessStartInfo();
                StartInformation.FileName = Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text));
                StartInformation.UseShellExecute = true;
                Process process = Process.Start(StartInformation);
            }
        }

        private void OpenFolder3_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), Path.GetFileNameWithoutExtension(selectedTextBox2.Name), selectedTextBox3.Text)))
            {
                string file = Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), Path.GetFileNameWithoutExtension(selectedTextBox2.Name), selectedTextBox3.Text);
                ProcessStartInfo StartInformation = new ProcessStartInfo();
                StartInformation.FileName = Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), Path.GetFileNameWithoutExtension(selectedTextBox2.Name));
                StartInformation.UseShellExecute = true;
                Process process = Process.Start(StartInformation);
            }
        }

        private FilePickerFileType MatchFilter(string sender)
        {
            var enumValues = Enum.GetValues(typeof(Misc.FileTypes)).Cast<Misc.FileTypes>().ToArray();
            for (int i = 0; i < enumValues.Length; i++)
            {
                if (sender.EndsWith(enumValues[i].ToString()))
                {
                    return Misc.FileFilters.all[i];
                }
            }
            return null;
        }

        private async void Replace_Click(object sender, RoutedEventArgs e)
        {
            var file = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select a file to open...",
                AllowMultiple = false,
                FileTypeFilter = Misc.FileFilters.rbin,
            });
            try
            {
                if (!string.IsNullOrWhiteSpace(file[0].Path.LocalPath))
                {
                    File.Copy(file[0].Path.LocalPath, Path.Combine(Misc.Paths.current, selectedTextBox.Text), true);
                    RBIN.Load(file[0].Path.LocalPath);
                }
            }
            catch (Exception ex)
            {
                Log.Text = ex.Message;
            }
        }

        private async void Replace2_Click(object sender, RoutedEventArgs e)
        {
            var filter = new List<FilePickerFileType>();

            foreach (var child in Files2.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox2)
                {
                    FilePickerFileType type = MatchFilter(textBox.Text);
                    if (type != null)
                    {
                        filter.Add(type);
                    }
                    else
                    {
                        filter.Add(new FilePickerFileType($"{Path.GetExtension(textBox.Text)} files")
                        {
                            Patterns = new List<string> { $"*{Path.GetExtension(textBox.Text)}" }
                        });
                    }
                    break;
                }
            }
            var file = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select a file to open...",
                AllowMultiple = false,
                FileTypeFilter = filter,
            });
            try
            {
                if (!string.IsNullOrWhiteSpace(file[0].Path.LocalPath))
                {
                    try
                    {
                        if (Path.GetExtension(file[0].Path.LocalPath) == ".ctt")
                        {
                            string path = Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), selectedTextBox2.Name);
                            File.Copy(file[0].Path.LocalPath, path, true);
                            CTT.Decode(path);
                        }
                        else if (Containers.IsArc(file[0].Path.LocalPath))
                        {
                            string path = Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), selectedTextBox2.Name);
                            File.Copy(file[0].Path.LocalPath, path, true);
                            Containers.Generic.Unpack(path);
                            
                        }
                        else
                        {
                            File.Copy(file[0].Path.LocalPath, Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), selectedTextBox2.Name), true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Text = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Text = ex.Message;
            }
        }

        private void Pack2_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in Files2.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox2)
                {
                    if (textBox.Text.EndsWith(".ctt"))
                    {
                        string file2 = "";
                        bool found = false;
                        foreach (var arr in textureLinks)
                        {
                            if (arr.Length >= 2 && arr[0] == FileName.Text)
                            {
                                file2 = arr[1];
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            NewTools.CTT.Decode(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), Path.GetFileName(selectedTextBox2.Name)), true);
                            string[] files2 = Directory.GetFiles(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text)), $"{selectedTextBox2.Name}.*.png", SearchOption.AllDirectories);
                            file2 = files2[0];
                        }
                        FileName.Text = selectedTextBox.Text + Path.DirectorySeparatorChar + selectedTextBox2.Name;
                        Log.Text = "Packing...";
                        NewTools.CTT.Encode(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), textBox.Name), file2);
                        Log.Text = $"Packed {textBox.Text}!";
                    }
                    
                    else if (Containers.IsArc(textBox.Text) && Directory.Exists(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), Path.GetFileNameWithoutExtension(textBox.Name))))
                    {
                        ShowGenericWarning();
                        Containers.Generic.Pack(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), textBox.Name));
                        Log.Text = $"Packed {textBox.Text}!";
                    }
                    else
                    {
                        Log.Text = $"{textBox.Text} is not an archive nor texture file, and cannot be packed.";
                    }    
                    break;
                }

            }
        }
        private async void Replace3_Click(object sender, RoutedEventArgs e)
        {
            var filter = new List<FilePickerFileType>();

            foreach (var child in Files2.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox2)
                {
                    FilePickerFileType type = MatchFilter(textBox.Text);
                    if (type != null)
                    {
                        filter.Add(type);
                    }
                    else
                    {
                        filter.Add(new FilePickerFileType($"{Path.GetExtension(textBox.Text)} files")
                        {
                            Patterns = new List<string> { $"*{Path.GetExtension(textBox.Text)}" }
                        });
                    }
                    break;
                }
            }
            var file = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select a file to open...",
                AllowMultiple = false,
                FileTypeFilter = filter,
            });
            try
            {
                if (!string.IsNullOrWhiteSpace(file[0].Path.LocalPath))
                {
                    try
                    {
                        if (Path.GetExtension(file[0].Path.LocalPath) == ".ctt")
                        {
                            File.Copy(file[0].Path.LocalPath, Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), Path.GetFileNameWithoutExtension(selectedTextBox2.Name), selectedTextBox3.Text), true);
                            File.Copy(file[0].Path.LocalPath, Path.Combine(Misc.Paths.toolkit, selectedTextBox3.Text), true);
                            CTT.Decode(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), Path.GetFileNameWithoutExtension(selectedTextBox2.Name), Path.GetFileName(selectedTextBox3.Text)));

                        }
                        else
                        {
                            File.Copy(file[0].Path.LocalPath, Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), Path.GetFileNameWithoutExtension(selectedTextBox2.Name), selectedTextBox3.Text), true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Text = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Text = ex.Message;
            }
        }

        private void Pack3_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in Files3.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox3)
                {
                    if (textBox.Text.EndsWith(".ctt"))
                    {
                        string file2 = "";
                        bool found = false;
                        foreach (var arr in textureLinks)
                        {
                            if (arr.Length >= 2 && arr[0] == FileName.Text)
                            {
                                file2 = arr[1];
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            NewTools.CTT.Decode(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), Path.GetFileNameWithoutExtension(selectedTextBox2.Name), Path.GetFileName(selectedTextBox3.Text)), true);
                            string[] files2 = Directory.GetFiles(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), Path.GetFileNameWithoutExtension(selectedTextBox2.Name)), $"{selectedTextBox3.Text}.*.png", SearchOption.AllDirectories);
                            file2 = files2[0];
                        }
                        FileName.Text = selectedTextBox.Text + Path.DirectorySeparatorChar + selectedTextBox2.Name + Path.DirectorySeparatorChar + selectedTextBox3.Text;
                        Log.Text = "Packing...";
                        NewTools.CTT.Encode(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), Path.GetFileNameWithoutExtension(selectedTextBox2.Name), textBox.Text), file2);
                        Log.Text = $"Packed {textBox.Text}!";
                    }
                    else if (Containers.IsArc(textBox.Text) && Directory.Exists(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), Path.GetFileNameWithoutExtension(selectedTextBox2.Name), Path.GetFileNameWithoutExtension(textBox.Text))))
                    {
                        ShowGenericWarning();
                        Containers.Generic.Pack(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedTextBox.Text), Path.GetFileNameWithoutExtension(selectedTextBox2.Name), textBox.Text));
                        Log.Text = $"Packed {textBox.Text}!";
                    }
                    break;
                }
            }
        }
        private void Flag2(object sender, RoutedEventArgs e)
        {
            string file1 = selectedTextBox.Text;
            string file2 = Path.Combine(Path.GetFileNameWithoutExtension(selectedTextBox.Text), selectedTextBox2.Name);
            bool isin = false;
            if (!flaggedFiles.Contains(file1) && !selectedTextBox.Text.Contains("User-Added.rbin"))
            {
                flaggedFiles.Add(file1);
            }
            if (!flaggedFiles2.Contains(file2))
            {
                flaggedFiles2.Add(file2);
            }
            else
            {
                foreach (string fileref in flaggedFiles3)
                {
                    if (fileref.Contains(Path.GetFileNameWithoutExtension(selectedTextBox2.Name) + Path.DirectorySeparatorChar))
                    {
                        isin = true;
                        break;
                    }
                }
                if (isin)
                {
                    Log.Text = "You cannot unflag this file, as another file depends on it.";
                }
                else
                {
                    flaggedFiles2.Remove(file2);
                }
            }
            isin = false;
            foreach (string fileref in flaggedFiles2)
            {
                if (fileref.Contains(Path.GetFileNameWithoutExtension(selectedTextBox.Text) + Path.DirectorySeparatorChar))
                {
                    isin = true;
                    break;
                }
            }
            if (!isin)
            {
                Log.Text = $"No flagged file references {selectedTextBox.Text}, removing from flagged list.";
                flaggedFiles.Remove(file1);
            }

        }
        private void Flag3(object sender, RoutedEventArgs e)
        {
            string file1 = selectedTextBox.Text;
            string file2 = Path.Combine(Path.GetFileNameWithoutExtension(selectedTextBox.Text), selectedTextBox2.Name);
            string file3 = Path.Combine(Path.GetFileNameWithoutExtension(selectedTextBox.Text), Path.GetFileNameWithoutExtension(selectedTextBox2.Name), selectedTextBox3.Text);
            if (!flaggedFiles.Contains(file1) && !selectedTextBox.Text.Contains("User-Added.rbin"))
            {
                flaggedFiles.Add(file1);
            }
            if (!flaggedFiles2.Contains(file2))
            {
                flaggedFiles2.Add(file2);
            }
            if (!flaggedFiles3.Contains(file3))
            {
                flaggedFiles3.Add(file3);
            }
            else
            {
                flaggedFiles3.Remove(file3);
            }
        }
        private async void PackAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (string file in allfiles)
            {
                if (file.EndsWith(".ctt"))
                {
                    Log.Text = $"Packing {file}...";
                    string file2 = "";
                    bool found = false;
                    foreach (var arr in textureLinks)
                    {
                        string texture = arr[0];
                        string[] temp1 = texture.Split(Path.DirectorySeparatorChar);
                        texture = "";
                        for (int i = 0; i < (temp1.Count()); i++)
                        {
                            if (i < (temp1.Count() - 1))
                            {
                                texture += Path.GetFileNameWithoutExtension(temp1[i]) + Path.DirectorySeparatorChar;
                            }
                            else
                            {
                                texture += temp1[i];
                            }
                        }
                        if (arr.Length >= 2 && texture == file)
                        {
                            file2 = arr[1];
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        string[] files2 = Directory.GetFiles(Path.Combine(Misc.Paths.work, Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)), $"{Path.GetFileName(file)}.*.png", SearchOption.AllDirectories);
                        file2 = files2[0];
                    }
                    NewTools.CTT.Encode(Path.Combine(Misc.Paths.work, file), file2);
                    Log.Text = $"Packed {file}!";
                }
                else if (Containers.IsArc(file) && Directory.Exists(Path.Combine(Misc.Paths.work, Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file))))
                {
                    ShowGenericWarning();
                    Containers.Generic.Pack(Path.Combine(Misc.Paths.work, file));
                    Log.Text = $"Packed {file}!";
                }
                else if (file.EndsWith(".rbin"))
                { 
                    BetterDirCopy(Path.Combine(Misc.Paths.basePath, Path.GetFileNameWithoutExtension(file)), Path.Combine(Misc.Paths.pack, Path.GetFileNameWithoutExtension(file)), false, false);
                    RBIN.Pack(Path.Combine(Misc.Paths.pack, file), false, this);
                    Log.Text = $"Packed {file}!";
                }
                if (file.Length - file.Replace(Path.DirectorySeparatorChar.ToString(), "").Length == 1)
                {
                    Directory.CreateDirectory(Path.Combine(Misc.Paths.pack, Path.GetDirectoryName(file)));
                    File.Copy(Path.Combine(Misc.Paths.work, file), Path.Combine(Misc.Paths.pack, file), true);
                }
            }
        }

        private async void ExpMod_Click(object sender, RoutedEventArgs e)
        {
            MakePack finish = new MakePack(new Meta());
            await finish.ShowDialog(this);

            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string jsonString = System.IO.File.ReadAllText(Misc.Jsons.temp);
            Meta mod = JsonSerializer.Deserialize<Meta>(jsonString, jsonoptions);
            foreach (string file in allfiles)
            {
                if (file.EndsWith(".ctt"))
                {
                    Log.Text = $"Packing {file}...";
                    string file2 = "";
                    bool found = false;
                    foreach (var arr in textureLinks)
                    {
                        string texture = arr[0];
                        string[] temp1 = texture.Split(Path.DirectorySeparatorChar);
                        texture = "";
                        for (int i = 0; i < (temp1.Count()); i++)
                        {
                            if (i < (temp1.Count() - 1))
                            {
                                texture += Path.GetFileNameWithoutExtension(temp1[i]) + Path.DirectorySeparatorChar;
                            }
                            else
                            {
                                texture += temp1[i];
                            }
                        }
                        if (arr.Length >= 2 && texture == file)
                        {
                            file2 = arr[1];
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        string[] files2 = Directory.GetFiles(Path.Combine(Misc.Paths.work, Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)), $"{Path.GetFileName(file)}.*.png", SearchOption.AllDirectories);
                        file2 = files2[0];
                    }
                    NewTools.CTT.Encode(Path.Combine(Misc.Paths.work, file), file2);
                    Log.Text = $"Packed {file}!";
                }
                else if (Containers.IsArc(file) && Directory.Exists(Path.Combine(Misc.Paths.work, Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file))))
                {
                    ShowGenericWarning();
                    Containers.Generic.Pack(Path.Combine(Misc.Paths.work, file));
                    Log.Text = $"Packed {file}!";
                }
                if (file.Length - file.Replace(Path.DirectorySeparatorChar.ToString(), "").Length == 1)
                {
                    string endpath = Nightmare_Editor_AUI.Managers.Standard.GetModFolder(mod.ID);
                    if (!string.IsNullOrWhiteSpace(mod.Prefix))
                    {
                        endpath = Path.Combine(endpath, mod.Prefix);
                    }
                    Directory.CreateDirectory(Path.Combine(endpath, Path.GetDirectoryName(file)));
                    File.Copy(Path.Combine(Misc.Paths.work, file), Path.Combine(endpath, file), true);
                }
            }
            File.Delete(Misc.Jsons.temp);
            ZipMod(mod);
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Help hw = new Help();
            hw.Show();
        }

        private void WindowSwap(object sender, RoutedEventArgs e)
        {
            windowSwap = !windowSwap;
            allfiles.Clear();
            allfiles.AddRange(flaggedFiles);
            allfiles.AddRange(flaggedFiles2);
            allfiles.AddRange(flaggedFiles3);
            allfiles.Sort();
            allfiles.Reverse();
            Queued.Children.Clear();
            foreach (string file in allfiles)
            {
                int count = file.Length - file.Replace(Path.DirectorySeparatorChar.ToString(), "").Length;
                string hex = "#000000";
                if (count == 0)
                {
                    hex = "#FF0000";
                }
                else if (count == 1)
                {
                    hex = "#F04080";
                }
                else if (count == 2)
                {
                    hex = "#FF80B0";
                }
                TextBox newTextBox = new TextBox
                {
                    Text = file,
                    IsReadOnly = true,
                    Width = file.Length * 10,
                    Height = 20,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Background = new SolidColorBrush(Avalonia.Media.Color.Parse(hex)),
                    BorderBrush = new SolidColorBrush(Avalonia.Media.Color.Parse("#4080f0")),
                    Foreground = new SolidColorBrush(Avalonia.Media.Color.Parse("#f2f2f2")),
                    Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.No),
                    Focusable = false,
                };
                Queued.Children.Add(newTextBox);
            }
            if (windowSwap)
            {
                Queue.Source = new Bitmap(AssetLoader.Open(new Uri($"avares://Nightmare Editor AUI/Images/unqueue.png", UriKind.RelativeOrAbsolute)));
                if (InfoWindow.IsVisible == true)
                {
                    windowStore = true;
                }
                else
                {
                    windowStore = false;
                }
                Blackout.IsVisible = true;
                InfoWindow.IsVisible = false;
                QueueWindow.IsVisible = true;
            }
            else
            {
                Queue.Source = new Bitmap(AssetLoader.Open(new Uri($"avares://Nightmare Editor AUI/Images/queue.png", UriKind.RelativeOrAbsolute)));
                if (windowStore)
                {
                    InfoWindow.IsVisible = true;
                }
                else
                {
                    InfoWindow.IsVisible = false;
                }
                Blackout.IsVisible = false;
                QueueWindow.IsVisible = false;
            }
        }

        private async void Link_Click(object sender, RoutedEventArgs e)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                $"Texture Link Warning",
                "Please select a texture with the same width and height as the original.\nAn image with a different width and height may not be encoded correctly.",
                MsBox.Avalonia.Enums.ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Info
            );
            await box.ShowAsPopupAsync(this);
            var file = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select a file to open...",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
                    {
                        new FilePickerFileType("Texture File")
                        {
                            Patterns = new List<string> { "*.png" }
                        }
                    }
            });
            if (!string.IsNullOrWhiteSpace(file[0].Path.LocalPath))
            {
                FileLink.Text = file[0].Path.LocalPath;
                Bitmap bitmap = new Bitmap(File.OpenRead(file[0].Path.LocalPath));
                if (textureSwap)
                {
                    TextureSmall.Source = bitmap;
                }
                else
                {
                    Texture.Source = bitmap;
                }
                bool found = false;
                foreach (var arr in textureLinks)
                {
                    if (arr.Length >= 2 && arr[0] == FileName.Text)
                    {
                        arr[1] = file[0].Path.LocalPath;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    textureLinks.Add([FileName.Text, file[0].Path.LocalPath]);
                }
            }
            QuickJson(true);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (FileLink.Text.EndsWith(".ctt"))
            {
                Directory.CreateDirectory(Misc.Paths.temp);
                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(Misc.Paths.temp, FileName.Text)));
                string file = Path.Combine(Misc.Paths.work, FileName.Text);
                string save = Path.Combine(Misc.Paths.temp, FileName.Text);
                var image = CTT.Decode(file, false);
                image.SaveAsPng(save);
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = save,
                    UseShellExecute = true
                });
            }
            else
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = FileLink.Text,
                    UseShellExecute = true
                });
            }
        }

        private async void Unlink_Click(object sender, PointerReleasedEventArgs e)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Texture Unlink",
                "Do you wish to unlink this texture?",
                ButtonEnum.YesNo,
                MsBox.Avalonia.Enums.Icon.Question
            );

            var result = await box.ShowAsPopupAsync(this);

            if (result == ButtonResult.Yes)
            {
                foreach (var arr in textureLinks)
                {
                    if (arr.Length >= 2 && arr[0] == FileName.Text)
                    {
                        textureLinks.Remove(arr);
                        break;
                    }
                }
                if (textureSwap)
                {
                    TextureSmall.Source = Texture.Source;
                }
                else
                {
                    Texture.Source = TextureSmall.Source;
                }
                FileLink.Text = Path.Combine(Misc.Paths.work, FileName.Text);
            }
            QuickJson(true);
        }

        private async void ScrollFileLink(object sender = null, TextChangedEventArgs e = null)
        {
            await Task.Delay(100);
            try
            {
                FileLink.CaretIndex = FileLink.Text.Length;
            }
            catch { }
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
        private void SNN_Click(object sender, PointerReleasedEventArgs e)
        {
            RenderOptions.SetBitmapInterpolationMode(TextureSmall, BitmapInterpolationMode.None);
            TextureSmall.InvalidateVisual();
        }
        private void SLS_Click(object sender, PointerReleasedEventArgs e)
        {
            RenderOptions.SetBitmapInterpolationMode(TextureSmall, BitmapInterpolationMode.HighQuality);
            TextureSmall.InvalidateVisual();
        }

        private void TextureSwap(object sender, RoutedEventArgs e)
        {
            textureSwap = !textureSwap;
            TextureTemp.Source = Texture.Source;
            Texture.Source = TextureSmall.Source;
            TextureSmall.Source = TextureTemp.Source;
            LocationTemp.Text = Location.Text;
            Location.Text = LocationSmall.Text;
            LocationSmall.Text = LocationTemp.Text;
        }

        private void FileLink_Click(object sender, PointerReleasedEventArgs e)
        {
            ScrollFileLink();
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                Unlink_Click(sender, e);
            }
        }

        private void Files2_Filter(object sender, SelectionChangedEventArgs e)
        {
            if (Search.Text == null)
            {
                Search.Text = "";
            }
            try
            {
                Files2.Children.Clear();
                filtered.Clear();
                string filter = "";
                if (Sort.SelectedIndex == 0)
                {
                    filter = "";
                }
                if (Sort.SelectedIndex == 1)
                {
                    filter = ".ctt";
                }
                else if (Sort.SelectedIndex == 2)
                {
                    filter = ".l2d";
                }
                else if (Sort.SelectedIndex == 3)
                {
                    filter = ".fep";
                }
                else if (Sort.SelectedIndex == 4)
                {
                    filter = ".pmo";
                }
                else if (Sort.SelectedIndex == 5)
                {
                    filter = ".pmp";
                }
                else if (Sort.SelectedIndex == 6)
                {
                    filter = ".txa";
                }
                foreach (var textbox in unfiltered)
                {
                    if (textbox.Text.Contains(filter) && (textbox.Text.StartsWith(Search.Text) || string.IsNullOrWhiteSpace(Search.Text)))
                    {
                        Files2.Children.Add(textbox);
                    }
                }
            }
            catch { }
        }

        private void Reverse_Rebirth(object sender, RoutedEventArgs e)
        {
            if (Nightmare_Editor_AUI.Managers.Standard.MainSettings.UI == 0)
            {
                Manager mw = new Manager();
                mw.Show();
                Close();
            }
            else
            {
                NewManager nmw = new NewManager();
                nmw.Show();
                Close();
            }
        }

        private async void ZipMod(Meta meta)
        {
            string folderpath = Nightmare_Editor_AUI.Managers.Standard.GetModFolder(meta.ID);
            if (Directory.Exists(folderpath))
            {
                try
                {
                    Misc.CopyDirectory(folderpath, Path.Combine(Misc.Paths.temp, meta.ID, meta.Name), true);

                    var jsonoptions = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    string jsonString = JsonSerializer.Serialize(meta, jsonoptions);
                    string filepath = Path.Combine(Misc.Paths.temp, meta.ID, meta.Name, "meta.json");
                    System.IO.File.WriteAllText(filepath, jsonString);
                    var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                    {
                        Title = "Save Nightmare Editor Mod",
                        FileTypeChoices = new List<FilePickerFileType>
                        {
                            new FilePickerFileType("Nightmare Editor Mod")
                            {
                                Patterns = new List<string> { "*.nem" }
                            }
                        }
                    });
                    if (file == null)
                    {
                        Console.WriteLine("Save file operation canceled.");
                        return;
                    }
                    using var archive = SharpCompress.Archives.Zip.ZipArchive.CreateArchive();
                    archive.AddAllFromDirectory(Path.Combine(Misc.Paths.temp, meta.ID));
                    archive.SaveTo(file.Path.LocalPath, SharpCompress.Common.CompressionType.Deflate);
                    Directory.Delete(Path.Combine(Misc.Paths.temp, meta.ID, meta.Name), true);
                }
                catch { }
            }
        }

        private void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Files2_Filter(null, null);
            }
        }

        private async void SaveTex_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var save = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
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
                if (!string.IsNullOrWhiteSpace(save.Path.LocalPath) && save != null)
                {
                    string file = Path.Combine(Misc.Paths.work, FileName.Text);
                    var image = CTT.Decode(file, false);
                    image.SaveAsPng(save.Path.LocalPath);
                }
            }
            catch (Exception exception)
            {

            }
        }

        public async void ShowGenericWarning()
        {
            var box2 = MessageBoxManager.GetMessageBoxStandard(
                "Generic Warning",
                "This file uses \"generic\" replacement, meaning you can't change the format or size of the texture. Sorry for the inconvenience!",
                MsBox.Avalonia.Enums.ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Info
            );
            await box2.ShowAsPopupAsync(this);
        }
    }
}