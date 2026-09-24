using System;
using System.Net.Mime;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Reactive;

namespace Nightmare_Editor_AUI.Controls;

public partial class ImageButton : UserControl
{
    public static readonly StyledProperty<IImage> SourceProperty =
        AvaloniaProperty.Register<MenuButton, IImage>(nameof(Source));
    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<MenuButton, string>(nameof(Description), defaultValue: "");
    public static readonly StyledProperty<BitmapInterpolationMode> InterpolationModeProperty =
        AvaloniaProperty.Register<MenuButton, BitmapInterpolationMode>(nameof(InterpolationMode), defaultValue: BitmapInterpolationMode.HighQuality);
    
    public event EventHandler? Click;
    
    public IImage Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
    
    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    
    public BitmapInterpolationMode InterpolationMode
    {
        get => GetValue(InterpolationModeProperty);
        set => SetValue(InterpolationModeProperty, value);
    }
    
    public ImageButton()
    {
        InitializeComponent();
        
        this.GetObservable(SourceProperty)
            .Subscribe(new AnonymousObserver<IImage>(e => Update()));
        this.GetObservable(DescriptionProperty)
            .Subscribe(new AnonymousObserver<string?>(e => Update()));
        this.GetObservable(InterpolationModeProperty)
            .Subscribe(new AnonymousObserver<BitmapInterpolationMode>(e => Update()));
    }
    
    private void Update()
    {
        InternalImage.Source = Source;
        RenderOptions.SetBitmapInterpolationMode(InternalImage, InterpolationMode);
        InternalImage.InvalidateVisual();
    }

    private void InternalButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Click?.Invoke(this, EventArgs.Empty);
    }
}