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
    
    private void Menu_Mods_OnClick(object? sender, EventArgs e)
    {
        MenuButtonsPanel.IsVisible = false;
        ModsWindow.IsVisible = true;
        MainText.Text = "Installed";
        MainTextShadow.Text = "Installed";
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
        
        EmulatorConfig.RightText = "No";
        if (MainSettings.Emulator)
        {
            EmulatorConfig.RightText = "Yes";
        }
        switch (MainSettings.Region)
        {
            case 0:
                RegionConfig.RightText = "North America";
                break;
            case 1:
                RegionConfig.RightText = "Europe";
                break;
            case 2:
                RegionConfig.RightText = "Japan";
                break;
            default:
                RegionConfig.RightText = "Unknown";
                break;
        }
        switch (MainSettings.ETC1Encoder)
        {
            case 0:
                ETCConfig.RightText = "Windows";
                break;
            case 1:
                ETCConfig.RightText = "Wine";
                break;
            case 2:
                ETCConfig.RightText = "Compatibility";
                break;
            default:
                ETCConfig.RightText = "Unknown";
                break;
        }
    }
    
    private void Window_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (SettingsWindow.IsVisible || ModsWindow.IsVisible)
            {
                MenuButtonsPanel.IsVisible = true;
                SettingsWindow.IsVisible = false;
                ModsWindow.IsVisible = false;
                MainText.Text = "Exam Editor";
                MainTextShadow.Text = "Exam Editor";
            }
        }
    }

    private void SwitchUIConfig_OnClick(object? sender, EventArgs e)
    {
        Settings settings = MainSettings;
        settings.UI = 0;
        SetSettings(settings);
        var mw = new Manager();
        mw.Show();
        Close();
    }

    private void EmulatorConfig_OnClick(object? sender, EventArgs e)
    {
        Settings settings = MainSettings;
        settings.Emulator = !settings.Emulator;
        SetSettings(settings);
        Refresh();
    }
    
    private void RegionConfig_OnClick(object? sender, EventArgs e)
    {
        Settings settings = MainSettings;
        settings.Region = (settings.Region + 1) % 3;
        SetSettings(settings);
        Refresh();
    }
    
    private void ETCConfig_OnClick(object? sender, EventArgs e)
    {
        Settings settings = MainSettings;
        settings.ETC1Encoder = (settings.ETC1Encoder + 1) % 3;
        SetSettings(settings);
        Refresh();
    }
}