using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Bonsai.Design
{
    public partial class StateTimeHistogram : UserControl
    {
        public StateTimeHistogram()
        {
            InitializeComponent();
            chart.ChartAreas[0].AxisX.Interval = 1;
            Dock = DockStyle.Fill;
        }

        public SeriesCollection StateTimes
        {
            get { return chart.Series; }
        }

        public ChartAreaCollection ChartAreas
        {
            get { return chart.ChartAreas; }
        }
    }
}
