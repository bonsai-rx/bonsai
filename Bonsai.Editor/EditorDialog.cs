using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Bonsai.Editor
{
    static class EditorDialog
    {
        public static void OpenUrl(Uri url)
        {
            OpenUrl(url.AbsoluteUri);
        }

        public static void OpenUrl(string url)
        {
            var validUrl = Uri.TryCreate(url, UriKind.Absolute, out Uri result) &&
                (result.Scheme == Uri.UriSchemeFile || result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
            if (!validUrl)
            {
                throw new ArgumentException("The URL is malformed.");
            }

            var activeForm = Form.ActiveForm;
            try
            {
                if (activeForm != null) activeForm.Cursor = Cursors.AppStarting;
                if (EditorSettings.IsRunningOnMono && Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    Process.Start("xdg-open", url);
                }
                else Process.Start(url);
            }
            catch { } //best effort
            finally
            {
                if (activeForm != null) activeForm.Cursor = null;
            }
        }

        public static void ShowDocs()
        {
            OpenUrl("http://bonsai-rx.org/docs/articles/editor");
        }

        public static void ShowForum()
        {
            OpenUrl("https://github.com/bonsai-rx/bonsai/discussions");
        }

        public static void ShowReportBug()
        {
            OpenUrl("https://github.com/bonsai-rx/bonsai/issues");
        }

        public static void ShowAboutBox()
        {
            using var about = new AboutBox();
            about.ShowDialog();
        }

        public static EditorResult ShowStartScreen(out string fileName)
        {
            using var start = new StartScreen();
            start.ShowDialog();
            fileName = start.FileName;
            return start.EditorResult;
        }
    }
}
