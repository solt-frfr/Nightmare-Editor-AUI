using System;
using System.Collections.Generic;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using Nightmare_Editor_AUI.Controls;
using Nightmare_Editor;

namespace Nightmare_Editor_AUI.Views;

public partial class MusicWindowMenu : UserControl
{
    public MusicWindowMenu()
    {
        InitializeComponent();
    }

    public event EventHandler? ReportDescription;
    
    private async void Window_OnInitialized(object? sender, EventArgs e)
    {
        await Dispatcher.UIThread.InvokeAsync(
            () => { }, DispatcherPriority.Background);
        MusicInputPanel.Items.Clear();
        MusicOutputPanel.Items.Clear();
        
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

    private void MusicSlot_Hover(object sender, EventArgs e)
    {
        if (sender is MusicSlot ms)
        {
            ReportDescription?.Invoke(ms.MusicEntry.Description + "\nLocation: " + ms.MusicEntry.Filename, e);
        }
    }
    
    private void MusicSlot_EndHover(object sender, EventArgs e)
    {
        
    }

    private void MusicSlotInput_Click(object sender, EventArgs e)
    {
        if (sender is MusicSlot ms && !(ms.MusicEntry is null))
        {
            InputSlot.MusicEntry = ms.MusicEntry;
        }
    }
    
    private void MusicSlotOutput_Click(object sender, EventArgs e)
    {
        if (sender is MusicSlot ms && !(ms.MusicEntry is null))
        {
            OutputSlot.MusicEntry = ms.MusicEntry;
        }
    }
}