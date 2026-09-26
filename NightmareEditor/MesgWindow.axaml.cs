using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LibGit2Sharp;
using static NightmareEditor.Managers.Standard;

namespace NightmareEditor;

public partial class MesgWindow : Window
{
    public enum MsgBoxType
    {
        Info = 0,
        YesNo = 1,
        Git = 2
    }

    private MsgBoxType _msgBoxType = MsgBoxType.Info;
    
    public NightmareLibrary.Misc.ErrorCode Result { get; private set; }
    
    public MesgWindow() // Designer Only. Give it paramaters.
    {
        InitializeComponent();
    }

    public MesgWindow(string title, string content, MsgBoxType msgBoxType)
    {
        InitializeComponent();
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

                    bool OnTransferProgress(TransferProgress progress)
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
    }

    private void DeclineButton_OnClick(object? sender, EventArgs e)
    {
        if (_msgBoxType == MsgBoxType.YesNo || _msgBoxType == MsgBoxType.Git)
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