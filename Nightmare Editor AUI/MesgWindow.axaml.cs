using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using static Nightmare_Editor_AUI.Managers.Standard;

namespace Nightmare_Editor_AUI;

public partial class MesgWindow : Window
{
    public enum MsgBoxType
    {
        Info = 0,
        YesNo = 1,
        Git = 2
    }

    private MsgBoxType _msgBoxType = MsgBoxType.Info;
    
    public ErrorCode Result { get; private set; }
    
    public MesgWindow() // Designer Only. Give it paramaters.
    {
        InitializeComponent();
    }

    public MesgWindow(string title, string content, MsgBoxType msgBoxType)
    {
        InitializeComponent();
        _msgBoxType = msgBoxType;
        TitleText.Text = title;
        Content.Text = content;
        if (msgBoxType == MsgBoxType.Info)
        {
            AcceptButton.Text = "OK";
            AcceptButton.IsVisible = true;
            DeclineButton.IsVisible = false;
            ExtraButton.IsVisible = false;
            Title = "Nightmare Editor - INFORMATION";
        }
        else if (msgBoxType == MsgBoxType.YesNo)
        {
            AcceptButton.Text = "Yes";
            DeclineButton.Text = "No";
            AcceptButton.IsVisible = true;
            DeclineButton.IsVisible = true;
            ExtraButton.IsVisible = false;
            Title = "Nightmare Editor - WARNING";
        }
    }

    private void AcceptButton_OnClick(object? sender, EventArgs e)
    {
        if (_msgBoxType == MsgBoxType.Info)
        {
            Close();
        }
        else if (_msgBoxType == MsgBoxType.YesNo || _msgBoxType == MsgBoxType.Git)
        {
            Result = ErrorCode.Success;
            Close();
        }
    }

    private void DeclineButton_OnClick(object? sender, EventArgs e)
    {
        if (_msgBoxType == MsgBoxType.YesNo || _msgBoxType == MsgBoxType.Git)
        {
            Result = ErrorCode.Cancelled;
            Close();
        }
    }

    private void ExtraButton_OnClick(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
}