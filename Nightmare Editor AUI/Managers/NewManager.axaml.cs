using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Nightmare_Editor_AUI;

public partial class NewManager : Window
{
    public NewManager()
    {
        InitializeComponent();
    }
    
    private void MenuButton_Hover(object? sender, PointerEventArgs e)
    {
        if (sender is Grid grid)
        {
            grid.Bind(
                MarginProperty,
                new Binding
                {
                    Path = "Height",
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                    {
                        AncestorType = typeof(Window)
                    },
                    Converter = (IValueConverter)this.Resources["PixelMargin"],
                    ConverterParameter = "12.0.0.3"
                }
            );
        }
    }

    private void MenuButton_EndHover(object? sender, PointerEventArgs e)
    {
        if (sender is Grid grid)
        {
            grid.Bind(
                MarginProperty,
                new Binding
                {
                    Path = "Height",
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                    {
                        AncestorType = typeof(Window)
                    },
                    Converter = (IValueConverter)this.Resources["PixelMargin"],
                    ConverterParameter = "0.0.0.3"
                }
            );
        }
    }
}