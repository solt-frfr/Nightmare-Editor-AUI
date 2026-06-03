using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using Avalonia.VisualTree;
using LibGit2Sharp;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Nightmare_Editor;
using Nightmare_Editor_AUI;
using Nightmare_Editor_AUI.Managers;
using static Nightmare_Editor_AUI.Managers.Standard;
using Nightmare_Editor_AUI.Controls;
using Nightmare_Editor.NewTools;
using SharpCompress.Archives;
using Path = SixLabors.ImageSharp.Drawing.Path;

namespace Nightmare_Editor_AUI;

public partial class NewManager : Window
{
    public NewManager()
    {
        InitializeComponent();
        Directory.CreateDirectory(Misc.Paths.mods);
        Refresh();
    }
    
    private void Menu_Mods_OnClick(object? sender, EventArgs e)
    {
        MenuButtonsPanel.IsVisible = false;
        ModsWindow.IsVisible = true;
        MainText.Text = "Installed";
        BottomLeftTextLower.Text = "Return";
        BottomLeftTextLower2.Text = "Back";
    }
    
    private void MenuButton_Hover(object? sender, PointerEventArgs e)
    {
        if (sender is Nightmare_Editor_AUI.Controls.MenuButton mb)
            BottomRightText.Text = mb.Description;
        if (sender is Nightmare_Editor_AUI.Controls.ConfigSlot cs)
            BottomRightText.Text = cs.Description;
        if (sender is Nightmare_Editor_AUI.Controls.ImageButton ib)
            BottomRightText.Text = ib.Description;
        if (sender is Nightmare_Editor_AUI.Controls.ModSlot ms)
        {
            BottomRightText.Text = ms.ModMeta.Description;
            AuthorKH3DText.Text = ms.ModMeta.Authors;
            IDKH3DText.Text = ms.ModMeta.ID;
            string modpath = GetModFolder(ms.ModMeta.ID);

            try
            {
                Color tempColor = Avalonia.Media.Color.Parse("#a80000");
                if (Avalonia.Media.Color.TryParse(ms.ModMeta.Color, out var color))
                {
                    tempColor = color;
                }
                HsvColor tempHSVColor = tempColor.ToHsv();
                HsvColor borderLightColor = new HsvColor
                (
                    tempHSVColor.A,
                    tempHSVColor.H,
                    Math.Clamp(tempHSVColor.S * (318d / 1000), 0, 1),
                    Math.Clamp(tempHSVColor.V * (875d / 659), 0, 1)
                );
                HsvColor borderDarkColor = new HsvColor
                (
                    tempHSVColor.A,
                    tempHSVColor.H,
                    Math.Clamp(tempHSVColor.S, 0, 1),
                    Math.Clamp(tempHSVColor.V * (184d / 659), 0, 1)
                );
                Color darkColor = new Color(tempColor.A, (byte)Math.Clamp(tempColor.R - 40, 0, tempColor.R), (byte)Math.Clamp(tempColor.G - 40, 0, tempColor.G), (byte)Math.Clamp(tempColor.B - 40, 0, tempColor.B));
                ModDisplay_Text.Text = ms.ModMeta.Name;
                ModDisplay_MainColor.Background = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new GradientStop(tempColor, 0),
                        new GradientStop(darkColor, 1),
                    }
                };
                ModDisplay_AccentColor.Background = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new GradientStop(borderLightColor.ToRgb(), 0),
                        new GradientStop(darkColor, 0.5),
                        new GradientStop(borderDarkColor.ToRgb(), 1),
                    }
                };
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            
            if (System.IO.File.Exists(System.IO.Path.Combine(modpath, "preview.webp")))
            {
                string imagePath = System.IO.Path.Combine(modpath, "preview.webp");

                if (File.Exists(imagePath))
                {
                    using var stream = File.OpenRead(imagePath);
                    ModPreview.Source = new Bitmap(stream);
                }
            }
            else if (System.IO.File.Exists(System.IO.Path.Combine(modpath, "preview.png")))
            {
                string imagePath = System.IO.Path.Combine(modpath, "preview.png");

                if (File.Exists(imagePath))
                {
                    using var stream = File.OpenRead(imagePath);
                    ModPreview.Source = new Bitmap(stream);
                }
            }
            else if (System.IO.File.Exists(System.IO.Path.Combine(modpath, "preview.jpg")))
            {
                string imagePath = System.IO.Path.Combine(modpath, "preview.jpg");

                if (File.Exists(imagePath))
                {
                    using var stream = File.OpenRead(imagePath);
                    ModPreview.Source = new Bitmap(stream);
                }
            }
            else
            {
                ModPreview.Source = null;
            }
        }
    }
    
    private void Menu_Settings_OnClick(object? sender, EventArgs e)
    {
        MenuButtonsPanel.IsVisible = false;
        SettingsWindow.IsVisible = true;
        MainText.Text = "Settings";
        BottomLeftTextLower.Text = "Return";
        BottomLeftTextLower2.Text = "Back";
    }
    
    private void Menu_About_OnClick(object? sender, EventArgs e)
    {
        MesgWindow mw = new MesgWindow("ABOUT EXAM EDITOR v1.0.0", "Exam Editor is a mod manager made by Solt11 specifically for the 3DS version of Kingdom Hearts Dream Drop Distance.\nPlease use OpenKH for the PC version, any mods I make will likely have an equivalent PC version.\n\nNightmare Editor is the real program, and I go more in-depth on my explanations about what and why I made this in the FAQ section of Nightmare Editor's Help Window.\n\nQ: AUI?\nA: Avalonia UI. This is a port from the WPF version and has become the only supported version.", MesgWindow.MsgBoxType.Info);
        mw.Show(this);
    }

    private void Menu_NE_OnClick(object? sender, EventArgs e)
    {
        var ew = new Editor();
        ew.Show();
        Close();
    }
    
    private void Menu_Download_OnClick(object? sender, EventArgs e)
    {
        OpenGamebanana();
    }

    private void DeployPathConfig_OnClick(object? sender, EventArgs e)
    {
        SetModDeployPath(this);
        Refresh();
    }

    private void Refresh()
    {
        DeployPathConfig.Description = "After clicking Deploy, the mods will be placed in \" " + MainSettings.DeployPath + " \".";
        
        EmulatorConfig.RightText = "No";
        if (MainSettings.Emulator)
        {
            EmulatorConfig.RightText = "Yes";
        }
        switch (MainSettings.Region)
        {
            case 0:
                RegionConfig.RightText = "North America";
                break;
            case 1:
                RegionConfig.RightText = "Europe";
                break;
            case 2:
                RegionConfig.RightText = "Japan";
                break;
            default:
                RegionConfig.RightText = "Unknown";
                break;
        }
        switch (MainSettings.ETC1Encoder)
        {
            case 0:
                ETCConfig.RightText = "Windows";
                break;
            case 1:
                ETCConfig.RightText = "Wine";
                break;
            case 2:
                ETCConfig.RightText = "Compatibility";
                break;
            default:
                ETCConfig.RightText = "Unknown";
                break;
        }
        
        ModsPanel.Children.Clear();
        string[] griditems = Directory.GetDirectories(Misc.Paths.mods);
        foreach (string modpath in griditems)
        {
            Meta mod = new Meta();
            string filepath = System.IO.Path.Combine(modpath, "meta.json");
            if (!System.IO.File.Exists(filepath))
            {
                string genid = modpath.Replace(Misc.Paths.mods, "");
                mod.Name = mod.ID = genid = genid.TrimStart(System.IO.Path.DirectorySeparatorChar);
                mod.Description = mod.Authors = "";
                string jsonString = JsonSerializer.Serialize(mod, WriteIndented);
                System.IO.File.WriteAllText(filepath, jsonString);
            }
            if (System.IO.File.Exists(filepath))
            {
                string jsonString = System.IO.File.ReadAllText(filepath);
                mod = JsonSerializer.Deserialize<Meta>(jsonString, WriteIndented);
                if (EnabledMods.Contains(mod.ID))
                    mod.IsChecked = true;
                else
                    mod.IsChecked = false;
            }
            var slot = new ModSlot
            {
                ModMeta = mod,
                Font = ModSlot.FontChoices.SmallAccurate,
                Margin = new Thickness(-1)
            };
            var context = ModContext(slot);
            slot.ContextMenu = context; 
            slot.PointerEntered += MenuButton_Hover;
            slot.Click += ModSlot_OnClick;
            ModsPanel.Children.Add(slot);
        }

        BottomLeftTextUpper2.Text = griditems.Length.ToString();
    }

    private ContextMenu ModContext(ModSlot slot)
    {
        var contextMenu = new ContextMenu();
        var open = new MenuItem
        {
            Header = "Open Folder",
            CommandParameter = slot,
        };
        open.Click += OpenFolder_Click;
        contextMenu.Items.Add(open);
        var edit = new MenuItem
        {
            Header = "Edit Metadata",
            CommandParameter = slot,
        };
        edit.Click += Edit_Click;
        contextMenu.Items.Add(edit);
        var zip = new MenuItem
        {
            Header = "Zip Mod",
            CommandParameter = slot,
        };
        zip.Click += Zip_Click;
        contextMenu.Items.Add(zip);
        var delete = new MenuItem
        {
            Header = "Delete Mod",
            CommandParameter = slot,
        };
        delete.Click += Delete_Click;
        contextMenu.Items.Add(delete);
        return contextMenu;
    }

    private void OpenFolder_Click(object? sender, EventArgs e)
    {
        if (sender is MenuItem mi &&
            mi.CommandParameter is ModSlot ms)
        {
            string folder = GetModFolder(ms.ModMeta.ID);
            try
            {
                if (Directory.Exists(folder))
                {
                    ProcessStartInfo StartInformation = new ProcessStartInfo();
                    StartInformation.FileName = folder;
                    StartInformation.UseShellExecute = true;
                    Process process = Process.Start(StartInformation);
                }
            }
            catch
            {
            }
            Refresh();
        }
    }

    private void Edit_Click(object? sender, EventArgs e)
    {
        if (sender is MenuItem mi &&
            mi.CommandParameter is ModSlot ms)
        {
            MakePack edit = new MakePack(ms.ModMeta);
            try
            {
                edit.ShowDialog(this);
            }
            catch
            {
            }
            Refresh();
        }
    }

    private async void Zip_Click(object? sender, EventArgs e)
    {
        if (sender is MenuItem mi &&
            mi.CommandParameter is ModSlot ms)
        {
            if (Directory.Exists(GetModFolder(ms.ModMeta.ID)))
            {
                try
                {
                    Misc.CopyDirectory(GetModFolder(ms.ModMeta.ID), System.IO.Path.Combine(Misc.Paths.temp, ms.ModMeta.ID, ms.ModMeta.Name), true);

                    var jsonoptions = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    string jsonString = JsonSerializer.Serialize(ms.ModMeta, jsonoptions);
                    string filepath = System.IO.Path.Combine(Misc.Paths.temp, ms.ModMeta.ID, ms.ModMeta.Name, "meta.json");
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
                    archive.AddAllFromDirectory(System.IO.Path.Combine(Misc.Paths.temp, ms.ModMeta.ID));
                    archive.SaveTo(file.Path.LocalPath, SharpCompress.Common.CompressionType.Deflate);
                }
                catch
                {
                }
            }

            Refresh();
        }
    }
    
    private async void Delete_Click(object? sender, EventArgs e)
    {
        if (sender is MenuItem mi &&
            mi.CommandParameter is ModSlot ms)
        {
            MesgWindow mw = new MesgWindow("WARNING", "Are you sure you want to delete " + ms.ModMeta.Name + "?", MesgWindow.MsgBoxType.YesNo);

            await mw.ShowDialog(this);

            if (mw.Result == ErrorCode.Success)
            {
                Directory.Delete(GetModFolder(ms.ModMeta.ID), true);
            }

            Refresh();
        }
    }

    private void Window_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (SettingsWindow.IsVisible || ModsWindow.IsVisible)
            {
                MenuButtonsPanel.IsVisible = true;
                SettingsWindow.IsVisible = false;
                ModsWindow.IsVisible = false;
                MainText.Text = "Exam Editor";
                BottomLeftTextLower.Text = "Made By";
                BottomLeftTextLower2.Text = "Solt11";
            }
        }
    }

    private void SwitchUIConfig_OnClick(object? sender, EventArgs e)
    {
        Settings settings = MainSettings;
        settings.UI = 0;
        SetSettings(settings);
        var mw = new Manager();
        mw.Show();
        Close();
    }

    private void EmulatorConfig_OnClick(object? sender, EventArgs e)
    {
        Settings settings = MainSettings;
        settings.Emulator = !settings.Emulator;
        SetSettings(settings);
        Refresh();
    }
    
    private void RegionConfig_OnClick(object? sender, EventArgs e)
    {
        Settings settings = MainSettings;
        settings.Region = (settings.Region + 1) % 3;
        SetSettings(settings);
        Refresh();
    }
    
    private void ETCConfig_OnClick(object? sender, EventArgs e)
    {
        Settings settings = MainSettings;
        settings.ETC1Encoder = (settings.ETC1Encoder + 1) % 3;
        SetSettings(settings);
        Refresh();
    }

    private async void UnpackRBINConfig_OnClick(object? sender, EventArgs e)
    {
        Directory.CreateDirectory(Misc.Paths.current);
        IStorageFolder? startFolder = null;
        if (Directory.Exists(MainSettings.DeployPath))
        {
            startFolder = await StorageProvider.TryGetFolderFromPathAsync(
                MainSettings.DeployPath
            );
        }
        
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select an RBIN to unpack",
            AllowMultiple = false,
            SuggestedStartLocation = startFolder,
            FileTypeFilter = Misc.FileFilters.rbin
        });
        if (files == null || files.Count == 0)
        {
            return;
        }
        if (!string.IsNullOrWhiteSpace(files[0].Path.LocalPath))
        {
            if (System.IO.Path.GetExtension(files[0].Path.LocalPath) == ".rbin")
            {
                try
                {
                    File.Copy(files[0].Path.LocalPath, System.IO.Path.Combine(Misc.Paths.current, System.IO.Path.GetFileName(files[0].Path.LocalPath)));
                }
                catch
                {
                    MesgWindow mw = new MesgWindow("INFORMATION", "You attempted to unpack an already unpacked file.", MesgWindow.MsgBoxType.Info);

                    await mw.ShowDialog(this);
                }
                RBIN.Load(files[0].Path.LocalPath);
            }
            else
            {
                string[] files2 = Directory.GetFiles(System.IO.Path.Combine(Misc.Paths.work, "User-Added"), "*.*", SearchOption.AllDirectories);
                File.Copy(files[0].Path.LocalPath, System.IO.Path.Combine(Misc.Paths.work, "User-Added", $"{files2.Length}-{System.IO.Path.GetFileName(files[0].Path.LocalPath)}"), true);
            }
        }
    }

    private void Mod_BG_Stack_AttatchedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is StackPanel sp)
        {
            sp.Children.Clear();
            for (int i = 0; i < 8; i++)
            {
                Grid grid = new Grid();
                grid.Clip = new PathGeometry
                {
                    Figures = PathFigures.Parse("M 136,0 A 2,2 0 0 1 138,2 L 138,12 A 2,2 0 0 1 136,14 L 2,14 A 2,2 0 0 1 0,12 L 0,2 A 2,2 0 0 1 2,0 Z")
                };
                grid.Width = 138;
                grid.Height = 14;
                grid.Margin = new Thickness(1);
                grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(13)));
                grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                Grid grid2 = new Grid();
                Grid.SetColumn(grid2, 0);
                grid2.Background = new SolidColorBrush(Color.Parse("#424242"));
                grid2.Children.Add(new Rectangle
                {
                    VerticalAlignment  = VerticalAlignment.Top,
                    Height = 1,
                    Fill = new SolidColorBrush(Avalonia.Media.Color.Parse("#434343"))
                });
                Grid grid3 = new Grid();
                Grid.SetColumn(grid3, 1);
                grid3.Background = new SolidColorBrush(Color.Parse("#393939"));
                grid3.Children.Add(new Rectangle
                {
                    VerticalAlignment  = VerticalAlignment.Top,
                    Height = 1,
                    Fill = new SolidColorBrush(Avalonia.Media.Color.Parse("#353535"))
                });
                grid3.Children.Add(new Rectangle
                {
                    VerticalAlignment  = VerticalAlignment.Bottom,
                    Height = 1,
                    Fill = new SolidColorBrush(Avalonia.Media.Color.Parse("#353535"))
                });
                grid.Children.Add(grid2);
                grid.Children.Add(grid3);
                sp.Children.Add(grid);
            }
        }
    }

    private void ModSlot_OnClick(object? sender, EventArgs e)
    {
        if (sender is ModSlot ms)
        {
            var em = EnabledMods;
            if (em.Contains(ms.ModMeta.ID))
            {
                em.Remove(ms.ModMeta.ID);
                ms.EquipE.IsVisible = false;
            }
            else
            {
                em.Add(ms.ModMeta.ID);
                ms.EquipE.IsVisible = true;
            }
            SetEnabledMods(em);
        }
    }

    private async void Deploy_OnClick(object? sender, EventArgs e)
    {
        try
        {
            Settings settings = MainSettings;
            
            if (!string.IsNullOrWhiteSpace(settings.DeployPath) && settings != null)
            {
                string text = $@"This will delete all files inside {settings.DeployPath}. Is this okay?";
                if (settings.Emulator)
                {
                    text = $"This will delete all files inside the following directories:\n\n{System.IO.Path.Combine(settings.DeployPath, "mods", GetTitleIDFromRegion(settings.Region), "romfs")}\n\n{System.IO.Path.Combine(settings.DeployPath, "textures", GetTitleIDFromRegion(settings.Region), "NightmareEditor")}\n\nIs this okay?";
                }
                
                MesgWindow mw = new MesgWindow("WARNING", text, MesgWindow.MsgBoxType.YesNo);

                await mw.ShowDialog(this);

                if (mw.Result == ErrorCode.Success)
                {
                    var returnValue = DeployMods(settings.DeployPath);
                    if (returnValue.errorCode == ErrorCode.Success)
                    {
                        MesgWindow mw2 = new MesgWindow("INFORMATION", $@"Succesfully deployed mods to {settings.DeployPath}!", MesgWindow.MsgBoxType.Info);

                        await mw2.ShowDialog(this);
                    }

                    if (returnValue.errorCode == ErrorCode.MissingRbin)
                    {
                        MesgWindow mw2 = new MesgWindow("INFORMATION", $@"Missing rbin(s) {returnValue.errorMessage}. Unpack them using the unpack button in the settings tab.", MesgWindow.MsgBoxType.Info);

                        await mw2.ShowDialog(this);
                    }
                }
            }
            else
            {
                MesgWindow mw2 = new MesgWindow("INFORMATION", "No output directory. Set one in the Settings tab.", MesgWindow.MsgBoxType.Info);

                await mw2.ShowDialog(this);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private void BottomLeftText2_OnClick(object? sender, PointerReleasedEventArgs e)
    {
        Window_OnKeyDown(sender, new KeyEventArgs
        {
            Key = Key.Escape
        });
    }

    private void Refresh_OnClick(object? sender, EventArgs e)
    {
        Refresh();
    }
    
    private void MainFolder_OnClick(object? sender, EventArgs e)
    {
        OpenModsFolder();
    }
    
    private void Install_OnClick(object? sender, EventArgs e)
    {
        InstallArchive(this);
        Refresh();
    }

    private void Git_OnClick(object? sender, EventArgs e)
    {
        MesgWindow mw1 = new MesgWindow("INFORMATION", "Cloning git repo...", MesgWindow.MsgBoxType.Info);
        mw1.Show();
        Repository.Clone("https://github.com/" + GitRepoBox.Text + ".git", System.IO.Path.Combine(Misc.Paths.mods, GitRepoBox.Text.Replace('/', '.').Replace('\\', '.')));
        MesgWindow mw2 = new MesgWindow("INFORMATION", "Done cloning.", MesgWindow.MsgBoxType.Info);
        mw2.Show();
        Refresh();
    }
    
    private async void UpdateGit_OnClick(object? sender, EventArgs e)
    {
        string[] folders = Directory.GetDirectories(Misc.Paths.mods);
        string updated = "Updated the following mods:";
        foreach (string folder in folders)
        {
            try
            {
                string jsonString = System.IO.File.ReadAllText(System.IO.Path.Combine(folder, "meta.json"));
                Meta meta = JsonSerializer.Deserialize<Meta>(jsonString, WriteIndented);
                using var repo = new Repository(folder);
                Commands.Pull(
                    repo,
                    new Signature("NightmareEditor", "nightmare@editor", DateTimeOffset.Now),
                    new PullOptions()
                );
                updated += "\n-   " + meta.Name;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.Message.Contains("conflicts prevent checkout"))
                {
                    string jsonString = System.IO.File.ReadAllText(System.IO.Path.Combine(folder, "meta.json"));
                    Meta meta = JsonSerializer.Deserialize<Meta>(jsonString, WriteIndented);
                    MesgWindow mw2 = new MesgWindow("WARNING", meta.Name + "\n" + ex.Message + "\nWould you like to update? This will delete any local changes to the mod.", MesgWindow.MsgBoxType.YesNo);

                    await mw2.ShowDialog(this);
                    
                    if (mw2.Result == ErrorCode.Success)
                    {
                        try
                        {
                            using (var repo = new Repository(folder))
                            {
                                foreach (var item in repo.RetrieveStatus())
                                {
                                    if (item.State == FileStatus.NewInWorkdir)
                                    {
                                        var path = System.IO.Path.Combine(repo.Info.WorkingDirectory, item.FilePath);
                                        if (File.Exists(path))
                                            File.Delete(path);
                                    }
                                }
                            
                                Commands.Pull(
                                repo,
                                new Signature("NightmareEditor", "nightmare@editor", DateTimeOffset.Now),
                                new PullOptions()
                                );
                            }
                                
                            updated += "\n-   " + meta.Name;
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                        }
                    }
                }
            }
        }
        MesgWindow mw = new MesgWindow("INFORMATION", updated, MesgWindow.MsgBoxType.Info);

        mw.Show();
        Refresh();
    }
}