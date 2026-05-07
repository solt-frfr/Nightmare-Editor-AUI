using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Nightmare_Editor;
using Nightmare_Editor.NewTools;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace Nightmare_Editor_AUI.Managers;

public static class Standard
{
    public static JsonSerializerOptions WriteIndented = new JsonSerializerOptions
    {
        WriteIndented = true
    };
    
    public static Settings MainSettings
    {
        get => GetSettings();
        set => SetSettings(value);
    }
    
    public static Settings GetSettings()
    {
        if (!File.Exists(Misc.Jsons.settings))
        {
            Settings settings = new Settings();
            settings.DeployPath = "";
            settings.DefaultImage = 0;
            settings.Region = 0;
            settings.Emulator = false;
            settings.ETC1Encoder = 2;
            settings.UI = 1;
            string jsonString = JsonSerializer.Serialize<Settings>(settings, WriteIndented);
            System.IO.File.WriteAllText(Misc.Jsons.settings, jsonString);
        }
        return JsonSerializer.Deserialize<Settings>(File.ReadAllText(Misc.Jsons.settings), WriteIndented);
    }
    
    public static void SetSettings(Settings newSettings)
    {
        string jsonString = JsonSerializer.Serialize<Settings>(newSettings, WriteIndented);
        System.IO.File.WriteAllText(Misc.Jsons.settings, jsonString);
    }
    
    public static readonly string UStitleID = "000400000008D300";
    public static readonly string EUtitleID = "0004000000095500";
    public static readonly string JPtitleID = "000400000004EE00";
    public static readonly string JPtitleIDupdate = "0004000E0004EE00";
    
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

    public static string GetModFolder(string ID)
    {
        string[] folders = Directory.GetDirectories(Misc.Paths.mods);
        string realFolder = "";
        foreach (string folder in folders)
        {
            try
            {
                string jsonString = System.IO.File.ReadAllText(Path.Combine(folder, "meta.json"));
                Meta meta = JsonSerializer.Deserialize<Meta>(jsonString, WriteIndented);
                if (meta.ID == ID)
                {
                    realFolder = folder;
                }
            }
            catch
            {

            }
        }
        return realFolder;
    }

    public static void OpenModsFolder()
    {
        if (Directory.Exists(Misc.Paths.mods))
        {
            ProcessStartInfo StartInformation = new ProcessStartInfo();
            StartInformation.FileName = Misc.Paths.mods;
            StartInformation.UseShellExecute = true;
            Process process = Process.Start(StartInformation);
        }
    }

    public static async void InstallArchive(Window sender)
    {
        try
        {
            var files = await sender.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
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
                using var archive = ArchiveFactory.OpenArchive(files[0].Path.LocalPath);
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
        catch
        {
        }
    }
    public static void OpenGamebanana()
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

    public async static void SetModDeployPath(Window sender)
    {
        IStorageFolder? startFolder = null;
        if (Directory.Exists(MainSettings.DeployPath))
        {
            startFolder = await sender.StorageProvider.TryGetFolderFromPathAsync(
                MainSettings.DeployPath
            );
        }
        
        var files = await sender.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Mod Deploy Path",
            AllowMultiple = false,
            SuggestedStartLocation = startFolder
        });
        if (files.Count == 1)
        {
            if (!string.IsNullOrWhiteSpace(files[0].Path.LocalPath))
            {
                var settings = MainSettings;
                settings.DeployPath = files[0].Path.LocalPath;
                SetSettings(settings);
            }
        }
    }
}