using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nightmare_Editor_AUI.Controls;
using Nightmare_Editor;
using Nightmare_Editor.NewTools;

namespace Nightmare_Editor_AUI.Views;

public partial class MusicWindowMenu : UserControl
{
    private Window parentWindow;
    public MusicWindowMenu()
    {
        InitializeComponent();
        AttachedToVisualTree += (_, _) =>
        {
            parentWindow = this.FindAncestorOfType<Window>();
        };
        CustomTrackSlot.MusicEntry = new MusicEntry
        {
            Track = "Custom Track",
            Description = "Shift-click to set a new file."
        };
        CustomTrackSlot.PointerEntered += MusicSlot_Hover;
    }

    public event EventHandler? ReportDescription;
    public event EventHandler? RequestRefresh;
    
    private async void Window_OnInitialized(object? sender, EventArgs e)
    {
        await Dispatcher.UIThread.InvokeAsync(
            () => { }, DispatcherPriority.Background);
        
        List<MusicEntry> musicEntries = JsonSerializer.Deserialize<List<MusicEntry>>(AssetLoader.Open(new Uri($"avares://Nightmare Editor AUI/Music/database.json", UriKind.RelativeOrAbsolute)), Managers.Standard.WriteIndented);
        for (int i = 0; i < musicEntries.Count; i++)
        {
            MusicSlot ms = new MusicSlot
            {
                MusicEntry = musicEntries[i],
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(10, 2, 5, 3)
            };
            ms.PointerEntered += MusicSlot_Hover;
            ms.PointerExited += MusicSlot_EndHover;
            ms.Click += MusicSlotInput_Click;
            MusicInputPanel.Items.Add(ms);
            await Dispatcher.UIThread.InvokeAsync(
                () => { }, DispatcherPriority.Background);
        }
        
        List<MusicEntry> musicOutEntries = JsonSerializer.Deserialize<List<MusicEntry>>(AssetLoader.Open(new Uri($"avares://Nightmare Editor AUI/Music/replacedb.json", UriKind.RelativeOrAbsolute)), Managers.Standard.WriteIndented);
        for (int i = 0; i < musicOutEntries.Count; i++)
        {
            MusicSlot ms = new MusicSlot
            {
                MusicEntry = musicOutEntries[i],
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(5, 2, 10, 3),
                IsPurple = true
            };
            ms.PointerEntered += MusicSlot_Hover;
            ms.PointerExited += MusicSlot_EndHover;
            ms.Click += MusicSlotOutput_Click;
            MusicOutputPanel.Items.Add(ms);
            await Dispatcher.UIThread.InvokeAsync(
                () => { }, DispatcherPriority.Background);
        }
    }

    private void MusicSlot_Hover(object? sender, EventArgs e)
    {
        if (sender is MusicSlot ms)
        {
            ReportDescription?.Invoke(ms.MusicEntry.Description + "\nLocation: " + ms.MusicEntry.Filename, e);
        }
    }
    
    private void MusicSlot_EndHover(object? sender, EventArgs e)
    {
        
    }

    private void MusicSlotInput_Click(object? sender, EventArgs e)
    {
        if (sender is MusicSlot ms && !(ms.MusicEntry is null))
        {
            InputSlot.MusicEntry = ms.MusicEntry;
            InputSlot.EnableBlueHighlight = true;
            bool isNull = OutputSlot.MusicEntry is null;
            ReplaceButton.LeftText = isNull ? "Select an Output" : "Ready!";
            ReplaceButton.Opacity = isNull ? 0.5 : 1;
            ReplaceButton.IsEnabled = !isNull;
        }
    }
    
    private void MusicSlotOutput_Click(object? sender, EventArgs e)
    {
        if (sender is MusicSlot ms && !(ms.MusicEntry is null))
        {
            OutputSlot.MusicEntry = ms.MusicEntry;
            OutputSlot.EnableBlueHighlight = true;
            bool isNull = InputSlot.MusicEntry is null;
            ReplaceButton.LeftText = isNull ? "Select an Input" : "Ready!";
            ReplaceButton.Opacity = isNull ? 0.5 : 1;
            ReplaceButton.IsEnabled = !isNull;
        }
    }

    private async void CustomTrackSlot_Click(object? sender, PointerEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CustomTrackSlot.MusicEntry.Filename) || (e.KeyModifiers & KeyModifiers.Shift) != 0)
        {
            IsEnabled = false;
            var files = await parentWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Select a music file.",
                AllowMultiple = false,
                FileTypeFilter = Misc.FileFilters.bcstm
            });
            if (files is null || files.Count == 0)
            {
                return;
            }
            if (files.Count == 1)
            {
                if (!string.IsNullOrWhiteSpace(files[0].Path.LocalPath))
                {
                    CustomTrackSlot.MusicEntry.Filename = files[0].Path.LocalPath;
                }
            }
        }
        if (!string.IsNullOrWhiteSpace(CustomTrackSlot.MusicEntry.Filename))
        {
            OutputSlot.MusicEntry = CustomTrackSlot.MusicEntry;
        }
        IsEnabled = true;
    }

    private void ReplaceButton_Click(object? sender, EventArgs e)
    {
        List<MusicEntry[]> music = JsonSerializer.Deserialize<MusicList>(File.ReadAllText(Misc.Jsons.music), Managers.Standard.WriteIndented).Music;
        MusicEntry replace = OutputSlot.MusicEntry;
        if (replace.Track == "Custom Track") replace.Description = "A custom track selected by the user.";
        for (int i = 0; i < music.Count; i++)
        {
            if (music[i][0].Track == InputSlot.MusicEntry.Track)
            {
                music[i][0] = InputSlot.MusicEntry;
                music[i][1] = replace;
                MusicList musiclistEarly = new MusicList();
                musiclistEarly.Music = music;
                string jsonStringEarly = JsonSerializer.Serialize<MusicList>(musiclistEarly, Managers.Standard.WriteIndented);
                File.WriteAllText(Misc.Jsons.music, jsonStringEarly);
                return;
            }
        }
        music.Add(new MusicEntry[] { InputSlot.MusicEntry, replace });
        MusicList musiclist = new MusicList();
        musiclist.Music = music;
        string jsonString = JsonSerializer.Serialize<MusicList>(musiclist, Managers.Standard.WriteIndented);
        File.WriteAllText(Misc.Jsons.music, jsonString);
        RequestRefresh?.Invoke(this, EventArgs.Empty);
        
    }

    private void ReplaceButton_Hover(object? sender, PointerEventArgs e)
    {
        ReportDescription.Invoke(ReplaceButton.Description, EventArgs.Empty);
    }
}