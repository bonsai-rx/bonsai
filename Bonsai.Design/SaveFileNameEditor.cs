using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace Bonsai.Design
{
    public class SaveFileNameEditor : FileNameEditor
    {
        protected override FileDialog CreateFileDialog()
        {
            return new SaveFileDialog();
        }
    }
}
