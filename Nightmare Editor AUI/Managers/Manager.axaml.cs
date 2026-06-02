using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Nightmare_Editor;
using Nightmare_Editor.NewTools;
using System.Collections.ObjectModel;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using SharpCompress;
using Avalonia.Platform.Storage;
using SharpCompress.Archives;
using SharpCompress.Common;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Nightmare_Editor_AUI.ViewModels;
using LibGit2Sharp;
using Nightmare_Editor_AUI;
using static Nightmare_Editor_AUI.Managers.Standard;
using Path = System.IO.Path;


namespace Nightmare_Editor
{
    /// <summary>
    /// Interaction logic for Manager.axaml
    /// </summary>
    public partial class Manager : Window
    {
        /// This is largely copied from Pulsar. It's software also developed by me.
        private List<string> enabledmods = new List<string>();
        private bool isInitialized = false;
        private List<string[]> music = new List<string[]>();
        private MainWindowViewModel viewModel = new MainWindowViewModel();

        public Manager()
        {
            InitializeComponent();
            ModsWindow(true);
            ModsButton.Content = MainButtonContent(true, "Mods");
            SettingsButton.Content = MainButtonContent(false, "Settings");
            MusicButton.Content = MainButtonContent(false, "Music");
            SettingsWindow.IsVisible = false;
            MusicWindow.IsVisible = false;
            Directory.CreateDirectory(Misc.Paths.mods);
            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            if (!System.IO.File.Exists(Misc.Jsons.enabled))
            {
                string jsonString = JsonSerializer.Serialize<List<string>>(new List<string>(), jsonoptions);
                System.IO.File.WriteAllText(Misc.Jsons.enabled, jsonString);
            }
            Refresh();
            isInitialized = true;
            DataContext = viewModel;
            
        }
        
        public static Grid MainButtonContent(bool on, string text)
        {
            Grid grid = new Grid();
            Viewbox vb1 = new Viewbox();
            Viewbox vb2 = new Viewbox();
            Viewbox vb3 = new Viewbox();
            TextBlock tb = new TextBlock();
            Rectangle rect1 = new Rectangle();
            Rectangle rect2 = new Rectangle();
            
            vb2.Margin = new Thickness(8);
            if (!on)
            {
                vb2.Margin = new Thickness(5);
            }
            vb1.Stretch = Stretch.Fill;
            vb2.Stretch = Stretch.Fill;

            tb.Text = text;
            tb.Margin = new Thickness(5);
            tb.FontWeight = FontWeight.Black;
            tb.Foreground = new SolidColorBrush(Color.Parse("#ADD8E6"));

            rect1.Fill = new SolidColorBrush(Color.Parse("#ADD8E6"));
            if (on)
            {
                tb.Foreground = Brushes.White;
                rect1.Fill = Brushes.White;
            }
            rect2.Fill = new SolidColorBrush(Color.Parse("#FFA580"));
            rect1.Height = 10;
            rect2.Height = 10;
            rect1.Width = 10;
            rect2.Width = 10;

            vb1.Child = rect1;
            vb2.Child = rect2;
            if (on)
            {
                Grid grid2 = new Grid();
                TextBlock tb2 = new TextBlock();
                tb2.Text = text;
                tb2.FontWeight = FontWeight.Black;
                tb2.Margin = new Thickness(6, 6, 4, 4);
                tb2.Foreground = Brushes.Black;
                grid2.Children.Add(tb2);
                grid2.Children.Add(tb);
                vb3.Child = grid2;
            }
            else
            {
                vb3.Child = tb;
            }
            
            grid.Children.Add(vb1);
            grid.Children.Add(vb2);
            grid.Children.Add(vb3);

            return grid;
        }

        private string[] CountFolders(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                string[] directories = Directory.GetDirectories(folderPath);
                int folderCount = directories.Length;
                return directories;
            }
            else
            {
                return null;
            }
        }

