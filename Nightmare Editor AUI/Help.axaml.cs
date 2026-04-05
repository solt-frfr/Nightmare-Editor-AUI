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
using Markdown.Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.VisualTree;

namespace Nightmare_Editor
{
    /// <summary>
    /// Interaction logic for Help.xaml
    /// </summary>
    public partial class Help : Window
    {
        public Help()
        {
            InitializeComponent();
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
    }
}
