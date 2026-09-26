using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using NightmareEditor;
using NightmareLibrary;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace NightmareEditor.Managers;

public static class Standard
{
    public static JsonSerializerOptions WriteIndented = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    public static readonly string EnableFile = ".ne-enabled";
    
    public static Settings MainSettings
    {
        get => GetSettings();
        set => SetSettings(value);
    }
    
    public static Settings GetSettings()
    {
        if (!File.Exists(Paths.Jsons.settings))
        {
            Settings settings = new Settings();
            settings.DeployPath = "";
            settings.DefaultImage = 0;
            settings.Region = 0;
            settings.Emulator = false;
            settings.ETC1Encoder = 2;
            settings.UI = 1;
            settings.UseMusicReplacements = false;
            string jsonString = JsonSerializer.Serialize<Settings>(settings, WriteIndented);
            System.IO.File.WriteAllText(Paths.Jsons.settings, jsonString);
        }
        return JsonSerializer.Deserialize<Settings>(File.ReadAllText(Paths.Jsons.settings), WriteIndented);
    }
    
    public static void SetSettings(Settings newSettings)
    {
        if (newSettings.UI < 0) newSettings.UI = 0;
        if (newSettings.ETC1Encoder < 0) newSettings.ETC1Encoder = 0;
        if (newSettings.Region < 0) newSettings.Region = 0;
        if (newSettings.DefaultImage < 0) newSettings.DefaultImage = 0;
        string jsonString = JsonSerializer.Serialize<Settings>(newSettings, WriteIndented);
        System.IO.File.WriteAllText(Paths.Jsons.settings, jsonString);
    }
    
    public static Themes.ThemeData CurrentTheme
    {
        get => GetTheme();
        set => SetTheme(value);
    }
    
    public static Themes.ThemeData GetTheme()
    {
        if (!File.Exists(Paths.Jsons.theme))
        {
            Themes.ThemeData theme = new Themes.ThemeData
            {
                Theme = Themes.Defaults.FirstOrDefault(x => x.Name == "Default"),
                MenuTheme = Themes.MenuDefaults.FirstOrDefault(x => x.Name == "Default")
            };
            string jsonString = JsonSerializer.Serialize<Themes.ThemeData>(theme, WriteIndented);
            System.IO.File.WriteAllText(Paths.Jsons.theme, jsonString);
        }
        return JsonSerializer.Deserialize<Themes.ThemeData>(File.ReadAllText(Paths.Jsons.theme), WriteIndented);
    }
    
    public static void SetTheme(Themes.ThemeData newTheme)
    {
        string jsonString = JsonSerializer.Serialize<Themes.ThemeData>(newTheme, WriteIndented);
        System.IO.File.WriteAllText(Paths.Jsons.theme, jsonString);
    }
    
    
    public static List<MusicEntry[]> MusicReplacements
    {
        get => GetMusicReplacements();
        set => SetMusicReplacements(value);
    }
    
    public static List<MusicEntry[]> GetMusicReplacements()
    {
        if (!File.Exists(Paths.Jsons.music))
        {
            MusicList ml = new MusicList();
            ml.Music = new List<MusicEntry[]>();
            string jsonString = JsonSerializer.Serialize<MusicList>(ml, WriteIndented);
            System.IO.File.WriteAllText(Paths.Jsons.music, jsonString);
        }

        List<MusicEntry[]> returnValue =
            JsonSerializer.Deserialize<MusicList>(File.ReadAllText(Paths.Jsons.music), WriteIndented).Music;
        if (returnValue is null) returnValue = new List<MusicEntry[]>();
        SetMusicReplacements(returnValue);
        return returnValue;
    }
    
