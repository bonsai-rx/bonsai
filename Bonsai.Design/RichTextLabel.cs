using System.Windows.Forms;

namespace Bonsai.Design
{
    internal class RichTextLabel : RichTextBox
    {
        const int WM_RBUTTONDOWN = 0x204;
        const int WM_RBUTTONUP = 0x205;
        const int WM_RBUTTONDBLCLK = 0x206;

        public RichTextLabel()
        {
            ReadOnly = true;
            TabStop = false;
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.UserMouse, true);
        }

        static MouseEventArgs CreateMouseEventArgs(MouseButtons button, int clicks = 1)
        {
            var mousePosition = MousePosition;
            return new MouseEventArgs(
                button,
                clicks,
                mousePosition.X,
                mousePosition.Y,
                delta: 0);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_RBUTTONDOWN:
                    OnMouseDown(CreateMouseEventArgs(MouseButtons.Right));
                    break;
                case WM_RBUTTONUP:
                    OnMouseUp(CreateMouseEventArgs(MouseButtons.Right));
                    break;
                case WM_RBUTTONDBLCLK:
                    OnMouseDoubleClick(CreateMouseEventArgs(MouseButtons.Right, clicks: 2));
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }
}
