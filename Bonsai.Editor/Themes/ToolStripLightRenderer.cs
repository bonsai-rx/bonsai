using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var textBox = item as ToolStripTextBox;
            if (textBox != null)
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
