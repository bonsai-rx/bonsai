using System;
using System.Reflection;
using System.Windows.Forms;
using Bonsai.Editor.Themes;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Bonsai.Editor.GraphView
{
    class AnnotationPanel : UserControl
    {
        static readonly object LinkClickedEvent = new();
        static readonly object CloseRequestedEvent = new();
        readonly RichTextBox textBox;
        readonly WebView2 webView;
        bool webViewInitialized;
        Action onInitialize;

        public AnnotationPanel()
        {
            if (EditorSettings.IsRunningOnMono)
            {
                textBox = new RichTextBox();
                textBox.Font = new System.Drawing.Font(Font.FontFamily, 12);
                textBox.BorderStyle = BorderStyle.None;
                textBox.Location = new System.Drawing.Point(2, 25);
                textBox.Multiline = true;
                textBox.ReadOnly = true;
                textBox.ScrollBars = RichTextBoxScrollBars.Vertical;
                textBox.Margin = new Padding(2);
                textBox.Size = new System.Drawing.Size(296, 70);
                textBox.WordWrap = true;
                textBox.Dock = DockStyle.Fill;
                textBox.ContextMenuStrip = new ContextMenuStrip();
                textBox.ContextMenuStrip.Items.Add("Close", null, (sender, e) => OnCloseRequested(e));
                textBox.LinkClicked += (sender, e) => OnLinkClicked(e);
                Controls.Add(textBox);
            }
            else
            {
                webView = new WebView2();
                webView.AllowExternalDrop = true;
                webView.CreationProperties = null;
                webView.DefaultBackgroundColor = System.Drawing.Color.White;
                webView.Dock = DockStyle.Fill;
                webView.Location = new System.Drawing.Point(2, 25);
                webView.Margin = new Padding(2);
                webView.Size = new System.Drawing.Size(296, 70);
                webView.ZoomFactor = 1D;
                webView.CoreWebView2InitializationCompleted += (sender, e) =>
                {
                    webViewInitialized = true;
                    webView.CoreWebView2.ContextMenuRequested += CoreWebView2_ContextMenuRequested;
                    webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                        MarkdownConvert.DefaultUrl,
                        Environment.CurrentDirectory,
                        CoreWebView2HostResourceAccessKind.Allow);
                    webView.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
                    webView.CoreWebView2.AddWebResourceRequestedFilter(
                        $"https://{MarkdownConvert.EmbeddedUrl}/*",
                        CoreWebView2WebResourceContext.Stylesheet);
                    InitializeTheme();
                    if (!string.IsNullOrEmpty(Text))
                    {
                        OnTextChanged(EventArgs.Empty);
                    }

                    onInitialize?.Invoke();
                    onInitialize = null;
                };
                Controls.Add(webView);
            }
        }

        public ThemeRenderer ThemeRenderer { get; set; }

        public bool HasWebView
        {
            get { return webView != null; }
        }

        public event LinkClickedEventHandler LinkClicked
        {
            add { Events.AddHandler(LinkClickedEvent, value); }
            remove { Events.RemoveHandler(LinkClickedEvent, value); }
        }

        public event EventHandler CloseRequested
        {
            add { Events.AddHandler(CloseRequestedEvent, value); }
            remove { Events.RemoveHandler(CloseRequestedEvent, value); }
        }

        public void NavigateToString(string text)
        {
            if (webView == null)
            {
                textBox.Text = text;
            }
            else if (webViewInitialized)
            {
                var html = MarkdownConvert.ToHtml(Font, text);
                webView.NavigateToString(html);
            }
            else onInitialize = () => NavigateToString(text);
        }

        public void Navigate(string uri)
        {
            if (webViewInitialized)
            {
                webView.CoreWebView2.Navigate(uri);
            }
            else onInitialize = () => Navigate(uri);
        }

        internal void InitializeTheme()
        {
            var colorTable = ThemeRenderer.ToolStripRenderer.ColorTable;
            if (webView == null)
            {
                textBox.BackColor = colorTable.WindowBackColor;
                textBox.ForeColor = colorTable.ControlForeColor;
            }
            else
            {
                webView.DefaultBackgroundColor = colorTable.WindowBackColor;
                webView.BackColor = colorTable.ControlBackColor;
                webView.ForeColor = colorTable.ControlForeColor;
                if (webView.CoreWebView2 != null)
                {
                    webView.CoreWebView2.Profile.PreferredColorScheme = ThemeRenderer.ActiveTheme switch
                    {
                        ColorTheme.Light => CoreWebView2PreferredColorScheme.Light,
                        ColorTheme.Dark => CoreWebView2PreferredColorScheme.Dark,
                        _ => CoreWebView2PreferredColorScheme.Auto
                    };
                }
            }
        }

        private void OnLinkClicked(LinkClickedEventArgs e)
        {
            (Events[LinkClickedEvent] as LinkClickedEventHandler)?.Invoke(this, e);
        }

        private void OnCloseRequested(EventArgs e)
        {
            (Events[CloseRequestedEvent] as EventHandler)?.Invoke(this, e);
        }

        private void CoreWebView2_ContextMenuRequested(object sender, CoreWebView2ContextMenuRequestedEventArgs e)
        {
            var closeMenuItem = webView.CoreWebView2.Environment.CreateContextMenuItem(
                "Close",
                iconStream: null,
                CoreWebView2ContextMenuItemKind.Command);
            closeMenuItem.CustomItemSelected += delegate { OnCloseRequested(EventArgs.Empty); };
            e.MenuItems.Add(closeMenuItem);
        }

        private void CoreWebView2_WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            if (e.ResourceContext == CoreWebView2WebResourceContext.Stylesheet)
            {
                var resourceUri = new Uri(e.Request.Uri);
                if (resourceUri.Segments?.Length == 2)
                {
                    var resourceStream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream($"Bonsai.Editor.Resources.WebView.{resourceUri.Segments[1]}");
                    var response = webView.CoreWebView2.Environment.CreateWebResourceResponse(
                        resourceStream, 200, "OK", "Content-Type: text/css");
                    e.Response = response;
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            webView?.EnsureCoreWebView2Async();
            base.OnLoad(e);
        }
    }
}
