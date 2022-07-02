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

        public void AddValues(string index, double[] values)
        {
            if (values.Length > 0)
            {
                var count = Series[0].Count;
                var updateLast = count > 0 && index.Equals(Series[0][count - 1].Tag);
                if (updateLast && BaseAxis <= BarBase.X2) UpdateLastBaseX();
                else if (updateLast) UpdateLastBaseY();
                else if (BaseAxis <= BarBase.X2) AddBaseX();
                else AddBaseY();

                void UpdateLastBaseX()
                {
                    for (int i = 0; i < Series.Length; i++)
                        Series[i][count - 1].Y = values[i];
                }

                void UpdateLastBaseY()
                {
                    for (int i = 0; i < Series.Length; i++)
                        Series[i][count - 1].X = values[i];
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
