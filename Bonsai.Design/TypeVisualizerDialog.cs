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
            if (control.MinimumSize != Size.Empty)
            {
                MinimumSize = new Size(
                    control.MinimumSize.Width + Width - control.Width,
                    control.MinimumSize.Height + Height - control.Height);
            }
            Controls.Add(control);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && !e.Handled)
            {
                Close();
                e.Handled = true;
            }

            if (e.KeyCode == Keys.F11)
            {
                if (FormBorderStyle == FormBorderStyle.None) FormBorderStyle = FormBorderStyle.Sizable;
                else FormBorderStyle = FormBorderStyle.None;
            }
            base.OnKeyDown(e);
        }
    }
}
