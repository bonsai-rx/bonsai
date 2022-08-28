using System.Windows.Forms;

namespace Bonsai.Editor.Themes
{
    class ToolStripLightRenderer : ToolStripExtendedRenderer
    {
        public ToolStripLightRenderer()
            : base(new LightColorTable())
        {
        }

        protected override void InitializeItem(ToolStripItem item)
        {
            if (item is ToolStripTextBox textBox)
            {
                textBox.BackColor = ColorTable.WindowBackColor;
                textBox.ForeColor = ColorTable.WindowText;
            }
            else
            {
                item.BackColor = ColorTable.ControlBackColor;
                item.ForeColor = ColorTable.ControlForeColor;
            }

            base.InitializeItem(item);
        }
    }
}
