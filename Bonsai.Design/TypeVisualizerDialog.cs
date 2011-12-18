using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bonsai.Design
{
    public partial class TypeVisualizerDialog : Form, IDialogTypeVisualizerService
    {
        public TypeVisualizerDialog()
        {
            InitializeComponent();
        }

        public void AddControl(Control control)
        {
            ClientSize = control.Size;
            Controls.Add(control);
        }
    }
}
