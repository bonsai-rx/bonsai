using System.Collections.Generic;
using System.Drawing;
using ZedGraph;

namespace Bonsai.Design.Visualizers
{
    class BarGraph : RollingGraph
    {
        public BarBase BaseAxis
        {
            get { return GraphPane.BarSettings.Base; }
            set { GraphPane.BarSettings.Base = value; }
        }

        public BarType BarType
        {
            get { return GraphPane.BarSettings.Type; }
            set { GraphPane.BarSettings.Type = value; }
        }

        internal override CurveItem CreateSeries(string label, IPointListEdit points, Color color)
        {
            var curve = new BarItem(label, points, color);
            curve.Label.IsVisible = !string.IsNullOrEmpty(label);
            curve.Bar.Fill.Type = FillType.Solid;
            curve.Bar.Border.IsVisible = false;
            return curve;
        }

        static int FindIndex(IPointListEdit series, string index)
        {
            for (int i = 0; i < series.Count; i++)
            {
                if (EqualityComparer<string>.Default.Equals(index, (string)series[i].Tag))
                {
                    return i;
                }
            }

            return -1;
        }

        public void AddValues(string index, double[] values)
        {
            if (values.Length > 0)
            {
                var updateIndex = FindIndex(Series[0], index);
                if (updateIndex >= 0 && BaseAxis <= BarBase.X2) UpdateLastBaseX();
                else if (updateIndex >= 0) UpdateLastBaseY();
                else if (BaseAxis <= BarBase.X2) AddBaseX();
                else AddBaseY();

                void UpdateLastBaseX()
                {
                    for (int i = 0; i < Series.Length; i++)
                        Series[i][updateIndex].Y = values[i];
                }

                void UpdateLastBaseY()
                {
                    for (int i = 0; i < Series.Length; i++)
                        Series[i][updateIndex].X = values[i];
                }

                void AddBaseX()
                {
                    for (int i = 0; i < Series.Length; i++)
                        Series[i].Add(new PointPair(0, values[i], index));
                }

                void AddBaseY()
                {
                    for (int i = 0; i < Series.Length; i++)
                        Series[i].Add(new PointPair(values[i], 0, index));
                }
            }
        }
    }
}
