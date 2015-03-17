using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Scripting
{
    class TabSpaceRichTextBox : RichTextBox
    {
        protected override bool ProcessCmdKey(ref Message m, Keys keyData)
        {
            if (keyData == Keys.Tab)
            {
                SelectionLength = 0;
                SelectedText = new string(' ', 2);
                return true;
            }

            return base.ProcessCmdKey(ref m, keyData);
        }
    }
}
