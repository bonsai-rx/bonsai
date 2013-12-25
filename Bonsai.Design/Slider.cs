using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace Bonsai.Design
{
    public partial class Slider : UserControl
    {
        string format;
        double multiplier = 1;
        double minimum = 0;
        double maximum = 100;
        int decimals;

        public Slider()
        {
            InitializeComponent();
        }

        public double Minimum
        {
            get { return minimum; }
            set
            {
                minimum = value;
                UpdateTrackBarRange();
            }
        }

        public double Maximum
        {
            get { return maximum; }
            set
            {
                maximum = value;
                UpdateTrackBarRange();
            }
        }

        public int Decimals
        {
            get { return decimals; }
            set
            {
                decimals = value;
                UpdateTrackBarRange();
            }
        }

        public double Value
        {
            get { return trackBar.Value / multiplier; }
            set { trackBar.Value = (int)(Math.Max(minimum, Math.Min(value, maximum)) * multiplier); }
        }

        public event EventHandler Scroll
        {
            add { trackBar.Scroll += value; }
            remove { trackBar.Scroll -= value; }
        }

        public event EventHandler ValueChanged
        {
            add { trackBar.ValueChanged += value; }
            remove { trackBar.ValueChanged -= value; }
        }

        private void UpdateTrackBarRange()
        {
            var value = Value;
            multiplier = Math.Pow(10, Decimals);
            format = string.Format("F{0}", Decimals);
            trackBar.Minimum = Math.Max(int.MinValue, (int)(minimum * multiplier));
            trackBar.Maximum = Math.Min(int.MaxValue, (int)(maximum * multiplier));
            Value = value;
            UpdateValueLabel();
        }

        private void UpdateValueLabel()
        {
            valueLabel.Text = Value.ToString(format, CultureInfo.InvariantCulture);
        }

        protected override void OnLoad(EventArgs e)
        {
            UpdateTrackBarRange();
            base.OnLoad(e);
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            UpdateValueLabel();
        }
    }
}
