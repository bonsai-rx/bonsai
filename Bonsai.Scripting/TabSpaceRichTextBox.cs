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
        const string Tab = "  ";

        protected override bool ProcessCmdKey(ref Message m, Keys keyData)
        {
            if (keyData == Keys.Tab)
            {
                SelectionLength = 0;
                var selectionStart = SelectionStart;
                SelectionStart = GetFirstCharIndexOfCurrentLine();
                SelectedText = Tab;
                SelectionStart = selectionStart + Tab.Length;
                return true;
            }

            if (keyData == (Keys.Shift | Keys.Tab))
            {
                var lineIndex = GetLineFromCharIndex(SelectionStart);
                var line = Lines[lineIndex];
                if (!string.IsNullOrEmpty(line) && line.StartsWith(Tab))
                {
                    var selectionStart = SelectionStart;
                    SelectionStart = GetFirstCharIndexOfCurrentLine();
                    SelectionLength = Tab.Length;
                    SelectedText = string.Empty;
                    SelectionStart = Math.Max(0, selectionStart - 2);
                }

                return true;
            }

            return base.ProcessCmdKey(ref m, keyData);
        }
    }
}
