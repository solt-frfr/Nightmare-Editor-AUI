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
        Refresh();
    }


    private void MenuButton_Hover(object? sender, PointerEventArgs e)
    {
        if (sender is Nightmare_Editor_AUI.Controls.MenuButton mb)
            BottomRightText.Text = mb.Description;
        if (sender is Nightmare_Editor_AUI.Controls.ConfigSlot cs)
            BottomRightText.Text = cs.Description;
    }
    
    private void Menu_Settings_OnClick(object? sender, EventArgs e)
    {
        MenuButtonsPanel.IsVisible = false;
        SettingsWindow.IsVisible = true;
        MainText.Text = "Settings";
        MainTextShadow.Text = "Settings";
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

    private void DeployPathConfig_OnClick(object? sender, EventArgs e)
    {
        SetModDeployPath(this);
        Refresh();
    }

    private void Refresh()
    {
        DeployPathConfig.Description = "After clicking Deploy, the mods will be placed in \" " + MainSettings.DeployPath + " \".";
    }
    
    private void Window_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (SettingsWindow.IsVisible)
            {
                MenuButtonsPanel.IsVisible = true;
                SettingsWindow.IsVisible = false;
                MainText.Text = "Exam Editor";
                MainTextShadow.Text = "Exam Editor";
            }
        }
    }
}