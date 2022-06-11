using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Bonsai.Design.Visualizers
{
    class ToolStripEditableLabel : IComponent
    {
        readonly ToolStripTextBox textBox;
        readonly ToolStripStatusLabel valueLabel;
        readonly Action<string> onEdit;
        bool disposed;

        public ToolStripEditableLabel(ToolStripStatusLabel statusLabel, Action<string> onLabelEdit)
        {
            if (statusLabel is null)
            {
                throw new ArgumentNullException(nameof(statusLabel));
            }

            onEdit = onLabelEdit;
            valueLabel = statusLabel;
            textBox = new ToolStripTextBox();
            textBox.LostFocus += textBox_LostFocus;
            textBox.KeyDown += textBox_KeyDown;
            valueLabel.Click += valueLabel_Click;
        }

        public bool Enabled { get; set; } = true;

        public ISite Site { get; set; }

        public event EventHandler Disposed;

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                e.SuppressKeyPress = true;
                if (textBox.Owner is StatusStrip statusStrip)
                {
                    statusStrip.Select();
                }
            }
        }

        private void textBox_LostFocus(object sender, EventArgs e)
        {
            if (textBox.Text != valueLabel.Text && onEdit != null)
            {
                onEdit(textBox.Text);
            }

            if (textBox.Owner is StatusStrip statusStrip)
            {
                var labelIndex = statusStrip.Items.IndexOf(textBox);
                statusStrip.SuspendLayout();
                statusStrip.Items.Remove(textBox);
                statusStrip.Items.Insert(labelIndex, valueLabel);
                statusStrip.ResumeLayout();
            }
        }

        private void valueLabel_Click(object sender, EventArgs e)
        {
            if (Enabled && valueLabel.Owner is StatusStrip statusStrip)
            {
                var labelIndex = statusStrip.Items.IndexOf(valueLabel);
                statusStrip.SuspendLayout();
                statusStrip.Items.Remove(valueLabel);
                statusStrip.Items.Insert(labelIndex, textBox);
                textBox.Size = valueLabel.Size;
                textBox.Text = valueLabel.Text;
                statusStrip.ResumeLayout();
                textBox.Focus();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    textBox.Dispose();
                    Disposed?.Invoke(this, EventArgs.Empty);
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
