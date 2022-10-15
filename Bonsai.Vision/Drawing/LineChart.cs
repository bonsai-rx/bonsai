using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Vision.Drawing
{
    /// <summary>
    /// Represents an operator that specifies drawing a line chart by plotting
    /// each row of a matrix as a polyline element.
    /// </summary>
    [ResetCombinator]
    [Description("Draws a line chart by plotting each row of a matrix as a polyline element.")]
    public class LineChart : CanvasElement
    {
        const string SeriesCategory = "Series";
        static readonly Action<IplImage> EmptyRenderer = image => { };

        /// <summary>
        /// Gets or sets the matrix specifying the data content of the line chart.
        /// </summary>
        [XmlIgnore]
        [Category(SeriesCategory)]
        [Description("The matrix specifying the data content of the line chart.")]
        public Mat Data { get; set; }

        /// <summary>
        /// Gets or sets the lower bound of the data range.
        /// </summary>
        [Category(SeriesCategory)]
        [Description("The lower bound of the data range.")]
        public double Min { get; set; }

        /// <summary>
        /// Gets or sets the upper bound of the data range.
        /// </summary>
        [Category(SeriesCategory)]
        [Description("The upper bound of the data range.")]
        public double Max { get; set; }

        /// <summary>
        /// Gets or sets the optional region in which to draw the chart.
        /// By default the chart will cover the entire image.
        /// </summary>
        [Description("The optional region in which to draw the chart. By default the chart will cover the entire image.")]
        public Rect Destination { get; set; }

        /// <summary>
        /// Gets or sets the color of the chart lines.
        /// </summary>
        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the chart lines.")]
        public Scalar Color { get; set; } = Scalar.All(255);

        /// <summary>
        /// Gets or sets the thickness of the chart lines.
        /// </summary>
        [Description("The thickness of the chart lines.")]
        public int Thickness { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value specifying the line drawing algorithm used to
        /// draw the chart lines.
        /// </summary>
        [Description("Specifies the line drawing algorithm used to draw the chart lines.")]
        public LineFlags LineType { get; set; } = LineFlags.Connected8;

        /// <summary>
        /// Returns the line chart drawing operation.
        /// </summary>
        /// <inheritdoc/>
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
