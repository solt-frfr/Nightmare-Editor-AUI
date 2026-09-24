using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Reactive;

namespace NightmareEditor.Controls;

public partial class EditorFile : UserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<EditorFile, string>(nameof(Text), defaultValue: "");
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<EditorFile, bool>(nameof(IsSelected), defaultValue: false);
    public static readonly StyledProperty<bool> IsUserAddedProperty =
        AvaloniaProperty.Register<EditorFile, bool>(nameof(IsUserAdded), defaultValue: false);
    public static readonly StyledProperty<bool> RequestedContextProperty =
        AvaloniaProperty.Register<EditorFile, bool>(nameof(RequestedContext), defaultValue: false);
        
    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    
    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }
    
    public bool IsUserAdded
    {
        get => GetValue(IsUserAddedProperty);
        set => SetValue(IsUserAddedProperty, value);
    }
    
    public bool RequestedContext
    {
        get => GetValue(RequestedContextProperty);
        set => SetValue(RequestedContextProperty, value);
    }
    
    public event EventHandler? Click;
    
    private bool IsPressed = false;
    
    public EditorFile()
    {
        InitializeComponent();
        this.GetObservable(TextProperty)
            .Subscribe(new AnonymousObserver<string>(e => Update()));
        this.GetObservable(IsSelectedProperty)
            .Subscribe(new AnonymousObserver<bool>(e => Update()));
        this.GetObservable(RequestedContextProperty)
            .Subscribe(new AnonymousObserver<bool>(e => Update()));
        Update();
    }

    private void Update()
    {
        TextBlock.Text = Text;
        
        TextBlock.Background = new SolidColorBrush(Avalonia.Media.Color.Parse(IsSelected ? "#F04080" : "#202020"));
        Border.BorderBrush = new SolidColorBrush(Avalonia.Media.Color.Parse(RequestedContext ? "#F00000" : "#424242"));
    }
    
    private void EndHover(object? sender, PointerEventArgs e)
    {
        IsPressed = false;
    }

    private void Press(object? sender, PointerPressedEventArgs e)
    {
        IsPressed = true;
    }

    private void Release(object? sender, PointerReleasedEventArgs e)
    {
        if (IsPressed && !ProgressBar.IsVisible)
        {
            Click?.Invoke(this, EventArgs.Empty);
        }
    }

    public void RestoreCursor()
    {
        Grid.Cursor = new Cursor(StandardCursorType.Hand);
    }

    public void ChangeCursor(StandardCursorType cursorType)
    {
        Grid.Cursor = new Cursor(cursorType);
    }
}