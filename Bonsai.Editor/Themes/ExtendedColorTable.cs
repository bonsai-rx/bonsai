using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Editor.Themes
{
    class ExtendedColorTable : ProfessionalColorTable
    {
        public virtual Color ControlBackColor
        {
            get { return SystemColors.Control; }
        }

        public virtual Color ControlForeColor
        {
            get { return SystemColors.ControlText; }
        }

        public virtual Color ControlText
        {
            get { return SystemColors.ControlText; }
        }

        public virtual Color ControlDark
        {
            get { return SystemColors.ControlDark; }
        }

        public virtual Color ContentPanelBackColor
        {
            get { return Color.WhiteSmoke; }
        }

        public virtual Color InactiveCaption
        {
            get { return SystemColors.InactiveCaption; }
        }

        public virtual Color WindowBackColor
        {
            get { return SystemColors.Window; }
        }

        public virtual Color WindowText
        {
            get { return SystemColors.WindowText; }
        }
    }
}
