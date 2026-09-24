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
using static NightmareEditor.Managers.Standard;

namespace NightmareEditor
{
    public partial class App : Application
    {
        public static readonly string AssemblyName = "NightmareEditor";
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