using System.Text;
using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Nightmare_Editor.NewTools;
using System.Collections.Generic;
using Avalonia.Platform.Storage;
using Avalonia.Controls;
using System.Threading.Tasks;
using static Nightmare_Editor_AUI.Managers.Standard;

namespace Nightmare_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public static class Toolkit
    {
        public async static Task RbinPack(string filename, bool tooutput, Window parent)
        {
            string toolkitPath = Path.Combine(Misc.Paths.toolkit, "DDD Toolkit.exe");
            string inputFile = Path.Combine(Misc.Paths.toolkit, filename);
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"\"{toolkitPath}\" \"{inputFile}\"\"",
                UseShellExecute = true,
                WorkingDirectory = Misc.Paths.toolkit
            };

            Debug.WriteLine(startInfo.FileName);
            Debug.WriteLine(startInfo.Arguments);
            Process process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit();

            if (!tooutput)
            {
                var save = await parent.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
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
                if (!string.IsNullOrWhiteSpace(save.Path.LocalPath))
                {
                    File.Move(inputFile, save.Path.LocalPath, true);
                }
                else
                {
                    File.Delete(inputFile);
                }
            }
            else
            {
                Settings settings = MainSettings;

                File.Move(inputFile, settings.DeployPath + $@"\{filename}", true);
            }
            Directory.Delete(Path.Combine(Misc.Paths.toolkit, Path.GetFileNameWithoutExtension(filename)), true);
        }

        public static void RbinExtract(string filename)
        {
            string toolkitPath = Path.Combine(Misc.Paths.toolkit, "DDD Toolkit.exe");
            string inputFile = Path.Combine(Misc.Paths.toolkit, filename);
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"\"{toolkitPath}\" \"{inputFile}\"\"",
                UseShellExecute = true,
                WorkingDirectory = Misc.Paths.toolkit
            };

            Debug.WriteLine(startInfo.FileName);
            Debug.WriteLine(startInfo.Arguments);
            Process process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit();
            Directory.CreateDirectory(Misc.Paths.work);
            Directory.CreateDirectory(Misc.Paths.basePath);
            Directory.CreateDirectory(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(filename)));
            Directory.Delete(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(filename)), true);
            Directory.Move(Path.Combine(Misc.Paths.toolkit, Path.GetFileNameWithoutExtension(filename)), Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(filename)));
            List<string> Paths = new List<string>();
            string[] files = Directory.GetFiles(Misc.Paths.work, "*.*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                string filetrim = file.Replace(Misc.Paths.work + Path.DirectorySeparatorChar, "");
                if (!filetrim.Contains(".bmp") && !filetrim.Contains(".png") && (filetrim.Length - filetrim.Replace(Path.DirectorySeparatorChar.ToString(), "").Length <= 1))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(file.Replace(Misc.Paths.work + Path.DirectorySeparatorChar, Misc.Paths.basePath + Path.DirectorySeparatorChar)));
                    File.Copy(file, file.Replace(Misc.Paths.work + Path.DirectorySeparatorChar, Misc.Paths.basePath + Path.DirectorySeparatorChar), true);
                }
            }
            File.Delete(Path.Combine(Misc.Paths.toolkit, filename));
        }

        public static void CTTPack(string filename, string path, string format)
        {
            Settings settings = MainSettings;
            
            string toolkitPath = Path.Combine(Misc.Paths.toolkit, "ETC.exe");
            if (format != "ETC1" && format != "ETC1A4")
            {
                toolkitPath = Path.Combine(Misc.Paths.toolkit, "DDD Toolkit.exe");
            }
            string inputFile = Path.Combine(Misc.Paths.toolkit, $"{filename}.{format}.png");
            string newFile = Path.Combine(Misc.Paths.toolkit, filename + ".ctt");
            File.Copy(newFile, Path.Combine(Misc.Paths.toolkit, "og.ctt"));

            ProcessStartInfo startInfo = new ProcessStartInfo();

            if (settings.ETC1Encoder == 0)
            {
                startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C \"\"{toolkitPath}\" \"{inputFile}\"\"",
                    UseShellExecute = true,
                    WorkingDirectory = Misc.Paths.toolkit
                };
            }
            else if (settings.ETC1Encoder == 1)
            {
                startInfo = new ProcessStartInfo
                {
                    FileName = "wine",
                    Arguments = $"\"{toolkitPath}\" \"{inputFile}\"",
                    UseShellExecute = true,
                    WorkingDirectory = Misc.Paths.toolkit
                };
            }
            

            Debug.WriteLine(startInfo.FileName);
            Debug.WriteLine(startInfo.Arguments);
            Process process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit();

            File.Delete(Path.Combine(Misc.Paths.toolkit, "og.ctt"));
            File.Delete(inputFile);

            // Toolkit.CTTUnpack(filename, path);
            try
            {
                File.SetLastWriteTime(Path.Combine(path, filename + ".ctt"), DateTime.Now);
                File.SetLastWriteTime(Path.Combine(path, filename) + "." + format + ".bmp", DateTime.Now);
                File.SetLastWriteTime(Path.Combine(path, filename) + "." + format + ".png", DateTime.Now);
            }
            catch { }
        }

        public static void CTTUnpack(string filename, string path)
        {
            string toolkitPath = Path.Combine(Misc.Paths.toolkit, "ETC.exe");
            string inputFile = Path.Combine(Misc.Paths.toolkit, filename);
            byte[] buffer = new byte[0x80];
            using (FileStream fs = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                fs.Read(buffer, 0, buffer.Length);
            }
            byte formatByte = buffer[0x1C]; // read byte from file
            NewTools.CTT.Format formatenum = (NewTools.CTT.Format)formatByte;
            string format = formatenum.ToString();
            if (format != "ETC1" && format != "ETC1A4")
            {
                toolkitPath = Path.Combine(Misc.Paths.toolkit, "DDD Toolkit.exe");
            }
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"\"{toolkitPath}\" \"{inputFile}\"\"",
                UseShellExecute = true,
                WorkingDirectory = Misc.Paths.toolkit
            };

            Debug.WriteLine(startInfo.FileName);
            Debug.WriteLine(startInfo.Arguments);
            Process process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit();

            string[] files2 = Directory.GetFiles(Misc.Paths.toolkit, $"{filename}.*.png", SearchOption.AllDirectories);
            string file2 = files2[0];

            if (File.Exists(file2))
            {
                File.Move(inputFile, Path.Combine(path, Path.GetFileName(inputFile)), true);
                File.Move(file2, Path.Combine(path, Path.GetFileName(file2)), true);
                File.Move(file2.Replace(".png", ".bmp"), Path.Combine(path, Path.GetFileName(file2.Replace(".png", ".bmp"))), true);
            }
        }

        public static void ArcUnpack(string filename, string path)
        {
            string toolkitPath = Path.Combine(Misc.Paths.toolkit, "DDD Toolkit.exe");
            string inputFile = Path.Combine(Misc.Paths.toolkit, filename);
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"\"{toolkitPath}\" \"{inputFile}\"\"",
                UseShellExecute = true,
                WorkingDirectory = Misc.Paths.toolkit
            };

            Debug.WriteLine(startInfo.FileName);
            Debug.WriteLine(startInfo.Arguments);
            Process process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit();

            File.Delete(inputFile);
            if (Directory.Exists(Path.Combine(path, Path.GetFileNameWithoutExtension(filename))))
                Directory.Delete(Path.Combine(path, Path.GetFileNameWithoutExtension(filename)), true);
            if (Directory.Exists(Path.Combine(Misc.Paths.toolkit, Path.GetFileNameWithoutExtension(filename))))
                Directory.Move(Path.Combine(Misc.Paths.toolkit, Path.GetFileNameWithoutExtension(filename)), Path.Combine(path, Path.GetFileNameWithoutExtension(filename)));
        }
    }
}