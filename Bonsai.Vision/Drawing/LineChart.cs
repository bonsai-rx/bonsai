using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Vision.Drawing
{
    [Description("Draws a line chart by plotting each row of a matrix as a data series.")]
    public class LineChart : CanvasElement
    {
        static readonly Action<IplImage> EmptyRenderer = image => { };

        public LineChart()
        {
            Thickness = 1;
            Color = Scalar.All(255);
            LineType = LineFlags.Connected8;
        }

        [XmlIgnore]
        [Category("Series")]
        [Description("The matrix specifying the data content of the line chart.")]
        public Mat Data { get; set; }

        [Category("Series")]
        [Description("The minimum value of the data range.")]
        public double Min { get; set; }

        [Category("Series")]
        [Description("The maximum value of the data range.")]
        public double Max { get; set; }

        [Description("The optional region in which to draw the chart. By default the chart will cover the entire image.")]
        public Rect Destination { get; set; }

        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the chart lines.")]
        public Scalar Color { get; set; }

        [Description("The thickness of the chart lines.")]
        public int Thickness { get; set; }

        [Description("The algorithm used to draw the chart lines.")]
        public LineFlags LineType { get; set; }

        protected override Action<IplImage> GetRenderer()
        {
            var data = Data;
            if (data == null) return EmptyRenderer;

            var min = Min;
            var max = Max;
            var dataRange = max - min;
            if (dataRange == 0)
            {
                CV.MinMaxLoc(data, out min, out max);
                dataRange = max - min;
                if (dataRange == 0)
                {
                    max = 1;
                    min = -1;
                }
            }

            var color = Color;
            var rect = Destination;
            return image =>
            {
                if (rect.Width == 0) rect.Width = image.Width;
                if (rect.Height == 0) rect.Height = image.Height;
                var top = rect.Y;
                var bottom = rect.Y + rect.Height;
                var scaleY = (top - bottom)  / (max - min);
                var shiftY = -min * scaleY + bottom;
                var rangeWidth = data.Cols * ((rect.Width - 1) / (double)(data.Cols - 1));
                var points = new Point[data.Rows][];
                for (int i = 0; i < data.Rows; i++)
                {
                    points[i] = new Point[data.Cols];
                    var row = data.Rows > 1 ? data.GetRow(i) : data;
                    try
                    {
                        using (var rowT = row.Reshape(1, data.Cols))
                        using (var pointHeader = Mat.CreateMatHeader(points[i], data.Cols, 2, Depth.S32, 1))
                        using (var xAxis = pointHeader.GetCol(0))
                        using (var yAxis = pointHeader.GetCol(1))
                        {
                            CV.Range(xAxis, rect.X, rect.X + rangeWidth);
                            CV.ConvertScale(rowT, yAxis, scaleY, shiftY);
                        }
                    }
                    finally
                    {
                        if (row != data)
                        {
                            row.Dispose();
                        }
                    }
                }

                CV.PolyLine(image, points, false, Color, Thickness, LineType);
            };
        }
    }
}
