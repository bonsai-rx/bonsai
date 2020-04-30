using System.Windows.Forms;

namespace Bonsai.Editor.Themes
{
    class ToolStripLightRenderer : ToolStripExtendedRenderer
    {
        public ToolStripLightRenderer()
            : base(new LightColorTable())
        {
        }
    }
}
