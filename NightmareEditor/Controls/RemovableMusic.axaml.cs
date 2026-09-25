using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Reactive;
using NightmareEditor;

namespace NightmareEditor.Controls;

public partial class RemovableMusic : UserControl
{
    public static readonly StyledProperty<MusicEntry[]> MusicEntriesProperty =
        AvaloniaProperty.Register<RemovableMusic, MusicEntry[]>(nameof(MusicEntries));
    
    public class MusicRemoveEventArgs : EventArgs
    {
        public MusicEntry[] musicEntries { get; set; }
    }
    
    public event EventHandler<MusicRemoveEventArgs>? Click;
    
    private bool IsPressed = false;
    
    public MusicEntry[] MusicEntries
    {
        get => GetValue(MusicEntriesProperty);
        set => SetValue(MusicEntriesProperty, value);
    }
    
    public RemovableMusic()
    {
        InitializeComponent();
        this.GetObservable(MusicEntriesProperty)
            .Subscribe(new AnonymousObserver<MusicEntry[]>(e => Update()));
    }
    
    private void Update()
    {
        if (MusicEntries is null) return;
        InternalTextBlock.Text = MusicEntries[0].Track + " <-- " + MusicEntries[1].Track;
    }

    private void InternalButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Click?.Invoke(this, new MusicRemoveEventArgs()
        {
            musicEntries = MusicEntries
        });
    }
}