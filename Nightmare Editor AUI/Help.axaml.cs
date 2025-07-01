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
        }

        private void Int_Click(object sender, PointerReleasedEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Int.md");
            ScrollToTop();
        }

        private void Add_Click(object sender, PointerReleasedEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Add.md");
            ScrollToTop();
        }

        private void Usr_Click(object sender, PointerReleasedEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Usr.md");
            ScrollToTop();
        }

        private void Tkt_Click(object sender, PointerReleasedEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Tkt.md");
            ScrollToTop();
        }

        private void Lnk_Click(object sender, PointerReleasedEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Lnk.md");
            ScrollToTop();
        }

        private void For_Click(object sender, PointerReleasedEventArgs e)
        {
            MD.Markdown = QuickRead("Help/For.md");
            ScrollToTop();
        }

        private void Txa_Click(object sender, PointerReleasedEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Txa.md");
            ScrollToTop();
        }

        private void Pak_Click(object sender, PointerReleasedEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Pak.md");
            ScrollToTop();
        }

        private void Mod_Click(object sender, PointerReleasedEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Mod.md");
            ScrollToTop();
        }

        private void Qrk_Click(object sender, PointerReleasedEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Qrk.md");
            ScrollToTop();
        }

        private void Faq_Click(object sender, PointerReleasedEventArgs e)
        {
            MD.Markdown = QuickRead("Help/Faq.md");
            ScrollToTop();
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
