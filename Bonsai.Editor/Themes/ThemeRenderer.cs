using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            var handler = ThemeChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

    enum ColorTheme
    {
        Light,
        Dark
    }
}
