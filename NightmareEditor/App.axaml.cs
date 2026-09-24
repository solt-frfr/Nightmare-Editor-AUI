using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Nightmare_Editor;
using Nightmare_Editor_AUI.ViewModels;
using Nightmare_Editor_AUI;
using static Nightmare_Editor_AUI.Managers.Standard;

namespace Nightmare_Editor_AUI
{
    public partial class App : Application
    {
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