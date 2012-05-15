using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Design;
using System.Windows.Forms.DataVisualization.Charting;
using System.Linq.Expressions;

[assembly: TypeVisualizer(typeof(NumericTupleVisualizer), Target = typeof(Tuple<int, int>))]
[assembly: TypeVisualizer(typeof(NumericTupleVisualizer), Target = typeof(Tuple<int, float>))]
[assembly: TypeVisualizer(typeof(NumericTupleVisualizer), Target = typeof(Tuple<int, double>))]
[assembly: TypeVisualizer(typeof(NumericTupleVisualizer), Target = typeof(Tuple<float, float>))]
[assembly: TypeVisualizer(typeof(NumericTupleVisualizer), Target = typeof(Tuple<float, int>))]
[assembly: TypeVisualizer(typeof(NumericTupleVisualizer), Target = typeof(Tuple<float, double>))]
[assembly: TypeVisualizer(typeof(NumericTupleVisualizer), Target = typeof(Tuple<double, double>))]
[assembly: TypeVisualizer(typeof(NumericTupleVisualizer), Target = typeof(Tuple<double, int>))]
[assembly: TypeVisualizer(typeof(NumericTupleVisualizer), Target = typeof(Tuple<double, float>))]

namespace Bonsai.Design
{
    public class NumericTupleVisualizer : DialogTypeVisualizer
    {
        int numSeries;
        PlotControl chart;
        Delegate showDelegate;

        public NumericTupleVisualizer()
            : this(1)
        {
        }

        public NumericTupleVisualizer(int numSeries)
        {
            this.numSeries = numSeries;
        }

        protected PlotControl Chart
        {
            get { return chart; }
        }

        protected void AddValue(object xValue, object yValue)
        {
            AddValue(xValue, new[] { yValue });
        }

        protected void AddValue(object xValue, params object[] yValue)
        {
            for (int i = 0; i < yValue.Length; i++)
            {
                chart.Series[i].Points.AddXY(xValue, yValue[i]);
            }
        }

        public override void Show(object value)
        {
            if (value != null)
            {
                if (showDelegate == null)
                {
                    var visualizerInstance = Expression.Constant(this);
                    var valueExpression = Expression.Parameter(value.GetType());
                    var item1Expression = Expression.Convert(Expression.Property(valueExpression, "Item1"), typeof(object));
                    var item2Expression = Expression.Convert(Expression.Property(valueExpression, "Item2"), typeof(object));
                    var addValueExpression = Expression.Call(visualizerInstance, "AddValue", null, item1Expression, item2Expression);
                    var addValueLambda = Expression.Lambda(addValueExpression, valueExpression);
                    showDelegate = addValueLambda.Compile();
                }

                showDelegate.DynamicInvoke(value);
            }
        }

        public override void Load(IServiceProvider provider)
        {
            chart = new PlotControl();
            for (int i = 1; i < numSeries; i++)
            {
                var series = chart.Series.Add(chart.Series[0].Name + i);
                series.ChartType = chart.Series[0].ChartType;
                series.ChartArea = chart.Series[0].ChartArea;
            }

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(chart);
            }
        }

        public override void Unload()
        {
            showDelegate = null;
            chart.Dispose();
            chart = null;
        }
    }
}
