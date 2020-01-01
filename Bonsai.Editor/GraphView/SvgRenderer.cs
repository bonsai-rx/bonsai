using SvgNet;
using SvgNet.SvgElements;
using SvgNet.SvgGdi;
using SvgNet.SvgTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Bonsai.Editor.GraphView
{
    delegate void SvgRenderer(SvgRendererState state, IGraphics graphics);

    class SvgRendererState
    {
        public float Scale;
        public PointF Translation;
        public Color CurrentColor;
        public SolidBrush Fill;
        public Pen Stroke;

        public Brush FillStyle()
        {
            return FillStyle(CurrentColor);
        }

        public Brush FillStyle(Color color)
        {
            Fill.Color = color;
            return Fill;
        }

        public Pen StrokeStyle(Color color, float width)
        {
            Stroke.Color = color;
            Stroke.Width = width;
            return Stroke;
        }
    }

    class SvgRendererContext
    {
        readonly ParameterExpression state;
        readonly ParameterExpression graphics;
        readonly Dictionary<string, Brush> gradients;
        readonly List<Expression> expressions;

        public SvgRendererContext()
        {
            state = Expression.Parameter(typeof(SvgRendererState), "state");
            graphics = Expression.Parameter(typeof(IGraphics), "graphics");
            gradients = new Dictionary<string, Brush>();
            expressions = new List<Expression>();
        }

        public ParameterExpression State
        {
            get { return state; }
        }

        public ParameterExpression Graphics
        {
            get { return graphics; }
        }

        public IDictionary<string, Brush> Gradients
        {
            get { return gradients; }
        }

        public ICollection<Expression> Expressions
        {
            get { return expressions; }
        }
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
            var data = (string)element.Attributes[attribute];
            if (data == null) return null;

            var previous = true;
            var pathBuilder = new StringBuilder(data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                var c = data[i];
                if (char.IsWhiteSpace(c) || c == ',' || c == 'e')
                {
                    if (!previous) pathBuilder.Append(c);
                    previous = true;
                    continue;
                }

                var letter = char.IsLetter(c);
                if (!previous && (letter || c == '-'))
                {
                    pathBuilder.Append(' ');
                }
                pathBuilder.Append(c);
                if (letter && i < data.Length - 1) pathBuilder.Append(' ');
                previous = letter;
            }

            return (SvgPath)pathBuilder.ToString();
        }

        static PointF? ParsePoint(SvgElement element, string x, string y)
        {
            var valX = (string)element.Attributes[x];
            var valY = (string)element.Attributes[y];
            if (valX != null && valY != null)
            {
                return new PointF(((SvgLength)valX).Value, ((SvgLength)valY).Value);
            }
            else return null;
        }

        [DebuggerDisplay("Fill = {Fill}, Stroke = {Stroke}")]
        class SvgDrawingStyle
        {
            public Expression Fill;
            public Expression Stroke;

            public SvgDrawingStyle(Expression fill, Expression stroke)
            {
                Fill = fill;
                Stroke = stroke;
            }
        }

        static SvgStyle ParseStyle(SvgElement element, string attribute)
        {
            var value = element.Attributes[attribute];
            var style = value as SvgStyle;
            if (style == null)
            {
                var rawStyle = value as string;
                if (rawStyle == null) return null;
                style = (SvgStyle)rawStyle;
            }

            return style;
        }

        static void SetStyleAttribute(ref SvgStyle style, SvgElement element, string attribute)
        {
            var value = (string)element.Attributes[attribute];
            if (value != null)
            {
                if (style == null) style = new SvgStyle();
                style.Set(attribute, value);
            }
        }

        SvgStyle GetElementStyle(SvgElement element)
        {
            SvgStyle style = null;
            do
            {
                var elementStyle = ParseStyle(element, "style");
                if (style == null) style = elementStyle;
                else if (elementStyle != null) style += elementStyle;
                SetStyleAttribute(ref style, element, "fill");
                SetStyleAttribute(ref style, element, "fill-opacity");
                SetStyleAttribute(ref style, element, "stroke");
                SetStyleAttribute(ref style, element, "stroke-width");
                SetStyleAttribute(ref style, element, "stroke-opacity");
            } while (style == null && (element = element.Parent) != null && element.Name == "g");
            return style;
        }

        SvgDrawingStyle CreateStyle(SvgElement element, SvgRendererContext context)
        {
            Expression fill, stroke;
            var style = GetElementStyle(element);
            fill = CreateFill(style, context);
            stroke = CreateStroke(style, context);
            if (fill == null && stroke == null) return null;
            else return new SvgDrawingStyle(fill, stroke);
        }

        Matrix ParseTransform(SvgElement element, Matrix parent)
        {
            return ParseTransform(element, parent, "transform");
        }

        Matrix ParseTransform(SvgElement element, Matrix parent, string attribute)
        {
            Matrix result = null;
            var transformAttribute = element.Attributes[attribute];
            if (transformAttribute != null)
            {
                result = new Matrix();
                disposableResources.Add(result);
                var transformList = transformAttribute as SvgTransformList;
                if (transformList == null) transformList = (SvgTransformList)(string)transformAttribute;
                for (int i = 0; i < transformList.Count; i++)
                {
                    var transform = transformList[i];
                    result.Multiply(transform.Matrix);
                }
            }

            if (parent != null && result == null) result = parent;
            else if (parent != null) result.Multiply(parent, MatrixOrder.Append);
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
            return transform != null ? Expression.Constant(transform) : null;
        }

        Expression CreateFill(SvgStyle style, SvgRendererContext context)
        {
            if (style == null) return CreateFill(null, null, context);
            var fill = (string)style.Get("fill");
            var opacity = (string)style.Get("fill-opacity");
            return CreateFill(fill, opacity, context);
        }

        Expression CreateFill(string fill, string opacity, SvgRendererContext context)
        {
            const string UrlPrefix = "url(";
            if (fill == "none") return null;
            if (fill == null) return Expression.Call(context.State, "FillStyle", null);
            if (fill.StartsWith(UrlPrefix))
            {
                Brush brush;
                var href = fill.Substring(UrlPrefix.Length, fill.Length - UrlPrefix.Length - 1);
                if (!context.Gradients.TryGetValue(href, out brush)) return null;
                else return Expression.Constant(brush);
            }
            else
            {
                var color = ((SvgColor)fill).Color;
                if (opacity != null)
                {
                    var opacityValue = ((SvgLength)opacity).Value;
                    color = Color.FromArgb((int)(opacityValue * 255), color);
                }

                return Expression.Call(context.State, "FillStyle", null, Expression.Constant(color));
            }
        }

        Expression CreateStroke(SvgStyle style, SvgRendererContext context)
        {
            if (style == null) return null;
            var stroke = (string)style.Get("stroke");
            var strokeWidth = (string)style.Get("stroke-width");
            var strokeOpacity = (string)style.Get("stroke-opacity");
            return CreateStroke(stroke, strokeWidth, strokeOpacity, context);
        }

        Expression CreateStroke(string stroke, string strokeWidth, string opacity, SvgRendererContext context)
        {
            Expression colorArgument;
            if (stroke == null || stroke == "none") return null;
            var alpha = opacity == null ? null : (int?)(((SvgLength)opacity).Value * 255);
            if (stroke == "currentColor")
            {
                colorArgument = Expression.PropertyOrField(context.State, "CurrentColor");
                if (alpha.HasValue)
                {
                    var alphaArgument = Expression.Constant(alpha.Value);
                    colorArgument = Expression.Call(typeof(Color), "FromArgb", null, alphaArgument, colorArgument);
                }
            }
            else
            {
                var color = ((SvgColor)stroke).Color;
                if (alpha.HasValue) color = Color.FromArgb(alpha.Value, color);
                colorArgument = Expression.Constant(color);
            }
            var width = strokeWidth == null ? 1 : ((SvgLength)strokeWidth);
            var widthArgument = Expression.Constant(width.Value);
            return Expression.Call(context.State, "StrokeStyle", null, colorArgument, widthArgument);
        }

        void CreateDrawTransform(SvgElement element, Matrix transform, SvgRendererContext context)
        {
            var localTransform = CreateTransform(element, transform);
            var scale = Expression.PropertyOrField(context.State, "Scale");
            var translation = Expression.PropertyOrField(context.State, "Translation");
            var offsetX = Expression.PropertyOrField(translation, "X");
            var offsetY = Expression.PropertyOrField(translation, "Y");
            context.Expressions.Add(Expression.Call(context.Graphics, "TranslateTransform", null, offsetX, offsetY));
            context.Expressions.Add(Expression.Call(context.Graphics, "ScaleTransform", null, scale, scale));
            if (localTransform != null)
            {
                context.Expressions.Add(Expression.Call(context.Graphics, "MultiplyTransform", null, localTransform));
            }
        }

        void CreateResetTransform(SvgRendererContext context)
        {
            context.Expressions.Add(Expression.Call(context.Graphics, "ResetTransform", null));
        }

        void CreateDrawRectangle(SvgElement element, Matrix transform, SvgRendererContext context)
        {
            var style = CreateStyle(element, context);
            if (style != null)
            {
                var x = CreateFloat(element, "x");
                var y = CreateFloat(element, "y");
                var width = CreateFloat(element, "width");
                var height = CreateFloat(element, "height");
                CreateDrawTransform(element, transform, context);
                if (style.Fill != null)
                {
                    context.Expressions.Add(Expression.Call(context.Graphics, "FillRectangle", null, style.Fill, x, y, width, height));
                }
                if (style.Stroke != null)
                {
                    context.Expressions.Add(Expression.Call(context.Graphics, "DrawRectangle", null, style.Stroke, x, y, width, height));
                }
                CreateResetTransform(context);
            }
        }

        void CreateDrawCircle(SvgElement element, Matrix transform, SvgRendererContext context)
        {
            var style = CreateStyle(element, context);
            if (style != null)
            {
                var cx = ParseFloat(element, "cx");
                var cy = ParseFloat(element, "cy");
                var r = ParseFloat(element, "r");
                var x = Expression.Constant(cx - r);
                var y = Expression.Constant(cy - r);
                var d = Expression.Constant(2f * r);
                CreateDrawTransform(element, transform, context);
                if (style.Fill != null)
                {
                    context.Expressions.Add(Expression.Call(context.Graphics, "FillEllipse", null, style.Fill, x, y, d, d));
                }
                if (style.Stroke != null)
                {
                    context.Expressions.Add(Expression.Call(context.Graphics, "DrawEllipse", null, style.Stroke, x, y, d, d));
                }
                CreateResetTransform(context);
            }
        }

        void CreateDrawEllipse(SvgElement element, Matrix transform, SvgRendererContext context)
        {
            var style = CreateStyle(element, context);
            if (style != null)
            {
                var cx = ParseFloat(element, "cx");
                var cy = ParseFloat(element, "cy");
                var rx = ParseFloat(element, "rx");
                var ry = ParseFloat(element, "ry");
                var x = Expression.Constant(cx - rx);
                var y = Expression.Constant(cy - ry);
                var dx = Expression.Constant(2f * rx);
                var dy = Expression.Constant(2f * ry);
                CreateDrawTransform(element, transform, context);
                if (style.Fill != null)
                {
                    context.Expressions.Add(Expression.Call(context.Graphics, "FillEllipse", null, style.Fill, x, y, dx, dy));
                }
                if (style.Stroke != null)
                {
                    context.Expressions.Add(Expression.Call(context.Graphics, "DrawEllipse", null, style.Stroke, x, y, dx, dy));
                }
                CreateResetTransform(context);
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
            if (segment.Type == SvgPathSegType.SVG_SEGTYPE_SMOOTHCURVETO)
            {
                var first = points[points.Count - 1];
                if (points.Count == 1) points.Add(first);
                else
                {
                    var reflection = points[points.Count - 2];
                    reflection.X = 2 * first.X - reflection.X;
                    reflection.Y = 2 * first.Y - reflection.Y;
                    points.Add(reflection);
                }
            }
            for (int i = 0; i < data.Length / 2; i++)
            {
                points.Add(new PointF(
                    data[i * 2 + 0] + offset.X,
                    data[i * 2 + 1] + offset.Y));
            }
        }

        static void AddPathData(GraphicsPath path, SvgPath pathData)
        {
            var point = default(PointF);
            var bezierPoints = new List<PointF>(pathData.Count);
            var linePoints = new List<PointF>(pathData.Count);
            for (int i = 0; i < pathData.Count; i++)
            {
                var segment = pathData[i];
                if (segment.Abs && segment.Type != SvgPathSegType.SVG_SEGTYPE_CURVETO)
                {
                    point = default(PointF);
                }

                switch (segment.Type)
                {
                    case SvgPathSegType.SVG_SEGTYPE_CURVETO:
                    case SvgPathSegType.SVG_SEGTYPE_SMOOTHCURVETO:
                        AddLines(path, linePoints);
                        if (bezierPoints.Count == 0) bezierPoints.Add(point);
                        AddBezierData(bezierPoints, segment);
                        point = bezierPoints[bezierPoints.Count - 1];
                        break;
                    case SvgPathSegType.SVG_SEGTYPE_MOVETO:
                        AddLines(path, linePoints);
                        AddBeziers(path, bezierPoints);
                        path.StartFigure();
                        point.X += segment.Data[0];
                        point.Y += segment.Data[1];
                        linePoints.Add(point);
                        break;
                    case SvgPathSegType.SVG_SEGTYPE_LINETO:
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
                        if (segment.Abs) point.Y = linePoints[linePoints.Count - 1].Y;
                        linePoints.Add(point);
                        break;
                    case SvgPathSegType.SVG_SEGTYPE_VLINETO:
                        StartLine(bezierPoints, linePoints, segment, ref point);
                        AddBeziers(path, bezierPoints);
                        if (segment.Abs) point.X = linePoints[linePoints.Count - 1].X;
                        point.Y += segment.Data[0];
                        linePoints.Add(point);
                        break;
                    case SvgPathSegType.SVG_SEGTYPE_CLOSEPATH:
                        if (linePoints.Count > 0) point = linePoints[0];
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

        void CreateDrawPath(SvgElement element, Matrix transform, SvgRendererContext context)
        {
            var style = CreateStyle(element, context);
            if (style != null)
            {
                var path = CreatePath(element, "d");
                CreateDrawTransform(element, transform, context);
                if (style.Fill != null)
                {
                    context.Expressions.Add(Expression.Call(context.Graphics, "FillPath", null, style.Fill, path));
                }
                if (style.Stroke != null)
                {
                    context.Expressions.Add(Expression.Call(context.Graphics, "DrawPath", null, style.Stroke, path));
                }
                CreateResetTransform(context);
            }
        }

        Color ParseStopColor(SvgStopElement stop)
        {
            var color = new SvgColor((string)stop.Style.Get("stop-color"));
            var opacity = float.Parse((string)stop.Style.Get("stop-opacity"), CultureInfo.InvariantCulture);
            return Color.FromArgb((int)(255 * opacity), color.Color);
        }

        void CreateLinearGradient(SvgElement element, SvgRendererContext context)
        {
            var linearGradient = element as SvgLinearGradientElement;
            if (linearGradient != null)
            {
                var color1 = default(Color);
                var color2 = default(Color);
                LinearGradientBrush gradient;
                if (linearGradient.Children.Count == 2)
                {
                    var stop1 = linearGradient.Children[0] as SvgStopElement;
                    var stop2 = linearGradient.Children[1] as SvgStopElement;
                    if (stop1 != null && stop2 != null)
                    {
                        color1 = ParseStopColor(stop1);
                        color2 = ParseStopColor(stop2);
                    }
                }
                else
                {
                    Brush referenceGradient;
                    LinearGradientBrush referenceLinearGradient;
                    var href = new SvgXRef(linearGradient).Href;
                    if (href != null && context.Gradients.TryGetValue(href, out referenceGradient) &&
                       (referenceLinearGradient = referenceGradient as LinearGradientBrush) != null)
                    {
                        color1 = referenceLinearGradient.LinearColors[0];
                        color2 = referenceLinearGradient.LinearColors[1];
                    }
                }

                var gradientTransform = ParseTransform(linearGradient, null, "gradientTransform");
                var points = new[]
                {
                    ParsePoint(linearGradient, "x1", "y1").GetValueOrDefault(PointF.Empty),
                    ParsePoint(linearGradient, "x2", "y2").GetValueOrDefault(new PointF(1, 1))
                };
                if (gradientTransform != null) gradientTransform.TransformPoints(points);
                gradient = new LinearGradientBrush(points[0], points[1], color1, color2);
                context.Gradients.Add(new SvgUriReference(linearGradient).Href, gradient);
                disposableResources.Add(gradient);
            }
        }

        void CreateDrawDefinitions(SvgElement element, SvgRendererContext context)
        {
            foreach (SvgElement child in element.Children)
            {
                switch (child.Name)
                {
                    case "linearGradient": CreateLinearGradient(child, context); break;
                    default: continue;
                }
            }
        }

        void CreateDrawChildren(SvgElement element, Matrix transform, SvgRendererContext context)
        {
            foreach (SvgElement child in element.Children)
            {
                CreateDrawBody(child, transform, context);
            }
        }

        void CreateDrawBody(SvgElement element, Matrix transform, SvgRendererContext context)
        {
            switch (element.Name)
            {
                case "rect": CreateDrawRectangle(element, transform, context); break;
                case "circle": CreateDrawCircle(element, transform, context); break;
                case "ellipse": CreateDrawEllipse(element, transform, context); break;
                case "path": CreateDrawPath(element, transform, context); break;
                case "svg": CreateDrawChildren(element, transform, context); break;
                case "defs": CreateDrawDefinitions(element, context); break;
                case "g":
                    var localTransform = ParseTransform(element, transform);
                    CreateDrawChildren(element, localTransform, context);
                    break;
            }
        }

        SvgRenderer CreateRenderer(SvgElement element)
        {
            var context = new SvgRendererContext();
            CreateDrawBody(element, null, context);
            var body = context.Expressions.Count > 0 ? (Expression)Expression.Block(context.Expressions) : Expression.Empty();
            var renderer = Expression.Lambda<SvgRenderer>(body, context.State, context.Graphics);
            return renderer.Compile();
        }

        public SvgRenderer GetIconRenderer(GraphNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            var icon = node.Icon;
            SvgRenderer renderer = null;
            Stack<string> fallbackIcons = null;
            while (icon != null)
            {
                if (TryGetIconRenderer(icon, out renderer)) break;
                else
                {
                    if (fallbackIcons == null) fallbackIcons = new Stack<string>();
                    fallbackIcons.Push(icon.Name);
                    icon = icon.GetDefaultIcon();
                }
            }

            if (renderer == null)
            {
                if (node.Icon.IsIncludeElement) TryGetIconRenderer(ElementIcon.Include, out renderer);
                else TryGetIconRenderer(ElementIcon.Default, out renderer);
            }

            while (fallbackIcons != null && fallbackIcons.Count > 0)
            {
                rendererCache.Add(fallbackIcons.Pop(), renderer);
            }
            return renderer;
        }

        public SvgRenderer GetIconRenderer(ElementCategory category)
        {
            SvgRenderer renderer;
            var categoryIcon = ElementIcon.FromElementCategory(category);
            if (!TryGetIconRenderer(categoryIcon, out renderer))
            {
                rendererCache.Add(categoryIcon.Name, renderer);
            }

            return renderer;
        }

        bool TryGetIconRenderer(ElementIcon icon, out SvgRenderer renderer)
        {
            if (icon == null)
            {
                throw new ArgumentNullException("icon");
            }

            if (!rendererCache.TryGetValue(icon.Name, out renderer))
            {
                using (var iconStream = icon.GetStream())
                {
                    if (iconStream == null) return false;
                    var svgDocument = new XmlDocument();
                    svgDocument.XmlResolver = null;
                    svgDocument.Load(iconStream);
                    var element = SvgFactory.LoadFromXML(svgDocument, null);
                    renderer = CreateRenderer(element);
                    rendererCache.Add(icon.Name, renderer);
                }
            }

            return true;
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
