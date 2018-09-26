using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Editor.Themes
{
    class LightColorTable : ExtendedColorTable
    {
        public override Color ControlBackColor
        {
            get { return SystemColors.Control; }
        }

        public override Color InactiveCaption
        {
            get { return Color.LightGray; }
        }

        public override Color WindowBackColor
        {
            get { return Color.White; }
        }

        public override Color SeparatorDark
        {
            get { return Color.DarkGray; }
        }

        public override Color SeparatorLight
        {
            get { return Color.Gray; }
        }

        public override Color StatusStripGradientBegin
        {
            get { return ControlBackColor; }
        }

        public override Color StatusStripGradientEnd
        {
            get { return ControlBackColor; }
        }

        public override Color ToolStripBorder
        {
            get { return ControlBackColor; }
        }

        public override Color ToolStripGradientBegin
        {
            get { return ControlBackColor; }
        }

        public override Color ToolStripGradientMiddle
        {
            get { return ControlBackColor; }
        }

        public override Color ToolStripGradientEnd
        {
            get { return ControlBackColor; }
        }
    }
}