    public static void SetMusicReplacements(List<MusicEntry[]> newMusic)
    {
        MusicList ml = new MusicList { Music = newMusic };
        string jsonString = JsonSerializer.Serialize<MusicList>(ml, WriteIndented);
        System.IO.File.WriteAllText(Paths.Jsons.music, jsonString);
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

    public static void OpenModsFolder()
    {
        if (Directory.Exists(Paths.Folders.mods))
        {
            ProcessStartInfo StartInformation = new ProcessStartInfo();
            StartInformation.FileName = Paths.Folders.mods;
            StartInformation.UseShellExecute = true;
            Process process = Process.Start(StartInformation);
        }
    }

    public static async Task InstallArchive(Window sender)
    {
        try
        {
            var files = await sender.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Mod Archive",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Nightmare Editor Mod")
                    {
                        Patterns = new List<string> { "*.nem" }
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
                    entry.WriteToDirectory(Paths.Folders.mods, new ExtractionOptions()
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
    
    public static (Misc.ErrorCode errorCode, string errorMessage) DeployMods(string deploypath)
    {
        Settings settings = MainSettings;

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
        List<string> modFolders = new List<string>();

        foreach (string modFolder in Directory.GetDirectories(Paths.Folders.mods))
        {
            string folder = modFolder;
            Meta meta = JsonSerializer.Deserialize<Meta>(File.ReadAllText(Path.Combine(folder, "meta.json")), WriteIndented);
            if (!File.Exists(Path.Combine(modFolder, EnableFile))) continue;
            if (!string.IsNullOrWhiteSpace(meta.Prefix))
            {
                folder = Path.Combine(folder, meta.Prefix);
            }
            modFolders.Add(folder);
            string[] subdirectories = Directory.GetDirectories(folder);
            foreach (string subdir in subdirectories)
            {
                DirectoryInfo dir = new DirectoryInfo(subdir);
                string rbin = dir.Name;
                if (rbin == "~emulator-textures" && settings.Emulator)
                {
                    Directory.CreateDirectory(Path.Combine(deploypath, "textures",
                        GetTitleIDFromRegion(settings.Region), "NightmareEditor"));
                    Editor.BetterDirCopy(Path.Combine(folder, "~emulator-textures"),
                        Path.Combine(deploypath, "textures", GetTitleIDFromRegion(settings.Region),
                            "NightmareEditor", modFolder), false);
                }
                else if (!rbins.Contains(rbin) && Paths.accepted_rbins.Contains(rbin))
                {
                    rbins.Add(rbin);
                }
                else if (Paths.accepted_folders.Contains(rbin))
                {
                    Directory.CreateDirectory(Path.Combine(deploypath, rbin));
                    if (settings.Emulator)
                    {
                        Editor.BetterDirCopy(subdir, Path.Combine(deploypath, "mods", GetTitleIDFromRegion(settings.Region), "romfs", rbin), false);
                    }
                    else
                    {
                        Editor.BetterDirCopy(subdir, Path.Combine(deploypath, rbin), false);
                    }
                }
            }
        }
        
        bool stop = false;
        string failed_rbins = "";
        foreach (string rbin in rbins)
        {
            if (!File.Exists(Path.Combine(Paths.Folders.current, $"{rbin}.rbin")) || !Directory.Exists(Path.Combine(Paths.Folders.basePath, rbin)))
            {
                stop = true;
                if (string.IsNullOrWhiteSpace(failed_rbins))
                {
                    failed_rbins = rbin + ".rbin";
                }
                else
                {
                    failed_rbins += ", " + rbin + ".rbin";
                }
            }
        }
        if (stop)
        {
            return (Misc.ErrorCode.MissingRbin, failed_rbins);
        }

        Directory.CreateDirectory(Paths.Folders.pack);
        Directory.Delete(Paths.Folders.pack, true);
        Directory.CreateDirectory(Paths.Folders.pack);
        foreach (string folder in modFolders)
        {
            Editor.BetterDirCopy(folder, Paths.Folders.pack, false);
        }
        foreach (string rbin in rbins)
        {
            string file = rbin + ".rbin";
            Editor.BetterDirCopy(Path.Combine(Paths.Folders.basePath, rbin), Path.Combine(Paths.Folders.pack, rbin), false, false);
            byte[] rbinData = RBIN.Pack(Path.Combine(Paths.Folders.pack, file));
            if (settings.Emulator)
            {
                Directory.CreateDirectory(Path.Combine(settings.DeployPath, "mods", GetTitleIDFromRegion(settings.Region), "romfs"));
                File.WriteAllBytes(Path.Combine(settings.DeployPath, "mods", GetTitleIDFromRegion(settings.Region), "romfs", file), rbinData);
            }
            else
            {
                File.WriteAllBytes(Path.Combine(settings.DeployPath, file), rbinData);
            }
            
        }

        if (settings.UseMusicReplacements)
        {
            string musicpath = Path.Combine(deploypath, "sound", "en", "output", "stream");
            if (settings.Emulator)
            {
                musicpath = Path.Combine(deploypath, "mods", GetTitleIDFromRegion(settings.Region), "romfs", "sound", "en", "output", "stream");
            }
            Directory.CreateDirectory(musicpath);
            string jsonString = File.ReadAllText(Paths.Jsons.music);
            List<MusicEntry[]> music = JsonSerializer.Deserialize<MusicList>(jsonString, WriteIndented).Music;
            foreach (MusicEntry[] track in music)
            {
                if (track[1].IsInternalFile)
                {
                    MemoryStream ms = new MemoryStream();
                    AssetLoader.Open(new Uri($"avares://{App.AssemblyName}/Music/{track[1].Filename}", UriKind.RelativeOrAbsolute)).CopyTo(ms);
                    byte[] bcstm = ms.ToArray();
                    File.WriteAllBytes(Path.Combine(musicpath, track[0].Filename), bcstm);
                }
                else
                {
                    File.Copy(track[1].Filename, Path.Combine(musicpath, track[0].Filename));
                }
            }
        }
        return (Misc.ErrorCode.Success, "");
    }
    
    public static async Task ZipMod(Meta meta, Window sender)
    {
        string modPath = Path.Combine(Paths.Folders.mods, meta.Folder);
        if (Directory.Exists(modPath))
        {
            try
            {
                Misc.CopyDirectory(modPath, Path.Combine(Paths.Folders.temp, meta.Folder, meta.Name), true);

                var file = await sender.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
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
                archive.AddAllFromDirectory(Path.Combine(Paths.Folders.temp, meta.Folder));
                archive.SaveTo(file.Path.LocalPath, SharpCompress.Common.CompressionType.Deflate);
            }
            catch { }
        }
    }
}