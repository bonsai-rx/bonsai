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
        const int TrackBarPadding = 16;
        int decimalPlaces;
        string format;

        public Slider()
        {
            InitializeComponent();
        }

        public double Minimum { get; set; }

        public double Maximum { get; set; }

        public int DecimalPlaces
        {
            get { return decimalPlaces; }
            set
            {
                decimalPlaces = value;
                format = string.Format("F{0}", DecimalPlaces);
            }
        }

        public double Value
        {
            get { return Minimum + (Maximum - Minimum) * trackBar.Value / (double)trackBar.Maximum; }
            set
            {
                if (value > Maximum || value < Minimum)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                trackBar.Value = (int)(trackBar.Maximum * (value - Minimum) / (Maximum - Minimum));
            }
        }

        public event EventHandler ValueChanged
        {
            add { trackBar.ValueChanged += value; }
            remove { trackBar.ValueChanged -= value; }
        }

        private void UpdateTrackBarRange()
        {
            var value = trackBar.Value / (double)trackBar.Maximum;
            trackBar.Minimum = 0;
            trackBar.Maximum = Width - TrackBarPadding;
            trackBar.Value = (int)value * trackBar.Maximum;
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

        protected override void OnSizeChanged(EventArgs e)
        {
            UpdateTrackBarRange();
            base.OnSizeChanged(e);
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            UpdateValueLabel();
        }
    }
}
