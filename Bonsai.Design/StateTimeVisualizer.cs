using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Design;
using System.Windows.Forms.DataVisualization.Charting;

namespace Bonsai.Design
{
    public class StateTimeVisualizer : DialogTypeVisualizer
    {
        const int StateColumns = 10;
        StateTimeHistogram chart;
        List<object> valuesX;
        List<double> valuesY;

        object state;
        DateTime stateEnter;

        public StateTimeVisualizer()
        {
            valuesX = new List<object>();
            valuesY = new List<double>();
        }

        protected StateTimeHistogram Chart
        {
            get { return chart; }
        }

        protected void AddValue(DateTime time, object value)
        {
            if (value == null) return;
            if (valuesY.Count > 0)
            {
                var diff = time - stateEnter;
                valuesY[valuesY.Count - 1] = diff.TotalSeconds;
            }

            if (!value.Equals(state))
            {
                state = value;
                stateEnter = time;
                var excess = valuesX.Count - StateColumns;
                if (excess > 0)
                {
                    valuesX.RemoveRange(0, excess);
                    valuesY.RemoveRange(0, excess);
                }

                valuesX.Add(state.ToString());
                valuesY.Add(0);
            }

            chart.StateTimes[0].Points.DataBindXY(valuesX, valuesY);
        }

        public override void Show(object value)
        {
            AddValue(DateTime.Now, value);
        }

        public override void Load(IServiceProvider provider)
        {
            chart = new StateTimeHistogram();

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(chart);
            }
        }

        public override void Unload()
        {
            valuesX.Clear();
            valuesY.Clear();
            state = null;

            chart.Dispose();
            chart = null;
        }
    }
}
