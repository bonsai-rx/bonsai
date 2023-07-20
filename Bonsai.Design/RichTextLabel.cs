using System.Windows.Forms;

namespace Bonsai.Design
{
    internal class RichTextLabel : RichTextBox
    {
        public RichTextLabel()
        {
            ReadOnly = true;
            TabStop = false;
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.UserMouse, true);
        }
    }
}
