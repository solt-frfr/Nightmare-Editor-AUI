using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LibGit2Sharp;
using NightmareLibrary;
using static NightmareEditor.Managers.Standard;

namespace NightmareEditor;

public partial class MesgWindow : Window
{
    public enum MsgBoxType
    {
        Info = 0,
        YesNo = 1,
        Git = 2,
        GitUpdate = 3,
    }

    private MsgBoxType _msgBoxType = MsgBoxType.Info;
    
    public NightmareLibrary.Misc.ErrorCode Result { get; private set; }
    
    public MesgWindow() // Designer Only. Give it paramaters.
    {
        InitializeComponent();
    }

    public MesgWindow(string title, string content, MsgBoxType msgBoxType, double scale = 1)
    {
        InitializeComponent();
        Width *= scale;
        Height *= scale;
        _msgBoxType = msgBoxType;
        TitleText.Text = title;
        Content.Text = content;
        if (msgBoxType == MsgBoxType.Info)
        {
            AcceptButton.Text = "OK";
            AcceptButton.IsVisible = true;
            DeclineButton.IsVisible = false;
            ExtraButton.IsVisible = false;
            Title = "Exam Editor - INFORMATION";
        }
        else if (msgBoxType == MsgBoxType.YesNo)
        {
            AcceptButton.Text = "Yes";
            DeclineButton.Text = "No";
            AcceptButton.IsVisible = true;
            DeclineButton.IsVisible = true;
            ExtraButton.IsVisible = false;
            Title = "Exam Editor - WARNING";
        }
        else if (msgBoxType == MsgBoxType.Git)
        {
            AcceptButton.Text = "Install";
            DeclineButton.Text = "Done";
            AcceptButton.IsVisible = true;
            DeclineButton.IsVisible = true;
            ExtraButton.IsVisible = false;
            Title = "Exam Editor - Install a Git Repository";
            GitPanel.IsVisible = true;
        }
        else if (msgBoxType == MsgBoxType.GitUpdate)
        {
            AcceptButton.Text = "Yes";
            DeclineButton.Text = "No";
            AcceptButton.IsVisible = true;
            DeclineButton.IsVisible = true;
            ExtraButton.IsVisible = false;
            Title = "Exam Editor - Update Git Repositories";
            GitPanel.IsVisible = false;
            GitBox.IsVisible = false;
        }
    }
    
