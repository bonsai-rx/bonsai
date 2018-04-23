using SvgNet;
using SvgNet.SvgElements;
using SvgNet.SvgGdi;
using SvgNet.SvgTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Bonsai.Design
{
    delegate void SvgRenderer(SvgRendererState state, IGraphics graphics);

    class SvgRendererState
    {
        public float Scale;
        public PointF Translation;
        public Pen Outlining;
    }

    class SvgRendererFactory : IDisposable
    {
        bool disposed;
        readonly List<IDisposable> disposableResources = new List<IDisposable>();
        readonly Dictionary<string, SvgRenderer> rendererCache = new Dictionary<string, SvgRenderer>();

        static float ParseFloat(SvgElement element, string attribute)
        {
            return float.Parse((string)element.Attributes[attribute], CultureInfo.InvariantCulture);
        }

        static SvgPath ParsePath(SvgElement element, string attribute)
        {
            return (SvgPath)(string)element.Attributes[attribute];
        }

        [DebuggerDisplay("Fill = {Fill}, Stroke = {Stroke}")]
        struct SvgDrawingStyle
        {
            public Color? Fill;
            public Color? Stroke;
            public SvgLength StrokeWidth;
        }

        static SvgDrawingStyle ParseStyle(SvgElement element, string attribute)
        {
            SvgDrawingStyle result;
            var value = element.Attributes[attribute];
            var style = value as SvgStyle;
            if (style == null)
            {
                var rawStyle = value as string;
                if (rawStyle == null)
                {
                    result.Fill = null;
                    result.Stroke = null;
                    result.StrokeWidth = 0;
                    return result;
                }

                style = (SvgStyle)rawStyle;
            }

            var fill = (string)style.Get("fill");
            var stroke = (string)style.Get("stroke");
            var strokeWidth = (string)style.Get("stroke-width");
            result.Fill = fill == null || fill == "none" ? default(Color?) : ((SvgColor)fill).Color;
            result.Stroke = stroke == null || stroke == "none" ? default(Color?) : ((SvgColor)stroke).Color;
            result.StrokeWidth = strokeWidth == null ? 0 : (SvgLength)strokeWidth;
            return result;
        }

        Matrix ParseTransform(SvgElement element, Matrix parent)
        {
            var result = new Matrix();
            disposableResources.Add(result);
            var transformList = (SvgTransformList)element.Attributes["transform"];
            if (transformList != null)
            {
                for (int i = 0; i < transformList.Count; i++)
                {
                    var transform = transformList[i];
                    result.Multiply(transform.Matrix);
                }
            }
            if (parent != null) result.Multiply(parent, MatrixOrder.Append);
            return result;
        }

        static Expression CreateFloat(SvgElement element, string attribute)
        {
            var value = ParseFloat(element, attribute);
            return Expression.Constant(value, typeof(float));
        }

        Expression CreateTransform(SvgElement element, Matrix parent)
        {
            var transform = ParseTransform(element, parent);
            return Expression.Constant(transform);
        }

        Expression CreateBrush(Color color)
        {
            var brush = new SolidBrush(color);
            disposableResources.Add(brush);
            return Expression.Constant(brush);
        }

        Expression CreatePen(Color color, SvgLength width, Expression state)
        {
            if (width.Value == 0)
            {
                return Expression.PropertyOrField(state, "Outlining");
            }
            else
            {
                var pen = new Pen(color, width.Value);
                disposableResources.Add(pen);
                return Expression.Constant(pen);
            }
        }

        void CreateDrawTransform(SvgElement element, Matrix transform, Expression state, Expression graphics, List<Expression> expressions)
        {
            var localTransform = CreateTransform(element, transform);
            var scale = Expression.PropertyOrField(state, "Scale");
            var translation = Expression.PropertyOrField(state, "Translation");
            var offsetX = Expression.PropertyOrField(translation, "X");
            var offsetY = Expression.PropertyOrField(translation, "Y");
            expressions.Add(Expression.Call(graphics, "TranslateTransform", null, offsetX, offsetY));
            expressions.Add(Expression.Call(graphics, "ScaleTransform", null, scale, scale));
            expressions.Add(Expression.Call(graphics, "MultiplyTransform", null, localTransform));
        }

        void CreateResetTransform(Expression graphics, List<Expression> expressions)
        {
            expressions.Add(Expression.Call(graphics, "ResetTransform", null));
        }

        void CreateDrawRectangle(SvgElement element, Matrix transform, Expression state, Expression graphics, List<Expression> expressions)
        {
            var style = ParseStyle(element, "style");
            if (style.Fill.HasValue || style.Stroke.HasValue)
            {
                var x = CreateFloat(element, "x");
                var y = CreateFloat(element, "y");
                var width = CreateFloat(element, "width");
                var height = CreateFloat(element, "height");
                CreateDrawTransform(element, transform, state, graphics, expressions);
                if (style.Fill.HasValue)
                {
                    var brush = CreateBrush(style.Fill.Value);
                    expressions.Add(Expression.Call(graphics, "FillRectangle", null, brush, x, y, width, height));
                }
                if (style.Stroke.HasValue)
                {
                    var pen = CreatePen(style.Stroke.Value, style.StrokeWidth, state);
                    expressions.Add(Expression.Call(graphics, "DrawRectangle", null, pen, x, y, width, height));
                }
                CreateResetTransform(graphics, expressions);
            }
        }

        void CreateDrawCircle(SvgElement element, Matrix transform, Expression state, Expression graphics, List<Expression> expressions)
        {
            var style = ParseStyle(element, "style");
            if (style.Fill.HasValue || style.Stroke.HasValue)
            {
                var cx = CreateFloat(element, "cx");
                var cy = CreateFloat(element, "cy");
                var r = CreateFloat(element, "r");
                var x = Expression.Subtract(cx, r);
                var y = Expression.Subtract(cy, r);
                var d = Expression.Multiply(Expression.Constant(2f), r);
                CreateDrawTransform(element, transform, state, graphics, expressions);
                if (style.Fill.HasValue)
                {
                    var brush = CreateBrush(style.Fill.Value);
                    expressions.Add(Expression.Call(graphics, "FillEllipse", null, brush, x, y, d, d));
                }
                if (style.Stroke.HasValue)
                {
                    var pen = CreatePen(style.Stroke.Value, style.StrokeWidth, state);
                    expressions.Add(Expression.Call(graphics, "DrawEllipse", null, pen, x, y, d, d));
                }
                CreateResetTransform(graphics, expressions);
            }
        }

        void CreateDrawEllipse(SvgElement element, Matrix transform, Expression state, Expression graphics, List<Expression> expressions)
        {
            var style = ParseStyle(element, "style");
            if (style.Fill.HasValue || style.Stroke.HasValue)
            {
                var cx = CreateFloat(element, "cx");
                var cy = CreateFloat(element, "cy");
                var rx = CreateFloat(element, "rx");
                var ry = CreateFloat(element, "ry");
                var x = Expression.Subtract(cx, rx);
                var y = Expression.Subtract(cy, ry);
                var dx = Expression.Multiply(Expression.Constant(2f), rx);
                var dy = Expression.Multiply(Expression.Constant(2f), ry);
                CreateDrawTransform(element, transform, state, graphics, expressions);
                if (style.Fill.HasValue)
                {
                    var brush = CreateBrush(style.Fill.Value);
                    expressions.Add(Expression.Call(graphics, "FillEllipse", null, brush, x, y, dx, dy));
                }
                if (style.Stroke.HasValue)
                {
                    var pen = CreatePen(style.Stroke.Value, style.StrokeWidth, state);
                    expressions.Add(Expression.Call(graphics, "DrawEllipse", null, pen, x, y, dx, dy));
                }
                CreateResetTransform(graphics, expressions);
            }

        }

        static void AddLines(GraphicsPath path, List<PointF> points)
        {
            if (points.Count > 1)
            {
                path.AddLines(points.ToArray());
            }
            points.Clear();
        }

        static void AddBeziers(GraphicsPath path, List<PointF> points)
        {
            if (points.Count > 3)
            {
                path.AddBeziers(points.ToArray());
            }
            points.Clear();
        }

        static void StartLine(List<PointF> bezierPoints, List<PointF> linePoints, PathSeg segment, ref PointF point)
        {
            if (bezierPoints.Count > 0)
            {
                linePoints.Add(bezierPoints[bezierPoints.Count - 1]);
                point = segment.Abs ? default(PointF) : linePoints[0];
            }
        }

        static void AddBezierData(List<PointF> points, PathSeg segment)
        {
            var data = segment.Data;
            var offset = !segment.Abs && points.Count > 0 ? points[points.Count - 1] : PointF.Empty;
            for (int i = 0; i < data.Length / 2; i++)
            {
                points.Add(new PointF(
                    data[i * 2 + 0] + offset.X,
                    data[i * 2 + 1] + offset.Y));
            }
        }

        static void AddPathData(GraphicsPath path, SvgPath pathData)
        {
            var bezierPoints = new List<PointF>(pathData.Count);
            var linePoints = new List<PointF>(pathData.Count);
            for (int i = 0; i < pathData.Count; i++)
            {
                PointF point;
                var segment = pathData[i];
                if (linePoints.Count > 0 && (segment.Type == SvgPathSegType.SVG_SEGTYPE_CURVETO || !segment.Abs))
                {
                    point = linePoints[linePoints.Count - 1];
                }
                else point = default(PointF);
                switch (segment.Type)
                {
                    case SvgPathSegType.SVG_SEGTYPE_CURVETO:
                        AddLines(path, linePoints);
                        if (bezierPoints.Count == 0) bezierPoints.Add(point);
                        AddBezierData(bezierPoints, segment);
                        break;
                    case SvgPathSegType.SVG_SEGTYPE_LINETO:
                    case SvgPathSegType.SVG_SEGTYPE_MOVETO:
                        StartLine(bezierPoints, linePoints, segment, ref point);
                        AddBeziers(path, bezierPoints);
                        point.X += segment.Data[0];
                        point.Y += segment.Data[1];
                        linePoints.Add(point);
                        break;
                    case SvgPathSegType.SVG_SEGTYPE_HLINETO:
                        StartLine(bezierPoints, linePoints, segment, ref point);
                        AddBeziers(path, bezierPoints);
                        point.X += segment.Data[0];
                        if (segment.Abs) point.Y = linePoints[i - 1].Y;
                        linePoints.Add(point);
                        break;
                    case SvgPathSegType.SVG_SEGTYPE_VLINETO:
                        StartLine(bezierPoints, linePoints, segment, ref point);
                        AddBeziers(path, bezierPoints);
                        if (segment.Abs) point.X = linePoints[i - 1].X;
                        point.Y += segment.Data[0];
                        linePoints.Add(point);
                        break;
                    case SvgPathSegType.SVG_SEGTYPE_CLOSEPATH:
                        AddLines(path, linePoints);
                        AddBeziers(path, bezierPoints);
                        path.CloseFigure();
                        break;
                }
            }

            AddLines(path, linePoints);
            AddBeziers(path, bezierPoints);
        }

        Expression CreatePath(SvgElement element, string attribute)
        {
            var pathData = ParsePath(element, attribute);
            var path = new GraphicsPath();
            disposableResources.Add(path);
            AddPathData(path, pathData);
            return Expression.Constant(path);
        }

        void CreateDrawPath(SvgElement element, Matrix transform, Expression state, Expression graphics, List<Expression> expressions)
        {
            var style = ParseStyle(element, "style");
            if (style.Fill.HasValue || style.Stroke.HasValue)
            {
                var path = CreatePath(element, "d");
                CreateDrawTransform(element, transform, state, graphics, expressions);
                if (style.Fill.HasValue)
                {
                    var brush = CreateBrush(style.Fill.Value);
                    expressions.Add(Expression.Call(graphics, "FillPath", null, brush, path));
                }
                if (style.Stroke.HasValue)
                {
                    var pen = CreatePen(style.Stroke.Value, style.StrokeWidth, state);
                    expressions.Add(Expression.Call(graphics, "DrawPath", null, pen, path));
                }
                CreateResetTransform(graphics, expressions);
            }
        }

        void CreateDrawChildren(SvgElement element, Matrix transform, Expression state, Expression graphics, List<Expression> expressions)
        {
            foreach (SvgElement child in element.Children)
            {
                CreateDrawBody(child, transform, state, graphics, expressions);
            }
        }

        void CreateDrawBody(SvgElement element, Matrix transform, Expression state, Expression graphics, List<Expression> expressions)
        {
            switch (element.Name)
            {
                case "rect": CreateDrawRectangle(element, transform, state, graphics, expressions); break;
                case "circle": CreateDrawCircle(element, transform, state, graphics, expressions); break;
                case "ellipse": CreateDrawEllipse(element, transform, state, graphics, expressions); break;
                case "path": CreateDrawPath(element, transform, state, graphics, expressions); break;
                case "svg": CreateDrawChildren(element, transform, state, graphics, expressions); break;
                case "g":
                    var localTransform = ParseTransform(element, transform);
                    CreateDrawChildren(element, localTransform, state, graphics, expressions);
                    break;
            }
        }

        SvgRenderer Create(SvgElement element)
        {
            var transform = new Matrix();
            var state = Expression.Parameter(typeof(SvgRendererState), "state");
            var graphics = Expression.Parameter(typeof(IGraphics), "graphics");
            var expressions = new List<Expression>();
            CreateDrawBody(element, transform, state, graphics, expressions);
            var body = Expression.Block(expressions);
            var renderer = Expression.Lambda<SvgRenderer>(body, state, graphics);
            return renderer.Compile();
        }

        public SvgRenderer GetIconRenderer(WorkflowIcon icon)
        {
            if (icon == null)
            {
                throw new ArgumentNullException("icon");
            }

            SvgRenderer renderer;
            if (!rendererCache.TryGetValue(icon.Name, out renderer))
            {
                var iconStream = icon.GetStream();
                var svgDocument = new XmlDocument();
                svgDocument.Load(iconStream);
                var element = SvgFactory.LoadFromXML(svgDocument, null);
                renderer = Create(element);
                rendererCache.Add(icon.Name, renderer);
            }

            return renderer;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposableResources.RemoveAll(disposable =>
                {
                    disposable.Dispose();
                    return true;
                });
                disposed = true;
            }
        }
    }
}
