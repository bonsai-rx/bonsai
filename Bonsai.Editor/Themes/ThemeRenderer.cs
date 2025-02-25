using System;

namespace Bonsai.Editor.Themes
{
    class ThemeRenderer
    {
        ColorTheme activeTheme;
        readonly ToolStripExtendedRenderer darkRenderer;
        readonly ToolStripExtendedRenderer lightRenderer;
        public event EventHandler ThemeChanged;

        public ThemeRenderer()
        {
            activeTheme = ColorTheme.Light;
            darkRenderer = new ToolStripDarkRenderer();
            lightRenderer = new ToolStripLightRenderer();
        }

        public int LabelHeight { get; set; }

        public ColorTheme ActiveTheme
        {
            get { return activeTheme; }
            set
            {
                activeTheme = value;
                OnThemeChanged(EventArgs.Empty);
            }
        }

        public ToolStripExtendedRenderer ToolStripRenderer
        {
            get { return activeTheme == ColorTheme.Dark ? darkRenderer : lightRenderer; }
        }

        private void OnThemeChanged(EventArgs e)
        {
            ThemeChanged?.Invoke(this, e);
        }
    }

    enum ColorTheme
    {
        Light,
        Dark
    }
}
