using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Dsp.Design
{
    partial class LabeledNumericUpDown : UserControl
    {
        public LabeledNumericUpDown()
        {
            InitializeComponent();
        }

        public override string Text
        {
            get { return label.Text; }
            set
            {
                label.Text = value;
                OnTextChanged(EventArgs.Empty);
            }
        }

        public decimal Value
        {
            get { return numericUpDown.Value; }
            set { numericUpDown.Value = value; }
        }

        public decimal Minimum
        {
            get { return numericUpDown.Minimum; }
            set { numericUpDown.Minimum = value; }
        }

        public decimal Maximum
        {
            get { return numericUpDown.Maximum; }
            set { numericUpDown.Maximum = value; }
        }

        public decimal Increment
        {
            get { return numericUpDown.Increment; }
            set { numericUpDown.Increment = value; }
        }

        public int DecimalPlaces
        {
            get { return numericUpDown.DecimalPlaces; }
            set { numericUpDown.DecimalPlaces = value; }
        }

        public event EventHandler ValueChanged
        {
            add { numericUpDown.ValueChanged += value; }
            remove { numericUpDown.ValueChanged -= value; }
        }
    }
}