    private bool OnTransferProgress(TransferProgress progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            InternalProgressBar.IsIndeterminate = false;
            InternalProgressBar.Minimum = 0;
            InternalProgressBar.Value = progress.ReceivedObjects;
            InternalProgressBar.Maximum = progress.TotalObjects; 
        });
        return true;
    }

    private async Task UpdateMods()
    {
        string[] folders = Directory.GetDirectories(Paths.Folders.mods);
        List<string> updating = new List<string>();
        List<string> updated = new List<string>();
        List<string> skipped = new List<string>();
        List<string> validFolders = new List<string>();
        foreach (string folder in folders)
        {
            if (File.Exists(Path.Combine(folder, "meta.json")))
            {
                string jsonString = System.IO.File.ReadAllText(Path.Combine(folder, "meta.json"));
                Meta meta = JsonSerializer.Deserialize<Meta>(jsonString, WriteIndented);
                updating.Add(meta.Name);
                validFolders.Add(folder);
            }
        }
        var pullOptions = new PullOptions
        {
            FetchOptions = new FetchOptions
            {
                OnTransferProgress = OnTransferProgress
            }
        };
        string contentUpdateText = "";
        foreach (string folder in validFolders)
        {
            string jsonString = System.IO.File.ReadAllText(Path.Combine(folder, "meta.json"));
            Meta meta = JsonSerializer.Deserialize<Meta>(jsonString, WriteIndented);
            contentUpdateText = "Updating mods:";
            foreach (var mod in updating)
            {
                contentUpdateText += $"\n  - {mod}";
            }
            contentUpdateText += "\nUpdated mods:";
            foreach (var mod in updated)
            {
                contentUpdateText += $"\n  - {mod}";
            }
            contentUpdateText += "\nSkipped mods:";
            foreach (var mod in skipped)
            {
                contentUpdateText += $"\n  - {mod}";
            }

            Content.Text = contentUpdateText;
            try
            {
                InternalKH3DText.Text = $"Updating {meta.Name}...";
                await Dispatcher.UIThread.InvokeAsync(() => { });
                await Task.Run(() =>
                {
                    using var repo = new Repository(folder);
                    Commands.Pull(
                        repo,
                        GitSignature,
                        pullOptions
                    );
                    updated.Add(meta.Name);
                    updating.Remove(meta.Name);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.Message.Contains("conflicts prevent checkout"))
                {
                    MesgWindow mw2 = new MesgWindow("WARNING", meta.Name + "\n" + ex.Message + "\nWould you like to update? This will delete any local changes to the mod.", MesgWindow.MsgBoxType.YesNo);

                    await mw2.ShowDialog(this);
                    
                    if (mw2.Result == Misc.ErrorCode.Success)
                    {
                        try
                        {
                            await Task.Run(() =>
                            {
                                using (var repo = new Repository(folder))
                                {
                                    foreach (var item in repo.RetrieveStatus())
                                    {
                                        if (item.State == FileStatus.NewInWorkdir)
                                        {
                                            var path = Path.Combine(repo.Info.WorkingDirectory, item.FilePath);
                                            if (File.Exists(path))
                                                File.Delete(path);
                                        }
                                    }
                            
                                    Commands.Pull(
                                        repo,
                                        GitSignature,
                                        new PullOptions()
                                    );
                                }
                                updated.Add(meta.Name);
                                updating.Remove(meta.Name);
                            });
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                            skipped.Add(meta.Name);
                            updating.Remove(meta.Name);
                        }
                    }
                }
                else
                {
                    skipped.Add(meta.Name);
                    updating.Remove(meta.Name);
                }
            }
        }
        contentUpdateText = "Updated mods:";
        foreach (var mod in updated)
        {
            contentUpdateText += $"\n  - {mod}";
        }
        contentUpdateText += "\nSkipped mods:";
        foreach (var mod in skipped)
        {
            contentUpdateText += $"\n  - {mod}";
        }

        Content.Text = contentUpdateText;
        AcceptButton.IsVisible = false;
        DeclineButton.Text = "OK";
        InternalKH3DText.Text = "Done.";
    }

    private async void AcceptButton_OnClick(object? sender, EventArgs e)
    {
        if (_msgBoxType == MsgBoxType.Info)
        {
            Close();
        }
        else if (_msgBoxType == MsgBoxType.YesNo)
        {
            Result = NightmareLibrary.Misc.ErrorCode.Success;
            Close();
        }
        else if (_msgBoxType == MsgBoxType.Git)
        {
            string[] splitbysource = GitBox.Text.Split('@');
            string[] splitbyslash = splitbysource[0].Split('/');
            if (splitbyslash.Length < 2)
            {
                InternalKH3DText.Text = "Invalid Repository";
                return;
            }
            string source = splitbysource.Length > 1 ? splitbysource[1] : "github.com";
            ButtonPanel.IsEnabled = false;
            ButtonPanel.Opacity = 0.5;
            InternalKH3DText.Text = $"Cloning {splitbyslash[0]}/{splitbyslash[1]} at {source}...";
            if (Directory.Exists(
                    Path.Combine(Paths.Folders.mods, splitbysource[0].Replace('/', '.').Replace('\\', '.'))))
            {
                InternalKH3DText.Text = "Folder already exists. Delete it if you want to re-install.";
            }
            else
            {
                try
                {
                    InternalProgressBar.IsIndeterminate = true;
                    await Dispatcher.UIThread.InvokeAsync(
                        () => { }, DispatcherPriority.Background);
                    var cloneOptions = new CloneOptions
                    {
                        FetchOptions = 
                        {
                            OnTransferProgress = OnTransferProgress
                        }
                    };
                    await Task.Run(() => Repository.Clone($"https://{source}/{splitbyslash[0]}/{splitbyslash[1]}.git", 
                        Path.Combine(Paths.Folders.mods, splitbysource[0].Replace('/', '.').Replace('\\', '.')), cloneOptions));
                    InternalKH3DText.Text = $"Cloned {splitbyslash[0]}/{splitbyslash[1]} at {source}";
                }
                catch (Exception exception)
                {
                    InternalKH3DText.Text = exception.Message;
                    Console.WriteLine(exception.Message);
                }
            }
            ButtonPanel.IsEnabled = true;
            ButtonPanel.Opacity = 1;
        }
        else if (_msgBoxType == MsgBoxType.GitUpdate)
        {
            ButtonPanel.Opacity = 0.5;
            ButtonPanel.IsEnabled = false;
            GitPanel.IsVisible = true;
            await UpdateMods();
            ButtonPanel.IsEnabled = true;
            GitPanel.IsVisible = false;
            ButtonPanel.Opacity = 1;
        }
    }

    private void DeclineButton_OnClick(object? sender, EventArgs e)
    {
        if (_msgBoxType == MsgBoxType.YesNo || _msgBoxType == MsgBoxType.Git 
                                            || _msgBoxType == MsgBoxType.GitUpdate)
        {
            Result = NightmareLibrary.Misc.ErrorCode.Cancelled;
            Close();
        }
    }

    private void ExtraButton_OnClick(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
}