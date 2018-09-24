using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Editor.Themes
{
    class DarkColorTable : MapColorTable
    {
        public DarkColorTable()
            : base(new AntiColorTable(), ThemeHelper.Invert)
        {
        }

        class AntiColorTable : LightColorTable
        {
            public override Color ControlBackColor
            {
                get { return Color.LightGray; }
            }

            public override Color ContentPanelBackColor
            {
                get { return Color.Gainsboro; }
            }

            public override Color WindowBackColor
            {
                get { return SystemColors.Menu; }
            }
        }
    }
}
