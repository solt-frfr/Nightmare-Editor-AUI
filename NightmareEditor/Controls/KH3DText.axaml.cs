using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Reactive;
using Avalonia.Svg.Skia;
using Avalonia.Threading;

namespace Nightmare_Editor_AUI.Controls;

public partial class KH3DText : UserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<KH3DText, string>(nameof(Text), defaultValue: "");
    public static readonly StyledProperty<FontChoices> FontProperty =
        AvaloniaProperty.Register<KH3DText, FontChoices>(nameof(Font), defaultValue: FontChoices.Accurate);
    public static readonly StyledProperty<ImmutableSolidColorBrush> ColorProperty =
        AvaloniaProperty.Register<KH3DText, ImmutableSolidColorBrush>(nameof(Color), defaultValue: new ImmutableSolidColorBrush(Avalonia.Media.Colors.Black));
    public static readonly StyledProperty<bool> DropShadowProperty =
        AvaloniaProperty.Register<KH3DText, bool>(nameof(DropShadowProperty), defaultValue: true);
    
    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
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
    
    public ImmutableSolidColorBrush Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }
        
    public KH3DText()
    {
        InitializeComponent();
        
        this.GetObservable(TextProperty)
            .Subscribe(new AnonymousObserver<string?>(e => UpdateText()));
        this.GetObservable(FontProperty)
            .Subscribe(new AnonymousObserver<FontChoices>(e => UpdateText()));
        this.GetObservable(ColorProperty)
            .Subscribe(new AnonymousObserver<ImmutableSolidColorBrush?>(e => UpdateText()));
        this.GetObservable(DropShadowProperty)
            .Subscribe(new AnonymousObserver<bool>(e => UpdateText()));
    }
    
    public bool DropShadow
    {
        get => GetValue(DropShadowProperty);
        set => SetValue(DropShadowProperty, value);
    }

    public void UpdateText()
    {
        Panel.Children.Clear();
        if (DropShadow)
        {
            Panel.Effect = new DropShadowEffect
            {
                BlurRadius = 0,
                OffsetX = 1,
                OffsetY = 1,
                Opacity = 1,
                Color = Avalonia.Media.Colors.Black,
            };
        }
        
        if (string.IsNullOrWhiteSpace(Text))
        {
            return;
        }

        foreach (string line in Text.Split('\n'))
        {
            WrapPanel linePanel = new WrapPanel();
            linePanel.Orientation = Orientation.Horizontal;
            foreach (string word in line.Split(' '))
            {
                string[] splitwords = word.Split(System.IO.Path.DirectorySeparatorChar);
                foreach (string splitword in splitwords)
                {
                    FontChoices fontToUse = Font;
                    if (word != splitword)
                    {
                        if (Font == FontChoices.Accurate)
                        {
                            fontToUse = FontChoices.SmallAccurate;
                        }
                        if (Font == FontChoices.Appeal)
                        {
                            fontToUse = FontChoices.SmallAppeal;
                        }
                    }
                    StackPanel tempPanel = new StackPanel();
                    tempPanel.Orientation = Orientation.Horizontal;
                    if (Font == FontChoices.Accurate)
                    {
                        tempPanel.Margin = new Thickness(0,0,3,0);
                        tempPanel.Height = 16;
                    }
                    if (Font == FontChoices.SmallAccurate)
                    {
                        tempPanel.Margin = new Thickness(0,0,2,0);
                        tempPanel.Height = 14;
                    }

                    if (splitword != splitwords[splitwords.Length - 1])
                    {
                        tempPanel.Margin = new Thickness(0,0,1,0);
                    }

                    if (splitword != splitwords[0])
                    {
                        try
                        {
                            Path tempPath = FontPath(fontToUse, '/');
                            tempPath.Fill = Color;
                            tempPanel.Children.Add(new Viewbox
                            {
                                Child = tempPath,
                                Margin = new Thickness(0, 0, 1, GetBottomThickness('/', fontToUse)),
                                Height = tempPath.Height,
                                Width = tempPath.Width,
                                Stretch = Stretch.Fill,
                                VerticalAlignment = VerticalAlignment.Bottom,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                StretchDirection = StretchDirection.Both
                                
                            });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                    foreach (char character in splitword)
                    {
                        if ((int)character == 13)
                            continue;
                        try
                        {
                            Path tempPath = FontPath(fontToUse, character);
                            tempPath.Fill = Color;
                            tempPanel.Children.Add(new Viewbox
                            {
                                Child = tempPath,
                                Margin = new Thickness(0, 0, 1, GetBottomThickness(character, fontToUse)),
                                Height = tempPath.Height,
                                Width = tempPath.Width,
                                Stretch = Stretch.Fill,
                                VerticalAlignment = VerticalAlignment.Bottom,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                StretchDirection = StretchDirection.Both
                                
                            });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }

                    linePanel.Children.Add(tempPanel);
                }
            }
            
            Panel.Children.Add(linePanel);
        }
        
    }

    public double GetBottomThickness(char character, FontChoices font)
    {
        int index = (int)character;
        if (font == FontChoices.Accurate)
        {
            if (character == 'g' || character == 'j' || character == 'p' || character == 'q' || 
                character == ',')
                return 0;
            if (character == 'y' || character == '$' || character == '[' || character == ']' ||
                character == '{' || character == '}' || character == '|')
                return 1;
            if (character == '.')
                return 1.5;
            if (character == '+' || character == ':')
                return 3;
            if (character == '=')
                return 5;
            if (character == '-')
                return 7;
            if (character == '\"')
                return 8;
            if (character == '\'' || character == '`')
                return 9;
            if (character == '^' || character == '~')
                return 11;
            return 2;
        }

        if (font == FontChoices.SmallAccurate)
        {
            if (character == 'j' || character == ',')
                return 1;
            if (character == 'g' || character == 'p' || character == 'q' || character == 'y' ||
                character == '$' || character == '(' || character == ')' || character == '<' ||
                character == '>' || character == '[' || character == ']' || character == '{' ||
                character == '}')
                return 2;
            if (character == ':')
                return 3.5;
            if (character == '*' || character == '+' || character == '=')
                return 5;
            if (character == '-')
                return 7;
            if (character == '^')
                return 8;
            if (character == '\"' || character == '\'' || character == '~')
                return 9;
            if (character == '`')
                return 10;
            return 3;
        }
        return 0;

    }
    
    public static bool InRange(int value, int min, int max)
    {
        if (value >= min && value <= max)
            return true;
        return false;
    }

    public Path FontPath(FontChoices fontenum, char character)
    {
        (string data, double width, double height) value = ("", 0, 0);

        if ((int)fontenum == 0)
        {
            value = KH3DFonts.Accurate[(int)character];
        }
        if ((int)fontenum == 3)
        {
            value = KH3DFonts.SmallAccurate[(int)character];
        }

        return new Path
        {
            Data = Avalonia.Media.Geometry.Parse(value.data),
            Width = value.width,
            Height = value.height
        };
    }
}