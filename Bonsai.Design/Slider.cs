using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Globalization;

namespace Bonsai.Design
{
    /// <summary>
    /// Represents a slider control used to select values from a continuous range.
    /// </summary>
    public partial class Slider : UserControl
    {
        const int TrackBarPadding = 16;
        int? decimalPlaces;
        double minimum;
        double maximum;
        double value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Slider"/> class.
        /// </summary>
        public Slider()
        {
            maximum = 100;
            InitializeComponent();
            trackBar.Top = -10;
        }

        /// <summary>
        /// Gets or sets the type converter used to convert the slider value to a text representation.
        /// </summary>
        public TypeConverter Converter { get; set; }

        /// <summary>
        /// Gets or sets the upper limit of values in the slider.
        /// </summary>
        public double Minimum
        {
            get { return minimum; }
            set { minimum = value; }
        }

        /// <summary>
        /// Gets or sets the lower limit of values in the slider.
        /// </summary>
        public double Maximum
        {
            get { return maximum; }
            set { maximum = value; }
        }

        /// <summary>
        /// Gets or sets an optional maximum number of decimal places used
        /// when formatting the numeric display of the slider.
        /// </summary>
        public int? DecimalPlaces
        {
            get { return decimalPlaces; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                decimalPlaces = value;
            }
        }

        /// <summary>
        /// Gets or sets a numeric value which represents the position of the slider.
        /// </summary>
        public double Value
        {
            get { return value; }
            set
            {
                if (minimum >= maximum)
                {
                    throw new InvalidOperationException("The slider range is invalid. Minimum value is greater than or equal to maximum.");
                }

                UpdateValue(value);
                trackBar.Value = (int)(trackBar.Maximum * (this.value - minimum) / (maximum - minimum));
            }
        }

        /// <summary>
        /// Occurs when the slider value changes.
        /// </summary>
        public event EventHandler ValueChanged
        {
            add { trackBar.ValueChanged += value; }
            remove { trackBar.ValueChanged -= value; }
        }

        private void UpdateTrackBarRange()
        {
            trackBar.Minimum = 0;
            trackBar.Maximum = Width - TrackBarPadding;
            Value = value;
        }

        private void UpdateValue(double value)
        {
            if (decimalPlaces.HasValue) value = Math.Round(value, decimalPlaces.Value);
            value = Math.Max(minimum, Math.Min(value, maximum));
            if (Converter != null) valueLabel.Text = Converter.ConvertToInvariantString(value);
            else valueLabel.Text = value.ToString(CultureInfo.InvariantCulture);
            this.value = value;
        }

        /// <inheritdoc/>
        protected override void OnSizeChanged(EventArgs e)
        {
            UpdateTrackBarRange();
            base.OnSizeChanged(e);
        }

        private void trackBar_Scroll(object sender, EventArgs e)
        {
            UpdateValue(minimum + (maximum - minimum) * trackBar.Value / (double)trackBar.Maximum);
        }
    }
}