        public string CreateLinkImage(string link)
        {
            try
            {
                if (link.Contains("gamebanana.com"))
                {
                    return "Images/Gamebanana.png";
                }
                else if (link.Contains("github.com"))
                {
                    return "Images/Github.png";
                }
                else if (!string.IsNullOrWhiteSpace(link))
                {
                    return "Images/Web.png";
                }
                else
                {
                    return null;
                }
            }
            catch { return null; }
        }
        public void Refresh()
        {
            try
            {
                try
                {
                    enabledmods.Clear();
                }
                catch { }
                enabledmods = QuickJson(false, enabledmods, "enabledmods.json");
            }
            catch { }
            viewModel.AllMods.Clear();
            string[] griditems = CountFolders(Misc.Paths.mods);
            Settings settings = new Settings();
            List<string> blacklist = new List<string>();
            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            List<MusicEntry> musicEntries = JsonSerializer.Deserialize<List<MusicEntry>>(AssetLoader.Open(new Uri($"avares://Nightmare Editor AUI/Music/database.json", UriKind.RelativeOrAbsolute)), jsonoptions);
            MusicInputBox.Items.Clear();
            for (int i = 0; i < musicEntries.Count; i++)
            {
                MusicInputBox.Items.Add(musicEntries[i].Track);
            }

            MusicInputBox.SelectedIndex = 0;
            
            if (System.IO.File.Exists(Misc.Jsons.settings))
            {
                settings = MainSettings;

                PathBox.Text = settings.DeployPath;
                DefPrevBox.SelectedIndex = settings.DefaultImage;
                UsingEmulator.IsChecked = settings.Emulator;
                RegionBox.SelectedIndex = settings.Region;
                ETCBox.SelectedIndex = settings.ETC1Encoder;
                if (settings.DefaultImage < 0)
                {
                    settings.DefaultImage = 0;
                }
                Preview.Source = new Bitmap(AssetLoader.Open(new Uri($"avares://Nightmare Editor AUI/Images/Preview{settings.DefaultImage}.png", UriKind.RelativeOrAbsolute)));
            }

            foreach (string modpath in griditems)
            {
                Meta mod = new Meta();
                string filepath = Path.Combine(modpath, "meta.json");
                if (!System.IO.File.Exists(filepath))
                {
                    string genid = modpath.Replace(Misc.Paths.mods, "");
                    mod.Name = mod.ID = genid = genid.TrimStart(Path.DirectorySeparatorChar);
                    mod.Description = mod.Authors = "";
                    string jsonString = JsonSerializer.Serialize(mod, jsonoptions);
                    System.IO.File.WriteAllText(filepath, jsonString);
                }
                if (System.IO.File.Exists(filepath))
                {
                    string jsonString = System.IO.File.ReadAllText(filepath);
                    mod = JsonSerializer.Deserialize<Meta>(jsonString, jsonoptions);
                    if (!viewModel.AllMods.Contains(mod))
                    {
                        if (enabledmods.Contains(mod.ID))
                            mod.IsChecked = true;
                        else
                            mod.IsChecked = false;
                        mod.LinkImage = CreateLinkImage(mod.Link);
                        viewModel.AllMods.Add(mod);
                    }
                }
            }
            var sorted = viewModel.AllMods.OrderBy(i => i.Name).ToList();
            viewModel.AllMods.Clear();  // Remove all current items
            foreach (var item in sorted)
            {
                viewModel.AllMods.Add(item);  // Re-add in sorted order
            }
            this.DataContext = viewModel;
        }
        private void New_OnClick(object sender, RoutedEventArgs e)
        {
            var ew = new Editor();
            ew.Show();
            Close();
        }

