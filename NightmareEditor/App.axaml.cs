using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using NightmareEditor;
using NightmareEditor.ViewModels;
using NightmareEditor.Managers;
using static NightmareEditor.Managers.Standard;

namespace NightmareEditor
{
    public partial class App : Application
    {
        public static readonly string AssemblyName = "NightmareEditor";
        public static readonly string Version = "1.0.3";

        public static readonly string About = """
                                              Exam Editor is a mod manager made by Solt11 specifically for the 3DS version of Kingdom Hearts Dream Drop Distance.
                                              Please use OpenKH for the PC version, any mods I make will likely have an equivalent PC version.
                                              
                                              Nightmare Editor is the real program, and I go more in-depth on my explanations about what and why I made this in the FAQ section of Nightmare Editor's Help Window.
                                              
                                              This is a port of the WPF version to Avalonia UI and has become the only supported version.
                                              """;
        
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (MainSettings.UI == 0)
                {
                    desktop.MainWindow = new Manager
                    {
                        DataContext = new MainWindowViewModel(),
                    };
                }
                else
                {
                    desktop.MainWindow = new NewManager
                    {
                        DataContext = new MainWindowViewModel(),
                    };
                }
                
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}