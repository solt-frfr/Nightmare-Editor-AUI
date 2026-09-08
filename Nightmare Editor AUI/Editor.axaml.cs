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
using Avalonia.Threading;
using Nightmare_Editor_AUI;
using Nightmare_Editor_AUI.Controls;


namespace Nightmare_Editor
{
    /// <summary>
    /// Interaction logic for Editor.axaml
    /// </summary>
    public partial class Editor : Window
    {
        // "Game Archive files (*.rbin)|*.rbin|Texture files(*.ctt)|*.ctt|Layout 2D files(*.l2d)|*.l2d|Effect Files(*.fep)|*.fep|Model files(*.pmo)|*.pmo|Map files(*.pmp)|*.pmp|All files (*.*)|*.*";
        private EditorFile selectedRbin;
        private EditorFile selectedFile;
        private EditorFile selectedEmbeddedFile;
        private EditorFile requestedContextMenu;

        private List<string> flaggedFiles = new List<string>();
        private List<string> flaggedFiles2 = new List<string>();
        private List<string> flaggedFiles3 = new List<string>();

        private bool windowSwap = false;
        private bool windowStore = false;

        private bool textureSwap = false;
        private string displayedTextureName;

        private List<string> allfiles = new List<string>();

        private List<string[]> textureLinks = new List<string[]>();
        
        private List<EditorFile> unfiltered = new List<EditorFile>();
        private List<EditorFile> filtered = new List<EditorFile>();

        private int runningOperations = 0;

        private enum Column
        {
            Rbin = 1,
            File = 2,
            EmbeddedFile = 3
        }

        public bool rexIsVisible = true;
        public bool replaceIsVisible = true;
        public bool removeIsVisible = true;
        public bool remove2IsVisible = true;
        public bool flag2IsVisible = true;
        public bool flag3IsVisible = true;
        
        private string GetSelectedFilePath(Column target, bool useContextMenuRequester = false)
        {
            string returnValue = Path.GetFileNameWithoutExtension(selectedRbin.Text);
            string file2 = useContextMenuRequester && target == Column.File ? (requestedContextMenu ??= new EditorFile()).Name : (selectedFile ??= new EditorFile()).Name;
            string folder2 = Path.GetFileNameWithoutExtension(file2);
            string file3 = useContextMenuRequester && target == Column.EmbeddedFile ? (requestedContextMenu ??= new EditorFile()).Text : (selectedEmbeddedFile ??= new EditorFile()).Text;
            if (string.IsNullOrWhiteSpace(file2)) file2 = "notapplicable";
            if (string.IsNullOrWhiteSpace(folder2)) folder2 = "notapplicable";
            if (string.IsNullOrWhiteSpace(file3)) file3 = "notapplicable";
            switch (target)
            {
                case Column.Rbin:
                    returnValue = useContextMenuRequester ? requestedContextMenu.Text : selectedRbin.Text;
                    break;
                case Column.File:
                    returnValue = Path.Combine(returnValue, file2);
                    break;
                case Column.EmbeddedFile:
                    returnValue = Path.Combine(returnValue, folder2, file3);
                    break;
                default:
                    break;
            }
            return returnValue;
        }

        private string GetSelecedFilePathFolder(Column target)
        {
            string returnValue = Path.GetFileNameWithoutExtension(selectedRbin.Text);
            string file2 = (selectedFile ??= new EditorFile()).Name;
            string folder2 = Path.GetFileNameWithoutExtension(file2);
            string file3 = (selectedEmbeddedFile ??= new EditorFile()).Text;
            if (string.IsNullOrWhiteSpace(file2)) file2 = "notapplicable";
            if (string.IsNullOrWhiteSpace(folder2)) folder2 = "notapplicable";
            if (string.IsNullOrWhiteSpace(file3)) file3 = "notapplicable";
            switch (target)
            {
                case Column.Rbin:
                    break;
                case Column.File:
                    break;
                case Column.EmbeddedFile:
                    returnValue = Path.Combine(returnValue, folder2);
                    break;
                default:
                    break;
            }
            return returnValue;
        }


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
            List<EditorFile> tempList = new List<EditorFile>();
            foreach (string file in files)
            {
                string filetrim = file.Replace(Misc.Paths.current + Path.DirectorySeparatorChar, "");
                tempList = AddFile(tempList, filetrim, Column.Rbin, filetrim.Contains("User-Added.rbin"));
            }
            tempList = tempList.OrderBy(ef => ef.Name).ToList();
            foreach (var editorFile in tempList)
            {
                Files.Children.Add(editorFile);
            }
            if (!File.Exists(Misc.Jsons.textures))
            {
                QuickJson(true);
            }

