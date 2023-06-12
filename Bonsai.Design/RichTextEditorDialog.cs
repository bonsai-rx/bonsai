using System;
using System.Windows.Forms;

namespace Bonsai.Design
{
    internal partial class RichTextEditorDialog : Form
    {
        public RichTextEditorDialog()
        {
            InitializeComponent();
        }

        public string Value { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            textBox.Text = Value;
            if (Owner != null)
            {
                Icon = Owner.Icon;
                ShowIcon = true;
            }

            base.OnLoad(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && !e.Handled)
            {
                Close();
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.Control)
            {
                okButton.PerformClick();
            }
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            Value = textBox.Text;
        }
    }
}
