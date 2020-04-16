using System.Drawing;

namespace Bonsai.Editor.Themes
{
    class DarkColorTable : MapColorTable
    {
        public DarkColorTable()
            : base(new AntiColorTable(), ThemeHelper.Invert)
        {
        }

        public override Color InactiveCaption
        {
            get { return Color.Gray; }
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
                get { return Color.FromArgb(225, 225, 225); }
            }
        }
    }
}
