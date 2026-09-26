using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Immutable;
using Avalonia.Reactive;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace NightmareEditor.Controls;

public partial class KH3DTextBox : UserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<KH3DTextBox, string>(nameof(Text), defaultValue: "");
    public static readonly StyledProperty<string> WatermarkProperty =
        AvaloniaProperty.Register<KH3DTextBox, string>(nameof(Text), defaultValue: "");
    public static readonly StyledProperty<KH3DText.FontChoices> FontProperty =
        AvaloniaProperty.Register<KH3DTextBox, KH3DText.FontChoices>(nameof(Font), defaultValue: KH3DText.FontChoices.Accurate);
    public static readonly StyledProperty<ImmutableSolidColorBrush> ColorProperty =
        AvaloniaProperty.Register<KH3DTextBox, ImmutableSolidColorBrush>(nameof(Color), defaultValue: new ImmutableSolidColorBrush(Avalonia.Media.Colors.Black));
    
    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    
    public string Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }
    public KH3DText.FontChoices Font
    {
        get => GetValue(FontProperty);
        set => SetValue(FontProperty, value);
    }
    
    public ImmutableSolidColorBrush Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }
    
    public Window parentWindow;
    public KH3DTextBox()
    {
        InitializeComponent();
        AttachedToVisualTree += (_, _) =>
        {
            parentWindow = this.FindAncestorOfType<Window>();
        };
        this.GetObservable(WatermarkProperty)
            .Subscribe(new AnonymousObserver<string?>(e => InternalKH3DWatermark.Text = Watermark));
    }

    private void InputElement_OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        Carat.IsVisible = true;
        Console.WriteLine(Text);
    }
    
    private void InputElement_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        Carat.IsVisible = false;
    }

    private async void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Back)
        {
            if (InternalKH3DText.Text.Length < 2)
            {
                InternalKH3DText.Text = string.Empty;
            }
            else
            {
                int indexFromBack = 1;
                char[] charArray = InternalKH3DText.Text.ToCharArray();
                if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                {
                    while (indexFromBack < charArray.Length)
                    {
                        if (char.IsPunctuation(charArray[^indexFromBack]) ||
                            char.IsSeparator(charArray[^indexFromBack]))
                        {
                            if (indexFromBack > 1)
                            {
                                indexFromBack--;
                            }
                            break;
                        }
                        else indexFromBack++;
                    }
                }
                InternalKH3DText.Text = new string(InternalKH3DText.Text.ToCharArray()[0..^indexFromBack]);
            }
            e.Handled = true;
        }
        else if (!(e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Alt) || e.KeyModifiers.HasFlag(KeyModifiers.Meta)))
        {
            string character = "";
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                switch (e.Key)
                {
                    case Key.D0:
                        character += ")";
                        break;
                    case Key.D1:
                        character += "!";
                        break;
                    case Key.D2:
                        character += "@";
                        break;
                    case Key.D3:
                        character += "$";
                        break;
                    case Key.D5:
                        character += "%";
                        break;
                    case Key.D6:
                        character += "^";
                        break;
                    case Key.D7:
                        character += "&";
                        break;
                    case Key.D8:
                        character += "*";
                        break;
                    case Key.D9:
                        character += "(";
                        break;
                }
            }
            if (!string.IsNullOrWhiteSpace(e.KeySymbol)) character += e.KeySymbol.ToLower();
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift)) character = character.ToUpper();
            if (!string.IsNullOrWhiteSpace(character)) InternalKH3DText.Text += character;
            e.Handled = true;
        }
        else if (e.Key == Key.V)
        {
            var data = await parentWindow.Clipboard.TryGetTextAsync();
            if (data is string paste && !(paste is null))
            {
                InternalKH3DText.Text += paste;
            }
            e.Handled = true;
        }
        Text = InternalKH3DText.Text;
        InternalKH3DWatermark.IsVisible = string.IsNullOrEmpty(InternalKH3DText.Text);
        await Dispatcher.UIThread.InvokeAsync(
            () => { }, DispatcherPriority.Background);
        double width = InternalKH3DText.DesiredSize.Width;
        if (width > 2) width -= 2;
        Carat.Margin = new Thickness(width, 0, 0, 0);
        await Dispatcher.UIThread.InvokeAsync(
            () => { }, DispatcherPriority.Background);
        InternalScrollViewer.Offset = new Vector(
            InternalScrollViewer.Extent.Width - InternalScrollViewer.Viewport.Width,
            InternalScrollViewer.Offset.Y);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Focus();
    }
}