            Closing += OnClosing;

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
                File.WriteAllText(Misc.Jsons.textures, jsonString);
            }
            else
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = File.ReadAllText(Misc.Jsons.textures);
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
                            File.Copy(files[0].Path.LocalPath,
                                Path.Combine(Misc.Paths.current, Path.GetFileName(files[0].Path.LocalPath)));
                            
                            EditorFile newFile = MakeEditorFile(Path.GetFileName(files[0].Path.LocalPath), Column.Rbin, false);
                            Files.Children.Add(newFile);

                            await ExtractRbinWithUI(newFile, files[0].Path.LocalPath);
                        }
                        catch
                        {
                            Log.Text =
                                "You attempted to open a file that already exists. Use \"Replace\" if this was your intention.";
                        }
                    }
                    else if (Path.GetExtension(files[0].Path.LocalPath) == ".ctt")
                    {
                        string[] files2 = Directory.GetFiles(Path.Combine(Misc.Paths.work, "User-Added"), "*.*",
                            SearchOption.AllDirectories);
                        File.Copy(files[0].Path.LocalPath,
                            Path.Combine(Misc.Paths.work, "User-Added",
                                $"{files2.Length}-{Path.GetFileName(files[0].Path.LocalPath)}"), true);
                        CTT.Decode(Path.Combine(Misc.Paths.work, "User-Added",
                            $"{files2.Length}-{Path.GetFileName(files[0].Path.LocalPath)}"));
                    }
                    else if (Containers.IsArc(files[0].Path.LocalPath))
                    {
                        string[] files2 = Directory.GetFiles(Path.Combine(Misc.Paths.work, "User-Added"), "*.*",
                            SearchOption.AllDirectories);
                        File.Copy(files[0].Path.LocalPath,
                            Path.Combine(Misc.Paths.work, "User-Added",
                                $"{files2.Length}-{Path.GetFileName(files[0].Path.LocalPath)}"), true);
                        Containers.Generic.Unpack(Path.Combine(Misc.Paths.work, "User-Added",
                            $"{files2.Length}-{Path.GetFileName(files[0].Path.LocalPath)}"));
                    }
                    else
                    {
                        string[] files2 = Directory.GetFiles(Path.Combine(Misc.Paths.work, "User-Added"), "*.*",
                            SearchOption.AllDirectories);
                        File.Copy(files[0].Path.LocalPath,
                            Path.Combine(Misc.Paths.work, "User-Added",
                                $"{files2.Length}-{Path.GetFileName(files[0].Path.LocalPath)}"), true);
                    }

                    if (Path.GetExtension(files[0].Path.LocalPath) != ".rbin" &&
                        !File.Exists(Path.Combine(Misc.Paths.current, "User-Added.rbin")))
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

        private async Task ExtractRbinWithUI(EditorFile ef, string extractPath)
        {
            
            runningOperations++;
            ef.IsHitTestVisible = false;
            ef.ProgressBar.IsVisible = true;
            var progress = new Progress<(int current, int total, string message)>(message =>
            {
                ef.ProgressBar.Value = (double)message.current / (double)message.total;
                Log.Text = message.message;
            });
            await Task.Run(() =>
            {
                RBIN.Load(extractPath, progress: progress);
            });
            ef.IsHitTestVisible = true;
            ef.ProgressBar.IsVisible = false;
            runningOperations--;
        }

        private ContextMenu Cont1(bool isUserAdded)
        {
            var contextMenu = new ContextMenu();
            var open = new MenuItem
            {
                Header = "Open Folder",
            };
            open.Click += OpenFolder_Click;
            contextMenu.Items.Add(open);

            if (!isUserAdded)
            {
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
            }

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

        private ContextMenu Cont2(bool isUserAdded)
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

            if (isUserAdded)
            {
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
            }
            else
            {
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

            }

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

        private void ChangeSelection(EditorFile newFile, Column column)
        {
            try
            {
                switch (column)
                {
                    case Column.Rbin:
                        selectedRbin.IsSelected = false;
                        break;
                    case Column.File:
                        selectedFile.IsSelected = false;
                        break;
                    case Column.EmbeddedFile:
                        selectedEmbeddedFile.IsSelected = false;
                        break;
                    default:
                        break;
                }
            }
            catch { }
            switch (column)
            {
                case Column.Rbin:
                    selectedRbin = newFile;
                    selectedRbin.IsSelected = true;
                    break;
                case Column.File:
                    selectedFile = newFile;
                    selectedFile.IsSelected = true;
                    break;
                case Column.EmbeddedFile:
                    selectedEmbeddedFile = newFile;
                    selectedEmbeddedFile.IsSelected = true;
                    break;
                default:
                    break;
            }
        }

        private EditorFile MakeEditorFile(string filename, Column column, bool isUserAdded)
        {
            List<EditorFile> tempList = new List<EditorFile>();
            tempList = AddFile(tempList, filename, column, isUserAdded);
            return tempList[0];
        }
        private List<EditorFile> AddFile(List<EditorFile> list, string filename, Column column, bool isUserAdded)
        {
            if (filename == "User-Added.rbin") isUserAdded = true;
            EditorFile newFile = new EditorFile
            {
                Name = filename,
                Text = filename,
                IsUserAdded = isUserAdded,
            };

            if (column == Column.Rbin)
            {
                var contextMenu = Cont1(isUserAdded);
                newFile.ContextMenu = contextMenu;
                newFile.Click += EditorFile_Rbin_Click;
            }

            if (column == Column.File)
            {
                newFile.Name = filename;
                newFile.Text = Misc.RemoveAtFirst(filename, '-');

                var contextMenu = Cont2(isUserAdded);
                newFile.ContextMenu = contextMenu;
                newFile.Click += EditorFile_File_Click;
            }

            if (column == Column.EmbeddedFile)
            {
                var contextMenu = Cont3();
                newFile.ContextMenu = contextMenu;
                newFile.Click += EditorFile_EmbeddedFile_Click;
            }

            newFile.AddHandler(ContextRequestedEvent, EditorFile_ContextRequested, RoutingStrategies.Tunnel);

            list.Add(newFile);

            return list;
        }
        
        private void EditorFile_ContextRequested(object? sender, ContextRequestedEventArgs e)
        {
            if (sender is EditorFile ef)
            {
                try
                {
                    requestedContextMenu.RequestedContext = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                requestedContextMenu = ef;
                requestedContextMenu.RequestedContext = true;
            }
        }

        private async void EditorFile_Rbin_Click(object? sender, EventArgs e)
        {
            File_Click(sender, Column.Rbin);
        }
        private async void EditorFile_File_Click(object? sender, EventArgs e)
        {
            File_Click(sender, Column.File);
        }
        private async void EditorFile_EmbeddedFile_Click(object? sender, EventArgs e)
        {
            File_Click(sender, Column.EmbeddedFile);
        }
        
        
        private async void File_Click(object? sender, Column source)
        {
            sender ??= new object();
            if (sender is EditorFile tempef) ChangeSelection(tempef, source);
            Column destination = (Column)((int)source + 1);
            var sourceColumn = source switch
            {
                Column.Rbin => Files.Children,
                Column.File => Files2.Children,
                Column.EmbeddedFile => Files3.Children,
                _ => Files.Children
            };
            var destinationColumn = destination switch
            {
                Column.Rbin => Files.Children,
                Column.File => Files2.Children,
                Column.EmbeddedFile => Files3.Children,
                _ => Files2.Children
            };
            InfoWindow.IsVisible = source == Column.File || source == Column.EmbeddedFile;
            
            if (Directory.Exists(Path.Combine(Misc.Paths.work, GetSelecedFilePathFolder(destination))) && Enum.IsDefined(destination))
            {
                await ListFiles(sender, destination, sourceColumn, destinationColumn);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (destination == Column.EmbeddedFile && Files3.Children.Count > 0)
                    {
                        EditorFile_EmbeddedFile_Click(Files3.Children[0], null);
                    }
                });
            }
            else
            {
                AttemptDisplay(source);
            }
        }

        private async Task ListFiles(object sender, Column destination, Controls sourceColumn, Controls destinationColumn)
        {
            try
            {
                switch (destination)
                {
                    case Column.Rbin:
                        Files_Scroll.ScrollToHome();
                        break;
                    case Column.File:
                        Files2_Scroll.ScrollToHome();
                        break;
                    case Column.EmbeddedFile:
                        Files3_Scroll.ScrollToHome();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            Sort.SelectedIndex = 0;
            InfoWindow.IsVisible = false;
            HexWindow.IsVisible = false;
            List<EditorFile> tempList = new List<EditorFile>();
            bool isUserAdded = false;
            EditorFile usedFile = new EditorFile();
            var progress = new Progress<(int current, int total)>();
            foreach (var child in sourceColumn)
            {
                if (child is EditorFile ef && ef.IsSelected)
                {
                    usedFile = ef;
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ef.ChangeCursor(StandardCursorType.Wait);
                    });
                    await Dispatcher.UIThread.InvokeAsync(
                        () => { }, DispatcherPriority.Background);
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        IsHitTestVisible = false;
                    });
                    isUserAdded = ef.Text == "User-Added.rbin";
                    destinationColumn.Clear();
                    tempList.Clear();
                    Log.Text = "Loading...";
                    ef.ProgressBar.IsVisible = true;
                    progress = new Progress<(int current, int total)>(message =>
                    {
                        ef.ProgressBar.Value = (double)message.current / (double)message.total;
                    });
                    string searchDir = Path.Combine(Misc.Paths.work, GetSelecedFilePathFolder(destination));
                    string logText = $"Files in {ef.Text}";
                    if (Directory.Exists(searchDir))
                    {
                        Log.Text = logText;
                        string[] files = Directory.GetFiles(
                            searchDir, "*.*",
                            SearchOption.AllDirectories);
                        int i = 0;
                        foreach (string file in files)
                        {
                            string filetrim = file.Replace(searchDir + Path.DirectorySeparatorChar, "");
                            switch (destination)
                            {
                                case Column.Rbin:
                                    Files_Scroll.ScrollToHome();
                                    break;
                                case Column.File:
                                    if (!filetrim.Contains(Path.DirectorySeparatorChar) &&
                                        !filetrim.Contains(".bmp") &&
                                        !filetrim.Contains(".png") &&
                                        !filetrim.Contains(".txt") &&
                                        !filetrim.Contains(".json") &&
                                        !filetrim.Contains(".pnt"))
                                    {
                                        tempList = AddFile(tempList, filetrim, destination, isUserAdded);
                                    }

                                    break;
                                case Column.EmbeddedFile:
                                    if (!filetrim.Contains(Path.DirectorySeparatorChar) &&
                                        !filetrim.Contains(".bmp") &&
                                        !filetrim.Contains(".png") &&
                                        !filetrim.Contains(".txt") &&
                                        !filetrim.Contains(".json") &&
                                        !filetrim.Contains(".pnt"))
                                    {
                                        tempList = AddFile(tempList, filetrim, destination, isUserAdded);
                                    }
                                    break;
                                default:
                                    break;
                            }

                            i++;
                            if (progress is IProgress<(int current, int total)> iprogress)
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    iprogress.Report((i + 1, files.Length));
                                });
                        }
                    }
                    break;
                }
            }
            
            List<EditorFile> sorted;
            
            if (!isUserAdded && destination == Column.File)
            {
                sorted = tempList
                    .OrderBy(ef => int.Parse(ef.Name.Split('-')[0]))
                    .ToList();
            }
            else
            {
                sorted = tempList
                    .OrderBy(ef => ef.Name)
                    .ToList();
            }
            
            destinationColumn.Clear();
            tempList.Clear();
            if (destination == Column.File) unfiltered.Clear();

            int j = 0;
            foreach (var editorFile in sorted)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    destinationColumn.Add(editorFile);
                    j++;
                    if (progress is IProgress<(int current, int total)> iprogress)
                        iprogress.Report((j + 1, sorted.Count));
                }, DispatcherPriority.Background);
                if (destination == Column.File) unfiltered.Add(editorFile);
            }
            
            usedFile.ProgressBar.IsVisible = false;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                usedFile.RestoreCursor();
                IsHitTestVisible = true;
            });
        }

        private void AttemptDisplay(Column source)
        {
            string filepath = GetSelectedFilePath(source);
            EditorFile ef = source switch
            {
                Column.File => selectedFile,
                Column.EmbeddedFile => selectedEmbeddedFile,
                _ => selectedFile
            };
            string filename = ef.Name ??= "";
            if (filename.EndsWith(".ctt"))
            {
                Log.Text = $"Displaying {ef.Text}";
                AssignImage(filepath, source);
            }
            else if (filename.EndsWith(".txa"))
            {
                AnimWindow anim = new AnimWindow(Path.Combine(Misc.Paths.work, filepath));
                anim.Show();
                Log.Text = $"Opened {ef.Text} in Nightmare Animation Studio";
                InfoWindow.IsVisible = false;
            }

            Files3.IsVisible = source == Column.EmbeddedFile;
        }
        
        private async void AssignImage(string file, Column source)
        {
            string path = Path.Combine(Misc.Paths.work, file);
            displayedTextureName = Path.GetFileNameWithoutExtension(path);
            HexWindow.IsVisible = false;
            
            FileFormat.Text = ((CTT.Format)File.ReadAllBytes(path)[0x1C]).ToString();;
            MemoryStream ms = new MemoryStream();
            NewTools.CTT.Decode(path, false).SaveAsPng(ms);
            ms.Seek(0, SeekOrigin.Begin);
            Bitmap bitmap = new Bitmap(ms);
            Bitmap bitmap2;
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
                            bitmap2 = new Bitmap(File.OpenRead(arr[1]));
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
            var box = MessageBoxManager.GetMessageBoxStandard(
                $"Delete {requestedContextMenu.Text}",
                $"Do you wish to delete {requestedContextMenu.Text}?",
                ButtonEnum.YesNo,
                MsBox.Avalonia.Enums.Icon.Question
            );
            var result = await box.ShowAsPopupAsync(this);
            if (result == ButtonResult.Yes)
            {
                try
                {
                    Directory.Delete(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(requestedContextMenu.Text)), true);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
                try
                {
                    Directory.Delete(Path.Combine(Misc.Paths.basePath, Path.GetFileNameWithoutExtension(requestedContextMenu.Text)), true);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
                try
                {
                    File.Delete(Path.Combine(Misc.Paths.current, requestedContextMenu.Text));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
                try
                {
                    Files.Children.Remove(requestedContextMenu);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }
        private async void RemoveFile2(object sender, RoutedEventArgs e)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                $"Delete {requestedContextMenu.Text}",
                $"Do you wish to delete {requestedContextMenu.Text}?",
                ButtonEnum.YesNo,
                MsBox.Avalonia.Enums.Icon.Question
            );
            var result = await box.ShowAsPopupAsync(this);
            if (result == ButtonResult.Yes)
            {
                if (Directory.Exists(Path.Combine(Misc.Paths.work, "User-Added", Path.GetFileNameWithoutExtension(requestedContextMenu.Text))));
                Directory.Delete(Path.Combine(Misc.Paths.work, "User-Added", Path.GetFileNameWithoutExtension(requestedContextMenu.Text)), true);
                File.Delete(Path.Combine(Misc.Paths.work, "User-Added", requestedContextMenu.Text));
                Files2.Children.Remove(requestedContextMenu);
            }
        }

        private async void Again_Click(object sender, RoutedEventArgs e)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                $"Re-Extract {requestedContextMenu.Text}",
                $"Do wish to extract {requestedContextMenu.Text} again? This will replace all files inside.",
                ButtonEnum.YesNo,
                MsBox.Avalonia.Enums.Icon.Question
            );
            var result = await box.ShowAsPopupAsync(this);
            if (result == ButtonResult.Yes)
            {
                await ExtractRbinWithUI(requestedContextMenu, Path.Combine(Misc.Paths.current, requestedContextMenu.Text));
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Path.Combine(Misc.Paths.current, requestedContextMenu.Text)))
            {
                ProcessStartInfo StartInformation = new ProcessStartInfo();
                StartInformation.FileName = Misc.Paths.current;
                StartInformation.UseShellExecute = true;
                Process process = Process.Start(StartInformation);
            }
        }

        private void OpenFolder2_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Path.Combine(Misc.Paths.work, GetSelectedFilePath(Column.File, true))))
            {
                string file = Path.Combine(Misc.Paths.work, GetSelectedFilePath(Column.File, true));
                ProcessStartInfo StartInformation = new ProcessStartInfo();
                StartInformation.FileName = Path.Combine(Misc.Paths.work, GetSelecedFilePathFolder(Column.File));
                StartInformation.UseShellExecute = true;
                Process process = Process.Start(StartInformation);
            }
        }

        private void OpenFolder3_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Path.Combine(Misc.Paths.work, GetSelectedFilePath(Column.EmbeddedFile, true))))
            {
                string file = Path.Combine(Misc.Paths.work, GetSelectedFilePath(Column.EmbeddedFile, true));
                ProcessStartInfo StartInformation = new ProcessStartInfo();
                StartInformation.FileName = Path.Combine(Misc.Paths.work, GetSelecedFilePathFolder(Column.EmbeddedFile));
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
                    return Misc.FileFilters.all[i + 1];
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
                    File.Copy(file[0].Path.LocalPath, Path.Combine(Misc.Paths.current, requestedContextMenu.Text), true);
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

            FilePickerFileType type = MatchFilter(requestedContextMenu.Text);
            if (type != null)
            {
                filter.Add(type);
            }
            else
            {
                filter.Add(new FilePickerFileType($"{Path.GetExtension(requestedContextMenu.Text)} files")
                {
                    Patterns = new List<string> { $"*{Path.GetExtension(requestedContextMenu.Text)}" }
                });
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
                            string path = Path.Combine(Misc.Paths.work, GetSelectedFilePath(Column.File, true));
                            File.Copy(file[0].Path.LocalPath, path, true);
                            CTT.Decode(path);
                        }
                        else if (Containers.IsArc(file[0].Path.LocalPath))
                        {
                            string path = Path.Combine(Misc.Paths.work, GetSelectedFilePath(Column.File, true));
                            File.Copy(file[0].Path.LocalPath, path, true);
                            Containers.Generic.Unpack(path);
                            
                        }
                        else
                        {
                            File.Copy(file[0].Path.LocalPath, Path.Combine(Misc.Paths.work, GetSelectedFilePath(Column.File, true)), true);
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
            if (requestedContextMenu.Text.EndsWith(".ctt"))
            {
                string file2 = "";
                bool found = false;
                foreach (var arr in textureLinks)
                {
                    if (arr.Length >= 2 && arr[0] == selectedRbin.Text + Path.DirectorySeparatorChar + selectedFile.Name)
                    {
                        file2 = arr[1];
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    NewTools.CTT.Decode(Path.Combine(Misc.Paths.work, GetSelectedFilePath(Column.File, true)), true);
                    string[] files2 = Directory.GetFiles(Path.Combine(Misc.Paths.work, GetSelecedFilePathFolder(Column.File)), $"{requestedContextMenu.Name}.*.png", SearchOption.AllDirectories);
                    if (files2.Length < 1)
                    {
                        Log.Text = "Couldn't find a texture to pack.";
                        return;
                    }
                    file2 = files2[0];
                    
                }
                
                Log.Text = "Packing...";
                NewTools.CTT.Encode(Path.Combine(Misc.Paths.work, GetSelectedFilePath(Column.File, true)), file2);
                Log.Text = $"Packed {requestedContextMenu.Text}!";
            }
            
            else if (Containers.IsArc(requestedContextMenu.Text) && Directory.Exists(Path.Combine(Misc.Paths.work, Path.GetFileNameWithoutExtension(selectedRbin.Text), Path.GetFileNameWithoutExtension(requestedContextMenu.Name))))
            {
                ShowGenericWarning();
                Containers.Generic.Pack(Path.Combine(Misc.Paths.work, GetSelectedFilePath(Column.File, true)));
                Log.Text = $"Packed {requestedContextMenu.Text}!";
            }
            else
            {
                Log.Text = $"{requestedContextMenu.Text} is not an archive nor texture file, and cannot be packed.";
            }    
        }
        private async void Replace3_Click(object sender, RoutedEventArgs e)
        {
            var filter = new List<FilePickerFileType>();

            FilePickerFileType type = MatchFilter(requestedContextMenu.Text);
            if (type != null)
            {
                filter.Add(type);
            }
            else
            {
                filter.Add(new FilePickerFileType($"{Path.GetExtension(requestedContextMenu.Text)} files")
                {
                    Patterns = new List<string> { $"*{Path.GetExtension(requestedContextMenu.Text)}" }
                });
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
                            File.Copy(file[0].Path.LocalPath, Path.Combine(Misc.Paths.work, GetSelectedFilePath(Column.EmbeddedFile, true)), true);
                            File.Copy(file[0].Path.LocalPath, Path.Combine(Misc.Paths.toolkit, requestedContextMenu.Text), true);
                            CTT.Decode(Path.Combine(Misc.Paths.work, GetSelectedFilePath(Column.EmbeddedFile, true)));

                        }
                        else
                        {
                            File.Copy(file[0].Path.LocalPath, Path.Combine(Misc.Paths.work, GetSelectedFilePath(Column.EmbeddedFile, true)), true);
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
            if (requestedContextMenu.Text.EndsWith(".ctt"))
            {
                string file2 = "";
                bool found = false;
                foreach (var arr in textureLinks)
                {
                    if (arr.Length >= 2 && arr[0] == selectedRbin.Text + Path.DirectorySeparatorChar + selectedFile.Name + Path.DirectorySeparatorChar + requestedContextMenu.Text)
                    {
                        file2 = arr[1];
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    NewTools.CTT.Decode(Path.Combine(Misc.Paths.work, GetSelectedFilePath(Column.EmbeddedFile, true)), true);
                    string[] files2 = Directory.GetFiles(Path.Combine(Misc.Paths.work, GetSelecedFilePathFolder(Column.EmbeddedFile)), $"{requestedContextMenu.Text}.*.png", SearchOption.AllDirectories);
                    if (files2.Length < 1)
                    {
                        Log.Text = "Couldn't find a texture to pack.";
                        return;
                    }
                    file2 = files2[0];
                }
                Log.Text = "Packing...";
                NewTools.CTT.Encode(Path.Combine(Misc.Paths.work, GetSelectedFilePath(Column.EmbeddedFile, true)), file2);
                Log.Text = $"Packed {requestedContextMenu.Text}!";
            }
        }
        private void Flag2(object sender, RoutedEventArgs e)
        {
            string file1 = GetSelectedFilePath(Column.Rbin, false);
            string file2 = GetSelectedFilePath(Column.File, true);
            bool isin = false;
            if (!flaggedFiles.Contains(file1) && !selectedRbin.Text.Contains("User-Added.rbin"))
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
                    if (fileref.Contains(Path.GetFileNameWithoutExtension(requestedContextMenu.Name) + Path.DirectorySeparatorChar))
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
                if (fileref.Contains(Path.GetFileNameWithoutExtension(selectedRbin.Text) + Path.DirectorySeparatorChar))
                {
                    isin = true;
                    break;
                }
            }
            if (!isin)
            {
                Log.Text = $"No flagged file references {selectedRbin.Text}, removing from flagged list.";
                flaggedFiles.Remove(file1);
            }

        }
        private void Flag3(object sender, RoutedEventArgs e)
        {
            string file1 = GetSelectedFilePath(Column.Rbin, false);
            string file2 = GetSelectedFilePath(Column.File, false);
            string file3 = GetSelectedFilePath(Column.EmbeddedFile, true);
            if (!flaggedFiles.Contains(file1) && !selectedRbin.Text.Contains("User-Added.rbin"))
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
                newTextBox.Classes.Add("NoHover");
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
            if (file == null || file.Count < 1)
            {
                return;
            }
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
                    if (textbox.Text.EndsWith(filter) && (textbox.Text.Contains(Search.Text) || string.IsNullOrWhiteSpace(Search.Text)))
                    {
                        Files2.Children.Add(textbox);
                    }
                }
            }
            catch { }
        }

        private void Reverse_Rebirth(object sender, RoutedEventArgs e)
        {
            if (!(runningOperations > 0))
            {
                if (Nightmare_Editor_AUI.Managers.Standard.MainSettings.UI == 0)
                {
                    Manager mw = new Manager();
                    mw.Show();
                }
                else
                {
                    NewManager nmw = new NewManager();
                    nmw.Show();
                }
            }
            Close();
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
                    },
                    SuggestedFileName = displayedTextureName
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
        
        protected void OnClosing(object sender, WindowClosingEventArgs e)
        {
            if (runningOperations > 0)
            {
                e.Cancel = true;
                
                var box2 = MessageBoxManager.GetMessageBoxStandard(
                    "Cannot Close",
                    "There is still ongoing extraction, so the program cannot close.",
                    MsBox.Avalonia.Enums.ButtonEnum.Ok,
                    MsBox.Avalonia.Enums.Icon.Info
                );
                box2.ShowAsPopupAsync(this);
            }
        }
    }
}