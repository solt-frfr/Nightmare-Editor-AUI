using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Styling;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using NightmareEditor;

namespace NightmareEditor.Controls;

public partial class MusicSlot : UserControl
{
    public static readonly StyledProperty<MusicEntry> MusicEntryProperty =
        AvaloniaProperty.Register<MusicSlot, MusicEntry>(nameof(MusicEntry));
    public static readonly StyledProperty<bool> IsPurpleProperty =
        AvaloniaProperty.Register<MusicSlot, bool>(nameof(IsPurple));
    public static readonly StyledProperty<bool> CanHighlightProperty =
        AvaloniaProperty.Register<MusicSlot, bool>(nameof(CanHighlight), defaultValue: true);
    public static readonly StyledProperty<bool> EnableBlueHighlightProperty =
        AvaloniaProperty.Register<MusicSlot, bool>(nameof(EnableBlueHighlight));
        
    private bool IsPressed = false;
    private Styles? pathStyles;
    public event EventHandler<PointerEventArgs>? Click;
    
    
    public MusicEntry MusicEntry
    {
        get => GetValue(MusicEntryProperty);
        set => SetValue(MusicEntryProperty, value);
    }
    
    public bool IsPurple
    {
        get => GetValue(IsPurpleProperty);
        set => SetValue(IsPurpleProperty, value);
    }
    
    public bool CanHighlight
    {
        get => GetValue(CanHighlightProperty);
        set => SetValue(CanHighlightProperty, value);
    }
    
    public bool EnableBlueHighlight
    {
        get => GetValue(EnableBlueHighlightProperty);
        set => SetValue(EnableBlueHighlightProperty, value);
    }

    public MusicSlot()
    {
        InitializeComponent();
        this.GetObservable(MusicEntryProperty)
            .Subscribe(new AnonymousObserver<MusicEntry>(e => Update()));
        this.GetObservable(BoundsProperty)
            .Subscribe(new AnonymousObserver<Rect>(e => Update()));
        this.GetObservable(IsPurpleProperty)
            .Subscribe(new AnonymousObserver<bool>(e => Update()));
        this.GetObservable(EnableBlueHighlightProperty)
            .Subscribe(new AnonymousObserver<bool>(e => ChangeHighlight()));
    }
    
    private async void Update()
    {
        double width = 0;
        double height = 0;
        
        if (!(Double.IsNaN(Width) || Double.IsInfinity(Width) || Width < 5))
        {
            width = Width - 4;
        }
        else if (!(Double.IsInfinity(Bounds.Width) || Double.IsNaN(Bounds.Width) || Bounds.Width < 5))
        {
            width = Bounds.Width - 4;
        }

        height = 18;
        if (width < 18) width = 18;
        if (!(MusicEntry is null)) InternalKH3DText.Text = MusicEntry.Track;
        InternalChildGrid.Width = width;
        InternalChildGrid.Height = height;
        InternalParentGrid.Width = width;
        InternalParentGrid.Height = height;
        InternalChildGrid.Clip = new PathGeometry
        {
            Figures = PathFigures.Parse($"M 9,0 l {width-18},0 a 9,9 0 0 1 0,18 L 9,18 A 9,9 0 0 1 9,0 Z")
        };
        InternalGrid.Width = width + 4;
        InternalGrid.Height = height + 4;
        InternalRectangle.Width = width - 18;

        if (IsPurple)
        {
            InternalImage.Source = new SvgImage
            {
                Source = SvgSource.Load($"avares://{App.AssemblyName}/Images/synthesis-purple.svg")
            }; 
            InternalRectangle.Fill = new LinearGradientBrush()
            {
                StartPoint = new RelativePoint(0.8, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop(Color.Parse("#a600fc"), 0),
                    new GradientStop(Color.Parse("#00a600fc"), 1),
                }
            };
        }
        else
        {
            InternalImage.Source = new SvgImage
            {
                Source = SvgSource.Load($"avares://{App.AssemblyName}/Images/synthesis-red.svg")
            }; 
            InternalRectangle.Fill = new LinearGradientBrush()
            {
                StartPoint = new RelativePoint(0.8, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop(Color.Parse("#fc3228"), 0),
                    new GradientStop(Color.Parse("#00fc3228"), 1),
                }
            };
        }
    }

    private void ChangeHighlight()
    {
        if (EnableBlueHighlight)
        {
            if (pathStyles is null)
            {
                pathStyles = new Styles();
                pathStyles.AddRange(InternalPath.Styles);
                InternalPath.Styles.Clear();
            }
            InternalPath.Stroke = new SolidColorBrush(Color.Parse("#34ffff"));
            InternalPath.IsVisible = true;
        }
        else
        {
            if (!(pathStyles is null))
            {
                InternalPath.Styles.AddRange(pathStyles);
                pathStyles = null;
            }
            InternalPath.Stroke = new SolidColorBrush(Color.Parse("#cbc91f"));
        }
    }
    
    private void Pointer_EndHover(object? sender, PointerEventArgs e)
    {
        IsPressed = false;
        InternalPath.IsVisible = EnableBlueHighlight;
        if (!(pathStyles is null) && !EnableBlueHighlight)
        {
            InternalPath.Styles.AddRange(pathStyles);
            pathStyles = null;
        }
    }

    private void Pointer_Press(object? sender, PointerPressedEventArgs e)
    {
        IsPressed = true;
        if (pathStyles is null)
        {
            pathStyles = new Styles();
            pathStyles.AddRange(InternalPath.Styles);
            InternalPath.Styles.Clear();
        }
    }

    private void Pointer_Release(object? sender, PointerReleasedEventArgs e)
    {
        if (IsPressed)
        {
            Click?.Invoke(this, e);
        }
        if (!(pathStyles is null) && !EnableBlueHighlight)
        {
            InternalPath.Styles.AddRange(pathStyles);
            pathStyles = null;
        }
    }

    private void Pointer_Hover(object? sender, PointerEventArgs e)
    {
        if (!EnableBlueHighlight) InternalPath.IsVisible = CanHighlight;
    }
}