using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using Nightmare_Editor;
using Nightmare_Editor_AUI;
using Nightmare_Editor_AUI.Managers;
using static Nightmare_Editor_AUI.Managers.Standard;
using Nightmare_Editor_AUI.Controls;

namespace Nightmare_Editor_AUI;

public partial class NewManager : Window
{
    public NewManager()
    {
        InitializeComponent();
    }


    private void MenuButton_Hover(object? sender, PointerEventArgs e)
    {
        if (sender is Nightmare_Editor_AUI.Controls.MenuButton mb)
            BottomRightText.Text = mb.Description;
    }

    

    private void Menu_NE_OnClick(object? sender, EventArgs e)
    {
        var ew = new Editor();
        ew.Show();
        Close();
    }
    
    private void Menu_Download_OnClick(object? sender, EventArgs e)
    {
        OpenGamebanana();
    }
}