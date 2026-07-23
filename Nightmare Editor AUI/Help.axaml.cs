using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Markdown.Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Reactive;
using Avalonia.VisualTree;
using Nightmare_Editor_AUI.Controls;

namespace Nightmare_Editor
{
    /// <summary>
    /// Interaction logic for Help.xaml
    /// </summary>
    public partial class Help : Window
    {
        private static double scale = 1;
        public Help()
        {
            InitializeComponent();
            this.GetObservable(HeightProperty)
                .Subscribe(new AnonymousObserver<double>(e => EnsureHeight()));
            this.GetObservable(WidthProperty)
                .Subscribe(new AnonymousObserver<double>(e => EnsureWidth()));

            MD = this.FindControl<Markdown.Avalonia.MarkdownScrollViewer>("MD");
            MD.Markdown = QuickRead("Help/Int.md");
            ScrollToTop();
            foreach (TextBox tb in TextBoxPanel.Children)
            {
                tb.IsReadOnly = true;
                tb.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);
                tb.Focusable = false;
            }
        }
        
        private void Click(object sender, PointerReleasedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                MD.Markdown = QuickRead($"Help/{tb.Name}.md");
                ScrollToTop();
            }
        }

        private string QuickRead(string sender)
        {
            var streamInfo = AssetLoader.Open(new Uri($"avares://Nightmare Editor AUI/{sender}", UriKind.RelativeOrAbsolute));
            using (var reader = new StreamReader(streamInfo))
            {
                return reader.ReadToEnd();
            }
        }

        private void ScrollToTop()
        {
            var scrollViewer = MD.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToHome();
            }
        }

        private void Window_OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemMinus)
            {
                scale *= 10.0 / 9.0;
            }
            if (e.Key == Key.OemPlus)
            {
                scale *= 0.9;
            }
            if (e.Key == Key.D0)
            {
                scale = 1;
            }

            if (scale <= 0)
            {
                scale = 0.1;
            }
            Main.Height = Height * scale;
            Main.Width = Width * scale;
        }

        private void EnsureWidth()
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                Main.Width = this.Width * scale;
            });
        }
        private void EnsureHeight()
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                Main.Height = this.Height * scale;
            });
        }
    }
}
