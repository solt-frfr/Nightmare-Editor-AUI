using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Nightmare_Editor.NewTools
{
    public static class Misc
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
        public static class Paths
        {
            public static readonly string program = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            public static readonly string temp = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Temp");
            public static readonly string work = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Work");
            public static readonly string pack = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Pack");
            public static readonly string mods = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Mods");
            public static readonly string music = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Music");
            public static readonly string basePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Base"); // base didn't work
            public static readonly string current = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Current");
            public static readonly string toolkit = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "DDD-Toolkit");
        }

        /// <summary>
        /// Consistent paths for common jsons
        /// </summary>
        public static class Jsons
        {
            public static readonly string music = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "music.json");
            public static readonly string settings = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "settings.json");
            public static readonly string enabled = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "enabledmods.json");
            public static readonly string textures = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "textures.json");
            public static readonly string temp = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "temp.json");
        }

        public enum FileTypes
        {
            rbin = 0,
            ctt = 1,
            l2d = 2,
            fep = 3,
            pmo = 4,
            pmp = 5,
        }

        public static class FileFilters
        {
            public static readonly List<FilePickerFileType> all = new List<FilePickerFileType>()
            {
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
        }
        
        public static bool IsArc(string file)
        {
            if (Path.GetExtension(file) == ".pmo" || Path.GetExtension(file) == ".l2d" || Path.GetExtension(file) == ".fep" || Path.GetExtension(file) == ".pmp")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public static string ReplaceFirst(string str, string term, string replace)
        {
            int position = str.IndexOf(term);
            if (position < 0)
            {
                return str;
            }
            str = str.Substring(0, position) + replace + str.Substring(position + term.Length);
            return str;
        }
        
        public static string ReplaceFirst(string str, char term, char replace)
        {
            int position = str.IndexOf(term);
            if (position < 0)
            {
                return str;
            }
            str = str.Substring(0, position) + replace + str.Substring(position + 1);
            return str;
        }
        
        public static string RemoveAtFirst(string str, char term)
        {
            int position = str.IndexOf(term);
            if (position < 0)
            {
                return str;
            }
            str = str.Substring(position + 1);
            return str;
        }
    }
}
