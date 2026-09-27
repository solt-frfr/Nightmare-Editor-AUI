using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace NightmareEditor
{
    public static class Paths
    {
        public static void CombineFiles(string firstFile, string secondFile, string outputFile, int offset)
        {
            File.Delete(outputFile);
            using (var output = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            {
                using (var fs1 = new FileStream(firstFile, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[offset];
                    int bytesRead = fs1.Read(buffer, 0, offset);
                    output.Write(buffer, 0, bytesRead);
                }

                using (var fs2 = new FileStream(secondFile, FileMode.Open, FileAccess.Read))
                {
                    fs2.Seek(offset, SeekOrigin.Begin);
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = fs2.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }

        public static void CopyDirectory(string sourceDir, string destinationDir, bool overwrite = true)
        {
            if (!Directory.Exists(sourceDir))
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
            Directory.CreateDirectory(destinationDir);
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = System.IO.Path.GetFileName(file);
                string destFilePath = System.IO.Path.Combine(destinationDir, fileName);
                System.IO.File.Copy(file, destFilePath, overwrite);
            }
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string subDirName = System.IO.Path.GetFileName(subDir);
                string destSubDirPath = System.IO.Path.Combine(destinationDir, subDirName);
                CopyDirectory(subDir, destSubDirPath, overwrite);
            }
        }

        /// <summary>
        /// Consistent paths for common directories.
        /// </summary>
        public static class Folders
        {
            private static string _program { get; set; }
            public static string program
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(_program)) program = "balls";
                    return _program;
                }
                set
                {
                    try
                    {
                        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "check-write-access"), "");
                        File.Delete(Path.Combine(AppContext.BaseDirectory, "check-write-access"));
                        _program = AppContext.BaseDirectory;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine("Write access test failed, assuming flatpak...");
                        _program = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                    }
                }
            }
            public static readonly string temp = Path.Combine(program, "Temp");
            public static readonly string work = Path.Combine(program, "Work");
            public static readonly string pack = Path.Combine(program, "Pack");
            public static readonly string mods = Path.Combine(program, "Mods");
            public static readonly string basePath = Path.Combine(program, "Base");
            public static readonly string current = Path.Combine(program, "Current");
        }

        /// <summary>
        /// Consistent paths for common jsons
        /// </summary>
        public static class Jsons
        {
            public static readonly string music = Path.Combine(Folders.program, "music.json");
            public static readonly string settings = Path.Combine(Folders.program, "settings.json");
            public static readonly string textures = Path.Combine(Folders.program, "textures.json");
            public static readonly string theme = Path.Combine(Folders.program, "theme.json");
            public static readonly string queue = Path.Combine(Folders.program, "queue.json");
        }
        
        public static class FileFilters
        {
            public static readonly List<FilePickerFileType> all = new List<FilePickerFileType>()
            {
                new FilePickerFileType("All accepted files")
                {
                    Patterns = new List<string> { "*.rbin", "*.ctt", "*.l2d", "*.fep", "*.pmo", "*.pmp", "*.txa" }
                },
                new FilePickerFileType("Game Archive files")
                {
                    Patterns = new List<string> { "*.rbin" }
                },
                new FilePickerFileType("Texture Files")
                {
                    Patterns = new List<string> { "*.ctt" }
                },
                new FilePickerFileType("2D Layout files")
                {
                    Patterns = new List<string> { "*.l2d" }
                },
                new FilePickerFileType("Effect files")
                {
                    Patterns = new List<string> { "*.fep" }
                },
                new FilePickerFileType("Model files")
                {
                    Patterns = new List<string> { "*.pmo" }
                },
                new FilePickerFileType("Map files")
                {
                    Patterns = new List<string> { "*.pmp" }
                },
                new FilePickerFileType("Texture Animation files")
                {
                    Patterns = new List<string> { "*.txa" }
                },
                new FilePickerFileType("All files")
                {
                    Patterns = new List<string> { "*.*" }
                }
            };
            public static readonly List<FilePickerFileType> rbin = new List<FilePickerFileType>()
            {
                new FilePickerFileType("Game Archive files")
                {
                    Patterns = new List<string> { "*.rbin" }
                }
            };
            public static readonly List<FilePickerFileType> ctt = new List<FilePickerFileType>()
            {
                new FilePickerFileType("Texture Files")
                {
                    Patterns = new List<string> { "*.ctt" }
                }
            };
            public static readonly List<FilePickerFileType> l2d = new List<FilePickerFileType>()
            {
                new FilePickerFileType("2D Layout files")
                {
                    Patterns = new List<string> { "*.l2d" }
                }
            };
            public static readonly List<FilePickerFileType> fep = new List<FilePickerFileType>()
            {
                new FilePickerFileType("Effect files")
                {
                    Patterns = new List<string> { "*.fep" }
                }
            };
            public static readonly List<FilePickerFileType> pmo = new List<FilePickerFileType>()
            {
                new FilePickerFileType("Model files")
                {
                    Patterns = new List<string> { "*.pmo" }
                }
            };
            public static readonly List<FilePickerFileType> pmp = new List<FilePickerFileType>()
            {
                new FilePickerFileType("Map files")
                {
                    Patterns = new List<string> { "*.pmp" }
                }
            };
            public static readonly List<FilePickerFileType> txa = new List<FilePickerFileType>()
            {
                new FilePickerFileType("Texture Animation files")
                {
                    Patterns = new List<string> { "*.txa" }
                }
            };
            public static readonly List<FilePickerFileType> bcstm = new List<FilePickerFileType>()
            {
                new FilePickerFileType("Music files")
                {
                    Patterns = new List<string> { "*.bcstm" }
                }
            };
        }
        
        public static List<string> accepted_rbins = new List<string>()
        {
            "_grpdef",
            "cam",
            "chara_boss",
            "chara_d_obj",
            "chara_e_obj",
            "chara_enemy",
            "chara_f_obj",
            "chara_npc",
            "chara_pc",
            "chara_wep",
            "effect",
            "event",
            "font",
            "game",
            "item",
            "map",
            "menu",
            "message",
            "minigame",
            "mission",
            "setdata"
        };
        
        public static List<string> accepted_folders = new List<string>()
        {
            "movie",
            "sound",
            "system"
        };
        
        public static List<string> accepted_files = new List<string>()
        {
            "romarcs.prefs",
        };
    }
}