        private void Folder_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in ModDataGrid.SelectedItems)
            {
                Meta row = (Meta)item;
                if (row != null)
                {
                    if (Directory.Exists(Path.Combine(Misc.Paths.mods, row.ID)))
                    {
                        try
                        {
                            ProcessStartInfo StartInformation = new ProcessStartInfo();
                            StartInformation.FileName = Path.Combine(Misc.Paths.mods, row.ID);
                            StartInformation.UseShellExecute = true;
                            Process process = Process.Start(StartInformation);
                        }
                        catch { }
                    }
                }
            }
        }

        private async void Zip_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in ModDataGrid.SelectedItems)
            {
                Meta row = (Meta)item;
                if (row != null)
                {
                    if (Directory.Exists(GetModFolder(row.ID)))
                    {
                        try
                        {
                            Misc.CopyDirectory(GetModFolder(row.ID), Path.Combine(Misc.Paths.temp, row.ID, row.Name), true);

                            var jsonoptions = new JsonSerializerOptions
                            {
                                WriteIndented = true
                            };
                            string jsonString = JsonSerializer.Serialize(row, jsonoptions);
                            string filepath = Path.Combine(Misc.Paths.temp, row.ID, row.Name, "meta.json");
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
                            archive.AddAllFromDirectory(Path.Combine(Misc.Paths.temp, row.ID));
                            archive.SaveTo(file.Path.LocalPath, SharpCompress.Common.CompressionType.Deflate);
                        }
                        catch { }
                    }
                }
            }
            Refresh();
        }

        private async void Delete_OnClick(object sender, RoutedEventArgs e)
        {

            foreach (var item in ModDataGrid.SelectedItems)
            {
                Meta row = (Meta)item;
                if (row != null)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard(
                        $"Delete {row.Name}",
                        $"Are you sure you want to delete {row.Name}?",
                        ButtonEnum.YesNo,
                        MsBox.Avalonia.Enums.Icon.Question
                    );

                    var result = await box.ShowAsPopupAsync(this);

                    if (result == ButtonResult.Yes)
                    {
                        Directory.Delete(GetModFolder(row.ID), true);
                    }
                }
            }
            Refresh();
        }

        private void currentrow(object sender, SelectionChangedEventArgs e)
        {
            Meta row = (Meta)ModDataGrid.SelectedItem;
            if (DefPrevBox.SelectedIndex < 0)
            {
                DefPrevBox.SelectedIndex = 0;
            }
            try
            {
                if (string.IsNullOrWhiteSpace(row.Description) || row == null)
                    DescBox.Text = "Make Sora and Riku's mark of mastery exam your own. You're seeing this because this mod has no description, or no mod is selected.\n\nConfused about the buttons at the bottom? Hover over them for more info.";
                else
                    DescBox.Text = row.Description;
            }
            catch
            {
                DescBox.Text = "Make Sora and Riku's mark of mastery exam your own. You're seeing this because this mod has no description, or no mod is selected.\n\nConfused about the buttons at the bottom? Hover over them for more info.";
            }
            try
            {
                string modpath = GetModFolder(row.ID);
                if (System.IO.File.Exists(Path.Combine(modpath, "preview.webp")))
                {
                    string imagePath = Path.Combine(modpath, "preview.webp");

                    if (File.Exists(imagePath))
                    {
                        using var stream = File.OpenRead(imagePath);
                        Preview.Source = new Bitmap(stream);
                    }
                }
                else if (System.IO.File.Exists(Path.Combine(modpath, "preview.png")))
                {
                    string imagePath = Path.Combine(modpath, "preview.png");

                    if (File.Exists(imagePath))
                    {
                        using var stream = File.OpenRead(imagePath);
                        Preview.Source = new Bitmap(stream);
                    }
                }
                else if (System.IO.File.Exists(Path.Combine(modpath, "preview.jpg")))
                {
                    string imagePath = Path.Combine(modpath, "preview.jpg");

                    if (File.Exists(imagePath))
                    {
                        using var stream = File.OpenRead(imagePath);
                        Preview.Source = new Bitmap(stream);
                    }
                }
                else
                {
                    Preview.Source = new Bitmap(AssetLoader.Open(new Uri($"avares://Nightmare Editor AUI/Images/Preview{DefPrevBox.SelectedIndex}.png", UriKind.RelativeOrAbsolute)));
                }
            }
            catch
            {
                Preview.Source = new Bitmap(AssetLoader.Open(new Uri($"avares://Nightmare Editor AUI/Images/Preview{DefPrevBox.SelectedIndex}.png", UriKind.RelativeOrAbsolute)));
            }
        }

        private void Mods_Click(object sender, RoutedEventArgs e)
        {
            ModsButton.Content = MainButtonContent(true, "Mods");
            SettingsButton.Content = MainButtonContent(false, "Settings");
            MusicButton.Content = MainButtonContent(false, "Music");
            ModsWindow(true);
            MusicWindow.IsVisible = false;
            SettingsWindow.IsVisible = false;
        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            ModsButton.Content = MainButtonContent(false, "Mods");
            SettingsButton.Content = MainButtonContent(true, "Settings");
            MusicButton.Content = MainButtonContent(false, "Music");
            ModsWindow(false);
            MusicWindow.IsVisible = false;
            SettingsWindow.IsVisible = true;
        }
        private void Music_Click(object sender, RoutedEventArgs e)
        {
            ModsButton.Content = MainButtonContent(false, "Mods");
            SettingsButton.Content = MainButtonContent(false, "Settings");
            MusicButton.Content = MainButtonContent(true, "Music");
            ModsWindow(false);
            MusicWindow.IsVisible = true;
            SettingsWindow.IsVisible = false;
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            OpenGamebanana();
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private async void Deploy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Settings settings = MainSettings;
                
                if (!string.IsNullOrWhiteSpace(settings.DeployPath) && settings != null)
                {
                    string text = $@"This will delete all files inside {settings.DeployPath}. Is this okay?";
                    if (settings.Emulator)
                    {
                        text = $"This will delete all files inside the following directories:\n{Path.Combine(settings.DeployPath, "mods", GetTitleIDFromRegion(settings.Region), "romfs")}\n{Path.Combine(settings.DeployPath, "textures", GetTitleIDFromRegion(settings.Region), "NightmareEditor")}\nIs this okay?";
                    }
                    var box = MessageBoxManager.GetMessageBoxStandard(
                        $"Empty {settings.DeployPath}",
                        text,
                        ButtonEnum.YesNo,
                        MsBox.Avalonia.Enums.Icon.Question
                    );

                    var result = await box.ShowAsPopupAsync(this);

                    if (result == ButtonResult.Yes)
                    {
                        var returnValue = DeployMods(settings.DeployPath);
                        if (returnValue.errorCode == ErrorCode.Success)
                        {
                            var box2 = MessageBoxManager.GetMessageBoxStandard(
                                $"Get ready for a fun adventure!",
                                $@"Succesfully deployed mods to {settings.DeployPath}!",
                                MsBox.Avalonia.Enums.ButtonEnum.Ok,
                                MsBox.Avalonia.Enums.Icon.Info
                            );
                            await box2.ShowAsPopupAsync(this);
                        }

                        if (returnValue.errorCode == ErrorCode.MissingRbin)
                        {
                            var box2 = MessageBoxManager.GetMessageBoxStandard(
                                $"Missing {returnValue.errorMessage}",
                                $@"Missing rbin(s) {returnValue.errorMessage}. Unpack them using the unpack button in the settings tab.",
                                MsBox.Avalonia.Enums.ButtonEnum.Ok,
                                MsBox.Avalonia.Enums.Icon.Info
                            );
                            await box2.ShowAsPopupAsync(this);
                        }
                    }
                }
                else
                {
                    var box2 = MessageBoxManager.GetMessageBoxStandard(
                        $"Whoops!",
                        $@"No output directory. Set one in the Settings tab.",
                        MsBox.Avalonia.Enums.ButtonEnum.Ok,
                        MsBox.Avalonia.Enums.Icon.Info
                    );
                    await box2.ShowAsPopupAsync(this);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void ModsWindow(bool sender)
        {
            if (sender == false)
            {
                Mods.IsVisible = false;
                ModContent.IsVisible = false;
            }
            else
            {
                Mods.IsVisible = true;
                ModContent.IsVisible = true;
            }
        }

        private async void Path_Click(object sender, RoutedEventArgs e)
        {
            SetModDeployPath(this);
            PathBox.Text = MainSettings.DeployPath;
        }

        private void PathBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isInitialized) return;
            Settings settings = GetSettings();
            settings.DeployPath = PathBox.Text;
            settings.DefaultImage = DefPrevBox.SelectedIndex;
            settings.Region = RegionBox.SelectedIndex;
            settings.ETC1Encoder = ETCBox.SelectedIndex;
            if (UsingEmulator.IsChecked == true)
            {
                settings.Emulator = true;
            }
            else
            {
                settings.Emulator = false;
            }
            SetSettings(settings);
            Refresh();
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenModsFolder();
        }

        private async void InstallArchive_Click(object sender, RoutedEventArgs e)
        {
            InstallArchive(this);
        }

        private void OpenLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string url)
            {
                try
                {
                    System.Diagnostics.Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch { }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                var row = checkBox.DataContext as Meta;
                if (row != null)
                {
                    if (checkBox.IsChecked == true)
                    {
                        if (!enabledmods.Contains(row.ID))
                            enabledmods.Add(row.ID);
                    }
                    else
                    {
                        if (enabledmods.Contains(row.ID))
                            enabledmods.Remove(row.ID);
                    }
                    QuickJson(true, enabledmods, "enabledmods.json");
                    enabledmods = QuickJson(false, enabledmods, "enabledmods.json");
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                var row = checkBox.DataContext as Meta;
                if (row != null)
                {
                    
                }
            }
        }

        private List<string> QuickJson(bool write, List<string> what, string filename)
        {
            if (write)
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = JsonSerializer.Serialize(what, jsonoptions);
                System.IO.File.WriteAllText(Path.Combine(Misc.Paths.program, filename), jsonString);
                return null;
            }
            else
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = System.IO.File.ReadAllText(Path.Combine(Misc.Paths.program, filename));
                what = JsonSerializer.Deserialize<List<string>>(jsonString, jsonoptions);
                return what;
            }
        }

        public static string[] ListToArray(List<string> sender)
        {
            string[] send = new string[sender.Count];
            for (var i = 0; i < sender.Count; ++i)
                send[i] = sender[i];
            return send;
        }

        private void DefPrevBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInitialized) return;
            var cb = (ComboBox)sender!;
            if (cb.SelectedIndex < 0 && cb.ItemCount > 0)
            {
                cb.SelectedIndex = 0;
            }
            PathBox_TextChanged(null, null);
            Refresh();
        }
        
        private void Emulator_Changed(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;
            PathBox_TextChanged(null, null);
            Refresh();
        }

        private void Edit_OnClick(object sender, RoutedEventArgs e)
        {
            Meta row = (Meta)ModDataGrid.SelectedItem;
            MakePack edit = new MakePack(row);
            Preview.Source = new Bitmap(AssetLoader.Open(new Uri($"avares://Nightmare Editor AUI/Images/Preview{DefPrevBox.SelectedIndex}.png", UriKind.RelativeOrAbsolute)));
            try
            {
                edit.ShowDialog(this);
            }
            catch { }
            Refresh();
        }

        private async void File_Unpack(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(Misc.Paths.current);
            var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select a file to open...",
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Game Archive files")
                    {
                        Patterns = new List<string> { "*.rbin" }
                    }
                },
                AllowMultiple = false
            });
            if (files.Count == 1)
            {
                if (!string.IsNullOrWhiteSpace(files[0].Path.LocalPath))
                {
                    PathBox.Text = files[0].Path.LocalPath;
                }
            }
            if (!string.IsNullOrWhiteSpace(files[0].Path.LocalPath))
            {
                if (Path.GetExtension(files[0].Path.LocalPath) == ".rbin")
                {
                    try
                    {
                        File.Copy(files[0].Path.LocalPath, Path.Combine(Misc.Paths.current, Path.GetFileName(files[0].Path.LocalPath)));
                    }
                    catch
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard(
                            $"Whoops!",
                            "You attempted to unpack an already unpacked file.",
                            MsBox.Avalonia.Enums.ButtonEnum.Ok,
                            MsBox.Avalonia.Enums.Icon.Info
                        );
                        await box.ShowAsPopupAsync(this);
                    }
                    RBIN.Load(files[0].Path.LocalPath);
                }
                else
                {
                    string[] files2 = Directory.GetFiles(Path.Combine(Misc.Paths.work, "User-Added"), "*.*", SearchOption.AllDirectories);
                    File.Copy(files[0].Path.LocalPath, Path.Combine(Misc.Paths.work, "User-Added", $"{files2.Length}-{Path.GetFileName(files[0].Path.LocalPath)}"), true);
                }
            }
        }

        
        private void QuickMusicJson(bool write)
        {
            if (!isInitialized)
            {
                return;
            }
            if (write)
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                MusicList musiclist = new MusicList();
                musiclist.Music = music;
                string jsonString = JsonSerializer.Serialize<MusicList>(musiclist, jsonoptions);
                File.WriteAllText(Misc.Jsons.music, jsonString);
            }
            else
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = File.ReadAllText(Misc.Jsons.music);
                music = JsonSerializer.Deserialize<MusicList>(jsonString, jsonoptions).Music;
            }
        }

        private void Git_Click(object? sender, RoutedEventArgs e)
        {
            Repository.Clone("https://github.com/" + GitRepoBox.Text + ".git", Path.Combine(Misc.Paths.mods, GitRepoBox.Text.Replace('/', '.').Replace('\\', '.')));
            Refresh();
        }
        
        private async void UpdateGit_Click(object? sender, RoutedEventArgs e)
        {
            string[] folders = Directory.GetDirectories(Misc.Paths.mods);
            foreach (string folder in folders)
            {
                try
                {
                    var jsonoptions = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    string jsonString = System.IO.File.ReadAllText(Path.Combine(folder, "meta.json"));
                    Meta meta = JsonSerializer.Deserialize<Meta>(jsonString, jsonoptions);
                    if (meta.Link.Contains("github.com"))
                    {
                        using var repo = new Repository(folder);
                        Commands.Pull(
                            repo,
                            new Signature("NightmareEditor", "nightmare@editor", DateTimeOffset.Now),
                            new PullOptions()
                        );
                        UpdateButton.Content = "Updating " + meta.Name;
                    }
                }
                catch
                {
                    
                }
            }
            UpdateButton.Content = "Update Mods";
            var box = MessageBoxManager.GetMessageBoxStandard(
                $"Done",
                $"Updated all git repositories.",
                ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Success
            );

            await box.ShowAsPopupAsync(this);
            Refresh();
        }

        private void MusicInputBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedIndex != null && comboBox.SelectedValue != null)
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                List<MusicEntry> musicEntries = JsonSerializer.Deserialize<List<MusicEntry>>(AssetLoader.Open(new Uri($"avares://Nightmare Editor AUI/Music/database.json", UriKind.RelativeOrAbsolute)), jsonoptions);
                var correct = musicEntries.FirstOrDefault(me => me.Track == comboBox.SelectedValue.ToString());
                MusicInfoBox1.Text = correct.Description;
                MusicInfoBox2.Text = correct.Filename;
            }
        }

        private async void MusicPathButton_OnClick(object? sender, RoutedEventArgs e)
        {
            var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Select a music file.",
                AllowMultiple = false,
                FileTypeFilter = Misc.FileFilters.bcstm
            });
            if (files.Count == 1)
            {
                if (!string.IsNullOrWhiteSpace(files[0].Path.LocalPath))
                {
                    MusicPathBox.Text = files[0].Path.LocalPath;
                }
            }
        }

        private void MusicReplaceButton_OnClick(object? sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SwitchUI_OnClick(object? sender, RoutedEventArgs e)
        {
            Settings settings = MainSettings;
            settings.UI = 1;
            SetSettings(settings);
            var nmw = new NewManager();
            nmw.Show();
            Close();
        }
    }
}

