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
using Pulsar;
using Nightmare_Editor;
using Nightmare_Editor.NewTools;
using System.Collections.ObjectModel;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Interactivity;
using SharpCompress;
using Avalonia.Platform.Storage;
using SharpCompress.Archives;
using SharpCompress.Common;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Nightmare_Editor_AUI.ViewModels;
using LibGit2Sharp;


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

        public static readonly string UStitleID = "000400000008D300";
        public static readonly string EUtitleID = "0004000000095500";
        public static readonly string JPtitleID = "000400000004EE00";
        public static readonly string JPtitleIDupdate = "0004000E0004EE00";

        public Manager()
        {
            InitializeComponent();
            ModsWindow(true);
            SettingsWindow.IsVisible = false;
            MusicWindow.IsVisible = false;
            Directory.CreateDirectory(Misc.Paths.mods);
            if (!System.IO.File.Exists(Misc.Jsons.settings))
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                Settings settings = new Settings();
                settings.DeployPath = "";
                settings.DefaultImage = 0;
                settings.Region = 0;
                settings.Emulator = false;
                string jsonString = JsonSerializer.Serialize<Settings>(settings, jsonoptions);
                System.IO.File.WriteAllText(Misc.Jsons.settings, jsonString);
            }
            if (!System.IO.File.Exists(Misc.Jsons.enabled))
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = JsonSerializer.Serialize<List<string>>(new List<string>(), jsonoptions);
                System.IO.File.WriteAllText(Misc.Jsons.enabled, jsonString);
            }
            if (!System.IO.File.Exists(Misc.Jsons.music))
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = JsonSerializer.Serialize<List<string[]>>(new List<string[]>(), jsonoptions);
                System.IO.File.WriteAllText(Misc.Jsons.music, jsonString);
            }
            else
            {
                QuickMusicJson(false);
                foreach (var arr in music)
                {
                    if (arr.Length >= 3 && arr[1] == "bgm_001.bcstm")
                    {
                        TTF.SelectedIndex = Int32.Parse(arr[2]);
                    }
                    else if (arr.Length >= 3 && arr[1] == "bgm_014.bcstm")
                    {
                        TTB.SelectedIndex = Int32.Parse(arr[2]);
                    }
                    else if (arr.Length >= 3 && arr[1] == "bgm_011.bcstm")
                    {
                        TGF.SelectedIndex = Int32.Parse(arr[2]);
                    }
                    else if (arr.Length >= 3 && arr[1] == "bgm_020.bcstm")
                    {
                        TGB.SelectedIndex = Int32.Parse(arr[2]);
                    }
                    else if (arr.Length >= 3 && arr[1] == "bgm_012.bcstm")
                    {
                        NWF.SelectedIndex = Int32.Parse(arr[2]);
                    }
                    else if (arr.Length >= 3 && arr[1] == "bgm_021.bcstm")
                    {
                        NWB.SelectedIndex = Int32.Parse(arr[2]);
                    }
                }
            }
            Refresh();
            isInitialized = true;
            DataContext = viewModel;
        }
        
        public static string GetTitleIDFromRegion(int i)
        {
            if (i == 0)
            {
                return UStitleID;
            }
            if (i == 1)
            {
                return EUtitleID;
            }
            if (i == 2)
            {
                return JPtitleID;
            }
            if (i == 3)
            {
                return JPtitleIDupdate;
            }
            else
            {
                return UStitleID;
            }
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
            if (System.IO.File.Exists(Misc.Jsons.settings))
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = System.IO.File.ReadAllText(Misc.Jsons.settings);
                settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
                PathBox.Text = settings.DeployPath;
                DefPrevBox.SelectedIndex = settings.DefaultImage;
                UsingEmulator.IsChecked = settings.Emulator;
                RegionBox.SelectedIndex = settings.Region;
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
                    var jsonoptions = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    string jsonString = JsonSerializer.Serialize(mod, jsonoptions);
                    System.IO.File.WriteAllText(filepath, jsonString);
                }
                if (System.IO.File.Exists(filepath))
                {
                    var jsonoptions = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
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
                    if (Directory.Exists(Path.Combine(Misc.Paths.mods, row.ID)))
                    {
                        try
                        {
                            Misc.CopyDirectory(Path.Combine(Misc.Paths.mods, row.ID), Path.Combine(Misc.Paths.temp, row.ID, row.Name), true);

                            var jsonoptions = new JsonSerializerOptions
                            {
                                WriteIndented = true
                            };
                            string jsonString = JsonSerializer.Serialize(row, jsonoptions);
                            string filepath = Path.Combine(Misc.Paths.temp, row.ID, row.Name, "meta.json");
                            System.IO.File.WriteAllText(filepath, jsonString);

                            var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                            {
                                Title = "Select Mod Archive",
                                FileTypeChoices = new List<FilePickerFileType>
                                {
                                    new FilePickerFileType("Mod Archive")
                                    {
                                        Patterns = new List<string> { "*.zip" }
                                    }
                                }
                            });

                            if (file == null)
                            {
                                Console.WriteLine("Save file operation canceled.");
                                return;
                            }
                            using var archive = SharpCompress.Archives.Zip.ZipArchive.Create();
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
                        Directory.Delete(Path.Combine(Misc.Paths.mods, row.ID), true);
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
                string modpath = "";
                foreach (string path in CountFolders(Misc.Paths.mods))
                {
                    Meta mod = new Meta();
                    string filepath = Path.Combine(path, "meta.json");
                    if (!System.IO.File.Exists(filepath))
                    {
                        continue;
                    }
                    if (System.IO.File.Exists(filepath))
                    {
                        var jsonoptions = new JsonSerializerOptions
                        {
                            WriteIndented = true
                        };
                        string jsonString = System.IO.File.ReadAllText(filepath);
                        mod = JsonSerializer.Deserialize<Meta>(jsonString, jsonoptions);
                        if (mod.ID == row.ID)
                        {
                            modpath = path;
                        }
                    }
                }
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
            ModsImage.Source = new Bitmap(AssetLoader.Open(new Uri("avares://Nightmare Editor AUI/Images/ModsSel.png", UriKind.RelativeOrAbsolute)));
            SettingsImage.Source = new Bitmap(AssetLoader.Open(new Uri("avares://Nightmare Editor AUI/Images/SettingsUnsel.png", UriKind.RelativeOrAbsolute)));
            MusicImage.Source = new Bitmap(AssetLoader.Open(new Uri("avares://Nightmare Editor AUI/Images/MusicUnsel.png", UriKind.RelativeOrAbsolute)));
            ModsWindow(true);
            MusicWindow.IsVisible = false;
            SettingsWindow.IsVisible = false;
        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            ModsImage.Source = new Bitmap(AssetLoader.Open(new Uri("avares://Nightmare Editor AUI/Images/ModsUnsel.png", UriKind.RelativeOrAbsolute)));
            SettingsImage.Source = new Bitmap(AssetLoader.Open(new Uri("avares://Nightmare Editor AUI/Images/SettingsSel.png", UriKind.RelativeOrAbsolute)));
            MusicImage.Source = new Bitmap(AssetLoader.Open(new Uri("avares://Nightmare Editor AUI/Images/MusicUnsel.png", UriKind.RelativeOrAbsolute)));
            ModsWindow(false);
            MusicWindow.IsVisible = false;
            SettingsWindow.IsVisible = true;
        }
        private void Music_Click(object sender, RoutedEventArgs e)
        {
            ModsImage.Source = new Bitmap(AssetLoader.Open(new Uri("avares://Nightmare Editor AUI/Images/ModsUnsel.png", UriKind.RelativeOrAbsolute)));
            SettingsImage.Source = new Bitmap(AssetLoader.Open(new Uri("avares://Nightmare Editor AUI/Images/SettingsUnsel.png", UriKind.RelativeOrAbsolute)));
            MusicImage.Source = new Bitmap(AssetLoader.Open(new Uri("avares://Nightmare Editor AUI/Images/MusicSel.png", UriKind.RelativeOrAbsolute)));
            ModsWindow(false);
            MusicWindow.IsVisible = true;
            SettingsWindow.IsVisible = false;
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = "https://gamebanana.com/games/17208",
                    UseShellExecute = true
                });
            }
            catch { }
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private async void Deploy_Click2(string deploypath)
        {
            string jsonString = System.IO.File.ReadAllText(Misc.Jsons.settings);
            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            Settings settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
            if (settings.Emulator)
            {
                Directory.CreateDirectory(Path.Combine(deploypath, "mods", GetTitleIDFromRegion(settings.Region), "romfs"));
                Directory.Delete(Path.Combine(deploypath, "mods", GetTitleIDFromRegion(settings.Region), "romfs"), true);
                Directory.CreateDirectory(Path.Combine(deploypath, "mods", GetTitleIDFromRegion(settings.Region), "romfs"));
                Directory.CreateDirectory(Path.Combine(deploypath, "textures", GetTitleIDFromRegion(settings.Region), "NightmareEditor"));
                Directory.Delete(Path.Combine(deploypath, "textures", GetTitleIDFromRegion(settings.Region), "NightmareEditor"), true);
                Directory.CreateDirectory(Path.Combine(deploypath, "textures", GetTitleIDFromRegion(settings.Region), "NightmareEditor"));
            }
            else
            {
                Directory.CreateDirectory(deploypath);
                Directory.Delete(deploypath, true);
                Directory.CreateDirectory(deploypath);
            }
            List<string> rbins = new List<string>();
            string[] folders = Directory.GetDirectories(Misc.Paths.mods);
            List<string> modFolders = new List<string>();
            foreach (string folder in folders)
            {
                try
                {
                    jsonoptions = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    jsonString = System.IO.File.ReadAllText(Path.Combine(folder, "meta.json"));
                    Meta meta = JsonSerializer.Deserialize<Meta>(jsonString, jsonoptions);
                    if (enabledmods.Contains(meta.ID))
                    {
                        modFolders.Add(folder);
                        string[] subdirectories = Directory.GetDirectories(folder);
                        foreach (string subdir in subdirectories)
                        {
                            DirectoryInfo dir = new DirectoryInfo(subdir);
                            string rbin = dir.Name;
                            if (rbin == "~emulator-textures" && settings.Emulator)
                            {
                                Directory.CreateDirectory(Path.Combine(deploypath, "textures",
                                    Path.GetFileName(folder)));
                                Editor.BetterDirCopy(Path.Combine(folder, "~emulator-textures"),
                                    Path.Combine(deploypath, "textures", GetTitleIDFromRegion(settings.Region),
                                        "NightmareEditor", Path.GetFileName(folder)), false);
                            }
                            else if (!rbins.Contains(rbin) && Misc.accepted_rbins.Contains(rbin))
                            {
                                rbins.Add(rbin);
                            }
                            else if (Misc.accepted_folders.Contains(rbin))
                            {
                                Directory.CreateDirectory(Path.Combine(deploypath, rbin));
                                Editor.BetterDirCopy(subdir, Path.Combine(deploypath, rbin), false);
                            }
                        }
                    }
                }
                catch
                {

                }
            }
            bool stop = false;
            foreach (string rbin in rbins)
            {
                if (!File.Exists(Path.Combine(Misc.Paths.current, $"{rbin}.rbin")) || !Directory.Exists(Path.Combine(Misc.Paths.basePath, rbin)))
                {
                    stop = true;

                    var box = MessageBoxManager.GetMessageBoxStandard(
                        $"Missing {rbin}",
                        $@"Missing {rbin}.rbin. Unpack it using the unpack button in the settings tab.",
                        MsBox.Avalonia.Enums.ButtonEnum.Ok,
                        MsBox.Avalonia.Enums.Icon.Info
                    );
                    await box.ShowAsPopupAsync(this);
                }
            }
            if (stop)
            {
                return;
            }

            Directory.CreateDirectory(Misc.Paths.pack);
            Directory.Delete(Misc.Paths.pack, true);
            Directory.CreateDirectory(Misc.Paths.pack);
            foreach (string folder in modFolders)
            {
                Editor.BetterDirCopy(folder, Misc.Paths.pack, false);
            }
            foreach (string rbin in rbins)
            {
                string file = rbin + ".rbin";
                Editor.BetterDirCopy(Path.Combine(Misc.Paths.basePath, rbin), Path.Combine(Misc.Paths.pack, rbin), false, false);
                RBIN.Pack(Path.Combine(Misc.Paths.pack, file), true, this);
            }
            string musicpath = Path.Combine(deploypath, "sound", "en", "output", "stream");
            if (settings.Emulator)
            {
                musicpath = Path.Combine(deploypath, "mods", Manager.GetTitleIDFromRegion(settings.Region), "romfs", "sound", "en", "output", "stream");
            }
            Directory.CreateDirectory(musicpath);
            foreach (string[] track in music)
            {
                File.Copy(Path.Combine(Misc.Paths.program, track[0]), Path.Combine(musicpath, track[1]));
            }
            var box3 = MessageBoxManager.GetMessageBoxStandard(
                $"Get ready for a fun adventure!",
                $@"Succesfully deployed mods to {deploypath}!",
                MsBox.Avalonia.Enums.ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Info
            );
            await box3.ShowAsPopupAsync(this);
        }

        private async void Deploy_Click(object sender, RoutedEventArgs e)
        {
            string x = "";
            try
            {
                string jsonString = System.IO.File.ReadAllText(Misc.Jsons.settings);
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                Settings settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
                x = settings.DeployPath;
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
                        Deploy_Click2(settings.DeployPath);
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
            var files = await this.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Mod Deploy Path",
                AllowMultiple = false
            });
            if (files.Count == 1)
            {
                if (!string.IsNullOrWhiteSpace(files[0].Path.LocalPath))
                {
                    PathBox.Text = files[0].Path.LocalPath;
                }
            }
        }

        private void PathBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isInitialized) return;
            Settings settings = new Settings();
            settings.DeployPath = PathBox.Text;
            settings.DefaultImage = DefPrevBox.SelectedIndex;
            settings.Region = RegionBox.SelectedIndex;
            if (UsingEmulator.IsChecked == true)
            {
                settings.Emulator = true;
            }
            else
            {
                settings.Emulator = false;
            }
            string jsonString = System.IO.File.ReadAllText(Misc.Jsons.settings);
            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            jsonString = JsonSerializer.Serialize(settings, jsonoptions);
            System.IO.File.WriteAllText(Misc.Jsons.settings, jsonString);
            settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
            Refresh();
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(Misc.Paths.mods))
            {
                ProcessStartInfo StartInformation = new ProcessStartInfo();
                StartInformation.FileName = Misc.Paths.mods;
                StartInformation.UseShellExecute = true;
                Process process = Process.Start(StartInformation);
            }
        }

        private async void InstallArchive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select Mod Archive",
                    AllowMultiple = false,
                    FileTypeFilter = new List<FilePickerFileType>
                    {
                        new FilePickerFileType("Mod Archive")
                        {
                            Patterns = new List<string> { "*.zip" }
                        },
                        new FilePickerFileType("Legacy Mod Archive")
                        {
                            Patterns = new List<string> { "*.7z" }
                        }
                    }
                });
                if (files.Count == 1)
                {
                    using var archive = ArchiveFactory.Open(files[0].Path.LocalPath);
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        entry.WriteToDirectory(Misc.Paths.mods, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
            catch { }
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
                    if (!enabledmods.Contains(row.ID))
                        enabledmods.Add(row.ID);
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
                    if (enabledmods.Contains(row.ID))
                        enabledmods.Remove(row.ID);
                    QuickJson(true, enabledmods, "enabledmods.json");
                    enabledmods = QuickJson(false, enabledmods, "enabledmods.json");
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

        private void TTF_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool found = false;
            foreach (var arr in music)
            {
                if (arr.Length >= 3 && arr[1] == "bgm_001.bcstm")
                {
                    if (TTF.SelectedIndex == 0)
                    {
                        arr[0] = $@"Music\traverseintrance\traverseintrance.bcstm";
                        arr[2] = "0";
                    }
                    else if (TTF.SelectedIndex == 1)
                    {
                        arr[0] = $@"Music\traverseintrance\traversetown_sd.bcstm";
                        arr[2] = "1";
                    }
                    else if (TTF.SelectedIndex == 2)
                    {
                        arr[0] = $@"Music\traverseintrance\traversetown_hd.bcstm";
                        arr[2] = "2";
                    }
                    else if (TTF.SelectedIndex == 3)
                    {
                        arr[0] = $@"Music\traverseintrance\traversetown_com.bcstm";
                        arr[2] = "3";
                    }
                    else if (TTF.SelectedIndex == 4)
                    {
                        arr[0] = $@"Music\traverseintrance\traversetown_coded.bcstm";
                        arr[2] = "4";
                    }
                    else if (TTF.SelectedIndex == 5)
                    {
                        arr[0] = $@"Music\traverseintrance\traversetown_recoded.bcstm";
                        arr[2] = "5";
                    }
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                if (TTF.SelectedIndex == 0)
                {
                    music.Add([$@"Music\traverseintrance\traverseintrance.bcstm", "bgm_001.bcstm", "0"]);
                }
                else if (TTF.SelectedIndex == 1)
                {
                    music.Add([$@"Music\traverseintrance\traversetown_sd.bcstm", "bgm_001.bcstm", "1"]);
                }
                else if (TTF.SelectedIndex == 2)
                {
                    music.Add([$@"Music\traverseintrance\traversetown_hd.bcstm", "bgm_001.bcstm", "2"]);
                }
                else if (TTF.SelectedIndex == 3)
                {
                    music.Add([$@"Music\traverseintrance\traversetown_com.bcstm", "bgm_001.bcstm", "3"]);
                }
                else if (TTF.SelectedIndex == 4)
                {
                    music.Add([$@"Music\traverseintrance\traversetown_coded.bcstm", "bgm_001.bcstm", "4"]);
                }
                else if (TTF.SelectedIndex == 5)
                {
                    music.Add([$@"Music\traverseintrance\traversetown_recoded.bcstm", "bgm_001.bcstm", "5"]);
                }
            }
            QuickMusicJson(true);
        }

        private void TTB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool found = false;
            foreach (var arr in music)
            {
                if (arr.Length >= 3 && arr[1] == "bgm_014.bcstm")
                {
                    if (TTB.SelectedIndex == 0)
                    {
                        arr[0] = $@"Music\handtohand\handtohand.bcstm";
                        arr[2] = "0";
                    }
                    else if (TTB.SelectedIndex == 1)
                    {
                        arr[0] = $@"Music\handtohand\handinhand_sd.bcstm";
                        arr[2] = "1";
                    }
                    else if (TTB.SelectedIndex == 2)
                    {
                        arr[0] = $@"Music\handtohand\handinhand_hd.bcstm";
                        arr[2] = "2";
                    }
                    else if (TTB.SelectedIndex == 3)
                    {
                        arr[0] = $@"Music\handtohand\handinhand_com.bcstm";
                        arr[2] = "3";
                    }
                    else if (TTB.SelectedIndex == 4)
                    {
                        arr[0] = $@"Music\handtohand\nightoffate_coded.bcstm";
                        arr[2] = "4";
                    }
                    else if (TTB.SelectedIndex == 5)
                    {
                        arr[0] = $@"Music\handtohand\nightoffate_recoded.bcstm";
                        arr[2] = "5";
                    }
                    else if (TTB.SelectedIndex == 6)
                    {
                        arr[0] = $@"Music\handtohand\nightoffate_coded.bcstm";
                        arr[2] = "6";
                    }
                    else if (TTB.SelectedIndex == 7)
                    {
                        arr[0] = $@"Music\handtohand\nightoffate_recoded.bcstm";
                        arr[2] = "7";
                    }
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                if (TTB.SelectedIndex == 0)
                {
                    music.Add([$@"Music\handtohand\handinhand.bcstm", "bgm_014.bcstm", "0"]);
                }
                else if (TTB.SelectedIndex == 1)
                {
                    music.Add([$@"Music\handtohand\handinhand_sd.bcstm", "bgm_014.bcstm", "1"]);
                }
                else if (TTB.SelectedIndex == 2)
                {
                    music.Add([$@"Music\handtohand\handinhand_hd.bcstm", "bgm_014.bcstm", "2"]);
                }
                else if (TTB.SelectedIndex == 3)
                {
                    music.Add([$@"Music\handtohand\handinhand_com.bcstm", "bgm_014.bcstm", "3"]);
                }
                else if (TTB.SelectedIndex == 4)
                {
                    music.Add([$@"Music\handtohand\nightoffate_sd.bcstm", "bgm_014.bcstm", "4"]);
                }
                else if (TTB.SelectedIndex == 5)
                {
                    music.Add([$@"Music\handtohand\nightoffate_hd.bcstm", "bgm_014.bcstm", "5"]);
                }
                else if (TTB.SelectedIndex == 6)
                {
                    music.Add([$@"Music\handtohand\nightoffate_coded.bcstm", "bgm_014.bcstm", "6"]);
                }
                else if (TTB.SelectedIndex == 7)
                {
                    music.Add([$@"Music\handtohand\nightoffate_recoded.bcstm", "bgm_014.bcstm", "7"]);
                }
            }
            QuickMusicJson(true);
        }

        private void TGF_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool found = false;
            foreach (var arr in music)
            {
                if (arr.Length >= 3 && arr[1] == "bgm_011.bcstm")
                {
                    if (TGF.SelectedIndex == 0)
                    {
                        arr[0] = $@"Music\accessthegrid\accessthegrid.bcstm";
                        arr[2] = "0";
                    }
                    else if (TGF.SelectedIndex == 1)
                    {
                        arr[0] = $@"Music\accessthegrid\spaceparanoids_sd.bcstm";
                        arr[2] = "1";
                    }
                    else if (TGF.SelectedIndex == 2)
                    {
                        arr[0] = $@"Music\accessthegrid\spaceparanoids_hd.bcstm";
                        arr[2] = "2";
                    }
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                if (TGF.SelectedIndex == 0)
                {
                    music.Add([$@"Music\accessthegrid\accessthegrid.bcstm", "bgm_011.bcstm", "0"]);
                }
                else if (TGF.SelectedIndex == 1)
                {
                    music.Add([$@"Music\accessthegrid\spaceparanoids_sd.bcstm", "bgm_011.bcstm", "1"]);
                }
                else if (TGF.SelectedIndex == 2)
                {
                    music.Add([$@"Music\accessthegrid\spaceparanoids_hd.bcstm", "bgm_011.bcstm", "2"]);
                }
            }
            QuickMusicJson(true);
        }

        private void TGB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool found = false;
            foreach (var arr in music)
            {
                if (arr.Length >= 3 && arr[1] == "bgm_020.bcstm")
                {
                    if (TGB.SelectedIndex == 0)
                    {
                        arr[0] = $@"Music\digitaldomination\digitaldomination.bcstm";
                        arr[2] = "0";
                    }
                    else if (TGB.SelectedIndex == 1)
                    {
                        arr[0] = $@"Music\digitaldomination\bytebashing_sd.bcstm";
                        arr[2] = "1";
                    }
                    else if (TGB.SelectedIndex == 2)
                    {
                        arr[0] = $@"Music\digitaldomination\bytebashing_hd.bcstm";
                        arr[2] = "2";
                    }
                    else if (TGB.SelectedIndex == 3)
                    {
                        arr[0] = $@"Music\digitaldomination\bytestriking.bcstm";
                        arr[2] = "3";
                    }
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                if (TGB.SelectedIndex == 0)
                {
                    music.Add([$@"Music\digitaldomination\digitaldomination.bcstm", "bgm_020.bcstm", "0"]);
                }
                else if (TGB.SelectedIndex == 1)
                {
                    music.Add([$@"Music\digitaldomination\bytebashing_sd.bcstm", "bgm_020.bcstm", "1"]);
                }
                else if (TGB.SelectedIndex == 2)
                {
                    music.Add([$@"Music\digitaldomination\bytebashing_hd.bcstm", "bgm_020.bcstm", "2"]);
                }
                else if (TGB.SelectedIndex == 3)
                {
                    music.Add([$@"Music\digitaldomination\bytestriking.bcstm", "bgm_020.bcstm", "3"]);
                }
            }
            QuickMusicJson(true);
        }

        private void NWF_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool found = false;
            foreach (var arr in music)
            {
                if (arr.Length >= 3 && arr[1] == "bgm_012.bcstm")
                {
                    if (NWF.SelectedIndex == 0)
                    {
                        arr[0] = $@"Music\sacreddistance\sacreddistance.bcstm";
                        arr[2] = "0";
                    }
                    else if (NWF.SelectedIndex == 1)
                    {
                        arr[0] = $@"Music\sacreddistance\sacredmoon_sd.bcstm";
                        arr[2] = "1";
                    }
                    else if (NWF.SelectedIndex == 2)
                    {
                        arr[0] = $@"Music\sacreddistance\sacredmoon_hd.bcstm";
                        arr[2] = "2";
                    }
                    else if (NWF.SelectedIndex == 3)
                    {
                        arr[0] = $@"Music\sacreddistance\sacredmoon_days_sd.bcstm";
                        arr[2] = "3";
                    }
                    else if (NWF.SelectedIndex == 4)
                    {
                        arr[0] = $@"Music\sacreddistance\sacredmoon_days_hd.bcstm";
                        arr[2] = "4";
                    }
                    else if (NWF.SelectedIndex == 5)
                    {
                        arr[0] = $@"Music\sacreddistance\mysticmoon_sd.bcstm";
                        arr[2] = "5";
                    }
                    else if (NWF.SelectedIndex == 6)
                    {
                        arr[0] = $@"Music\sacreddistance\mysticmoon_hd.bcstm";
                        arr[2] = "6";
                    }
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                if (NWF.SelectedIndex == 0)
                {
                    music.Add([$@"Music\sacreddistance\sacreddistance.bcstm", "bgm_012.bcstm", "0"]);
                }
                else if (NWF.SelectedIndex == 1)
                {
                    music.Add([$@"Music\sacreddistance\sacredmoon_sd.bcstm", "bgm_012.bcstm", "1"]);
                }
                else if (NWF.SelectedIndex == 2)
                {
                    music.Add([$@"Music\sacreddistance\sacredmoon_hd.bcstm", "bgm_012.bcstm", "2"]);
                }
                else if (NWF.SelectedIndex == 3)
                {
                    music.Add([$@"Music\sacreddistance\sacredmoon_days_sd.bcstm", "bgm_012.bcstm", "3"]);
                }
                else if (NWF.SelectedIndex == 4)
                {
                    music.Add([$@"Music\sacreddistance\sacredmoon_days_hd.bcstm", "bgm_012.bcstm", "4"]);
                }
                else if (NWF.SelectedIndex == 5)
                {
                    music.Add([$@"Music\sacreddistance\mysticmoon_sd.bcstm", "bgm_012.bcstm", "5"]);
                }
                else if (NWF.SelectedIndex == 6)
                {
                    music.Add([$@"Music\sacreddistance\mysticmoon_hd.bcstm", "bgm_012.bcstm", "6"]);
                }
            }
            QuickMusicJson(true);
        }

        private void NWB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool found = false;
            foreach (var arr in music)
            {
                if (arr.Length >= 3 && arr[1] == "bgm_021.bcstm")
                {
                    if (NWB.SelectedIndex == 0)
                    {
                        arr[0] = $@"Music\deepdrop\deepdrop.bcstm";
                        arr[2] = "0";
                    }
                    else if (NWB.SelectedIndex == 1)
                    {
                        arr[0] = $@"Music\deepdrop\deepdrive_sd.bcstm";
                        arr[2] = "1";
                    }
                    else if (NWB.SelectedIndex == 2)
                    {
                        arr[0] = $@"Music\deepdrop\deepdrive_hd.bcstm";
                        arr[2] = "2";
                    }
                    else if (NWB.SelectedIndex == 3)
                    {
                        arr[0] = $@"Music\deepdrop\criticaldrive_sd.bcstm";
                        arr[2] = "3";
                    }
                    else if (NWB.SelectedIndex == 4)
                    {
                        arr[0] = $@"Music\deepdrop\criticaldrive_hd.bcstm";
                        arr[2] = "4";
                    }
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                if (NWB.SelectedIndex == 0)
                {
                    music.Add([$@"Music\deepdrop\deepdrop.bcstm", "bgm_021.bcstm", "0"]);
                }
                else if (NWB.SelectedIndex == 1)
                {
                    music.Add([$@"Music\deepdrop\deepdrive_sd.bcstm", "bgm_021.bcstm", "1"]);
                }
                else if (NWB.SelectedIndex == 2)
                {
                    music.Add([$@"Music\deepdrop\deepdrive_hd.bcstm", "bgm_021.bcstm", "2"]);
                }
                else if (NWB.SelectedIndex == 3)
                {
                    music.Add([$@"Music\deepdrop\criticaldrive_sd.bcstm", "bgm_021.bcstm", "3"]);
                }
                else if (NWB.SelectedIndex == 4)
                {
                    music.Add([$@"Music\deepdrop\criticaldrive_hd.bcstm", "bgm_021.bcstm", "4"]);
                }
            }
            QuickMusicJson(true);
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
    }
}

