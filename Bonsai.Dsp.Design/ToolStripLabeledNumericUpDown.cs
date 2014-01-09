using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Bonsai.Dsp.Design
{
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.MenuStrip |
                                       ToolStripItemDesignerAvailability.ToolStrip |
                                       ToolStripItemDesignerAvailability.ContextMenuStrip)]
    public class ToolStripLabeledNumericUpDown : ToolStripControlHost
    {
        public ToolStripLabeledNumericUpDown()
            : base(new LabeledNumericUpDown())
        {
        }

        LabeledNumericUpDown UpDown
        {
            get { return (LabeledNumericUpDown)Control; }
        }

        public override string Text
        {
            get { return UpDown.Text; }
            set
            {
                UpDown.Text = value;
                OnTextChanged(EventArgs.Empty);
            }
        }

        public decimal Value
        {
            get { return UpDown.Value; }
            set { UpDown.Value = value; }
        }

        public decimal Minimum
        {
            get { return UpDown.Minimum; }
            set { UpDown.Minimum = value; }
        }

        public decimal Maximum
        {
            get { return UpDown.Maximum; }
            set { UpDown.Maximum = value; }
        }

        public decimal Increment
        {
            get { return UpDown.Increment; }
            set { UpDown.Increment = value; }
        }

        public int DecimalPlaces
        {
            get { return UpDown.DecimalPlaces; }
            set { UpDown.DecimalPlaces = value; }
        }

        public event EventHandler ValueChanged
        {
            add { UpDown.ValueChanged += value; }
            remove { UpDown.ValueChanged -= value; }
        }
    }
}
