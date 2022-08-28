using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Vision.Drawing
{
    /// <summary>
    /// Represents an operator that specifies drawing a path from an array of vertices.
    /// </summary>
    [Description("Draws a path from an array of vertices.")]
    public class Path : CanvasElement
    {
        /// <summary>
        /// Gets or sets the array of vertices specifying the path.
        /// NaN values will not be connected or drawn.
        /// </summary>
        [XmlIgnore]
        [Description("The array of vertices specifying the path. NaN values will not be connected or drawn.")]
        public Point2f[] Points { get; set; }

        /// <summary>
        /// Gets or sets the color of the polylines connecting the path.
        /// </summary>
        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the polylines connecting the path.")]
        public Scalar Color { get; set; } = Scalar.All(255);

        /// <summary>
        /// Gets or sets the thickness of the polyline edges.
        /// </summary>
        [Description("The thickness of the polyline edges.")]
        public int Thickness { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value specifying the line drawing algorithm used to
        /// draw the polylines.
        /// </summary>
        [Description("Specifies the line drawing algorithm used to draw the polylines.")]
        public LineFlags LineType { get; set; } = LineFlags.Connected8;

        /// <summary>
        /// Gets or sets the number of fractional bits in the vertex coordinates.
        /// </summary>
        [Description("The number of fractional bits in the vertex coordinates.")]
        public int Shift { get; set; }

        static Point[][] GetPolyLine(Point2f[] path, int shift)
        {
            if (path == null) return null;
            var buffer = new Point[path.Length];
            var polyLine = new List<Point[]>();
            for (int i = 0, k = 0; i < path.Length; i++)
            {
                if (!float.IsNaN(path[i].X) && !float.IsNaN(path[i].Y))
                {
                    buffer[k++] = new Point(
                        (int)(path[i].X * (1 << shift)),
                        (int)(path[i].Y * (1 << shift)));
                }
                else if (k > 0)
                {
                    var segment = new Point[k];
                    Array.Copy(buffer, 0, segment, 0, segment.Length);
                    polyLine.Add(segment);
                    k = 0;
                }

                if (i == path.Length - 1 && k > 0)
                {
                    Array.Resize(ref buffer, k);
                    polyLine.Add(buffer);
                }
            }

            return polyLine.ToArray();
        }

        /// <summary>
        /// Returns the path drawing operation.
        /// </summary>
        /// <inheritdoc/>
        protected override Action<IplImage> GetRenderer()
        {
            var color = Color;
            var thickness = Thickness;
            var lineType = LineType;
            var shift = Shift;
            var points = GetPolyLine(Points, shift);
            return image =>
            {
                if (points != null && points.Length > 0)
                {
                    CV.PolyLine(image, points, false, color, thickness, lineType, shift);
                }
            };
        }
    }
}
