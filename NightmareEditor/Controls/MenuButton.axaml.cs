using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Reactive;
using Avalonia.Svg.Skia;

namespace NightmareEditor.Controls;

public partial class MenuButton : UserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<MenuButton, string>(nameof(Text), defaultValue: "");
    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<MenuButton, string>(nameof(Description), defaultValue: "");
    public static readonly StyledProperty<FontChoices> FontProperty =
        AvaloniaProperty.Register<MenuButton, FontChoices>(nameof(Font), defaultValue: FontChoices.SmallAccurate);
    public static readonly StyledProperty<double> ScaleProperty =
        AvaloniaProperty.Register<MenuButton, double>(nameof(Scale), defaultValue: 1);
    public event EventHandler? Click;
    
    private bool IsPressed = false;
    
    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    
    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public enum FontChoices
    {
        Accurate,
        Appeal,
        Cutscene,
        SmallAccurate,
        SmallAppeal,
    }
    
    public FontChoices Font
    {
        get => GetValue(FontProperty);
        set => SetValue(FontProperty, value);
    }

    public double Scale
    {
        get => GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
    
    
    public MenuButton()
    {
        InitializeComponent();
        
        this.GetObservable(TextProperty)
            .Subscribe(new AnonymousObserver<string?>(e => Update()));
        this.GetObservable(FontProperty)
            .Subscribe(new AnonymousObserver<FontChoices>(e => Update()));
        this.GetObservable(DescriptionProperty)
            .Subscribe(new AnonymousObserver<string?>(e => Update()));
        this.GetObservable(ScaleProperty)
            .Subscribe(new AnonymousObserver<double>(e => Update()));
    }
    
    private void Update()
    {
        InternalLayoutTransformControl.LayoutTransform = new ScaleTransform(Scale, Scale);
        InternalKH3DText.Font = (KH3DText.FontChoices)(int)Font;
        InternalKH3DText.Text = Text;
    }
    
    private void MenuButton_Hover(object? sender, PointerEventArgs e)
    {
        
        InternalGrid.Margin = new Thickness(12 * Scale,0,0,0);
        InternalImage.Source = new SvgImage
        {
            Source = SvgSource.Load($"avares://Nightmare Editor AUI/Images/menu_selected.svg")
        };
        InternalImage.Effect = new DropShadowEffect
        {
            Color = Color.Parse("#cbc91f"),
            OffsetX = 0,
            OffsetY = 0,
            Opacity = 1,
            BlurRadius = 2
        };

        InternalGrid.Effect = new DropShadowEffect
        {
            Color = Color.Parse("#cbc91f"),
            OffsetX = 0,
            OffsetY = 0,
            Opacity = 1,
            BlurRadius = 2
        };
    }

    private void MenuButton_EndHover(object? sender, PointerEventArgs e)
    {
        InternalGrid.Margin = new Thickness(0,0,0,0);

        InternalImage.Source = new SvgImage
        {
            Source = SvgSource.Load($"avares://Nightmare Editor AUI/Images/menu_option.svg")
        };
        InternalImage.Effect = null;

        InternalGrid.Effect = null;
        IsPressed = false;
    }

    private void MenuButton_Press(object? sender, PointerPressedEventArgs e)
    {
        IsPressed = true;
    }

    private void MenuButton_Release(object? sender, PointerReleasedEventArgs e)
    {
        if (IsPressed)
        {
            Click?.Invoke(this, EventArgs.Empty);
        }
    }
}