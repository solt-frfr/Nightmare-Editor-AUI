using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Reactive;
using Avalonia.Threading;
using NightmareEditor;

namespace NightmareEditor.Controls;

public partial class ModSlot : UserControl
{
    public static readonly StyledProperty<FontChoices> FontProperty =
        AvaloniaProperty.Register<ModSlot, FontChoices>(nameof(Font), defaultValue: FontChoices.SmallAccurate);
    public static readonly StyledProperty<Meta> ModMetaProperty =
        AvaloniaProperty.Register<ModSlot, Meta>(nameof(ModMeta));
    
    public event EventHandler? Click;
    
    private bool IsPressed = false;
    

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
    
    public Meta ModMeta
    {
        get => GetValue(ModMetaProperty);
        set => SetValue(ModMetaProperty, value);
    }
    
    public ModSlot()
    {
        InitializeComponent();
        
        this.GetObservable(FontProperty)
            .Subscribe(new AnonymousObserver<FontChoices>(e => RefreshUI()));
        this.GetObservable(ModMetaProperty)
            .Subscribe(new AnonymousObserver<Meta>(e => Dispatcher.UIThread.Post(RefreshUI)));
    }
    
    private void RefreshUI()
    {
        Meta useableMeta = ModMeta;
        if (ModMeta == null)
        {
            useableMeta = new Meta();
            useableMeta.Name = "Whoops!";
        }
        InternalKH3DText.Font = (KH3DText.FontChoices)(int)Font;
        InternalKH3DText.Text = useableMeta.Name;
        if (Managers.Standard.EnabledMods.Contains(useableMeta.ID))
        {
            EquipE.IsVisible = true;
        }
        else
        {
            EquipE.IsVisible = false;
        }

        Color tempColor = Avalonia.Media.Color.Parse("#a80000");
        if (Avalonia.Media.Color.TryParse(useableMeta.Color, out var color))
        {
            tempColor = color;
        }
        Color darkColor = new Color(tempColor.A, (byte)Math.Clamp(tempColor.R - 40, 0, tempColor.R), (byte)Math.Clamp(tempColor.G - 40, 0, tempColor.G), (byte)Math.Clamp(tempColor.B - 40, 0, tempColor.B));
        MainGrid.Background = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            GradientStops = new GradientStops
            {
                new GradientStop(tempColor, 0),
                new GradientStop(darkColor, 1),
            }
        };
    }
    
    private void ModSlot_Hover(object? sender, PointerEventArgs e)
    {
        EffectAmp1.Effect = new DropShadowEffect
        {
            Color = Avalonia.Media.Color.Parse("#cbc91f"),
            OffsetX = 0,
            OffsetY = 0,
            Opacity = 1,
            BlurRadius = 0.25
        };

        EffectAmp2.Effect = new DropShadowEffect
        {
            Color = Avalonia.Media.Color.Parse("#cbc91f"),
            OffsetX = 0,
            OffsetY = 0,
            Opacity = 1,
            BlurRadius = 0.25
        };
        
        EffectAmp3.Effect = new DropShadowEffect
        {
            Color = Avalonia.Media.Color.Parse("#cbc91f"),
            OffsetX = 0,
            OffsetY = 0,
            Opacity = 1,
            BlurRadius = 0.25
        };
        
        EffectAmp4.Effect = new DropShadowEffect
        {
            Color = Avalonia.Media.Color.Parse("#cbc91f"),
            OffsetX = 0,
            OffsetY = 0,
            Opacity = 1,
            BlurRadius = 0.25
        };
        RefreshUI();
    }

    private void ModSlot_EndHover(object? sender, PointerEventArgs e)
    {
        EffectAmp1.Effect = null;
        EffectAmp2.Effect = null;
        EffectAmp3.Effect = null;
        EffectAmp4.Effect = null;
        IsPressed = false;
    }

    private void ModSlot_Press(object? sender, PointerPressedEventArgs e)
    {
        IsPressed = true;
    }

    private void ModSlot_Release(object? sender, PointerReleasedEventArgs e)
    {
        if (IsPressed)
        {
            Click?.Invoke(this, EventArgs.Empty);
        }
    }
}