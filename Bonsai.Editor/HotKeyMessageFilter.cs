using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Editor
{
    class HotKeyMessageFilter : IMessageFilter
    {
        const int WM_KEYDOWN = 0x100;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_KEYDOWN)
            {
                var keyCode = (Keys)m.WParam;
                if (keyCode == Keys.Tab && Form.ModifierKeys.HasFlag(Keys.Control))
                {
                    if (Form.ModifierKeys.HasFlag(Keys.Shift)) CycleActiveForm(-1);
                    else CycleActiveForm(1);
                }
            }

            return false;
        }

        static void CycleActiveForm(int step)
        {
            var activeForm = Form.ActiveForm;
            if (activeForm != null && !activeForm.Modal && Application.OpenForms.Count > 1)
            {
                for (int i = 0; i < Application.OpenForms.Count; i++)
                {
                    var form = Application.OpenForms[i];
                    if (form == activeForm)
                    {
                        i = (i + Application.OpenForms.Count + step) % Application.OpenForms.Count;
                        activeForm = Application.OpenForms[i];
                        activeForm.Activate();
                        break;
                    }
                }
            }
        }
    }
}
