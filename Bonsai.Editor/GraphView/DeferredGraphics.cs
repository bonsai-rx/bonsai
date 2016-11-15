using SvgNet.SvgGdi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Design
{
    class DeferredGraphics : IGraphics
    {
        Action<Graphics> action;

        public void Execute(Graphics graphics)
        {
            if (action != null)
            {
                action(graphics);
            }
        }

        public void Clear()
        {
            action = null;
        }

        public void AddMetafileComment(byte[] data)
        {
        }

        public GraphicsContainer BeginContainer(Rectangle dstrect, Rectangle srcrect, GraphicsUnit unit)
        {
            throw new NotImplementedException();
        }

        public GraphicsContainer BeginContainer()
        {
            throw new NotImplementedException();
        }

        public GraphicsContainer BeginContainer(RectangleF dstrect, RectangleF srcrect, GraphicsUnit unit)
        {
            throw new NotImplementedException();
        }

        public void Clear(Color color)
        {
            action += graphics => graphics.Clear(color);
        }

        public Region Clip
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                action += graphics => graphics.Clip = value;
            }
        }

        public RectangleF ClipBounds
        {
            get { throw new NotImplementedException(); }
        }

        public CompositingMode CompositingMode
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                action += graphics => graphics.CompositingMode = value;
            }
        }

        public CompositingQuality CompositingQuality
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                action += graphics => graphics.CompositingQuality = value;
            }
        }

        public float DpiX
        {
            get { throw new NotImplementedException(); }
        }

        public float DpiY
        {
            get { throw new NotImplementedException(); }
        }

        public void DrawArc(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
        {
            action += graphics => graphics.DrawArc(pen, rect, startAngle, sweepAngle);
        }

        public void DrawArc(Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
        {
            action += graphics => graphics.DrawArc(pen, x, y, width, height, startAngle, sweepAngle);
        }

        public void DrawArc(Pen pen, RectangleF rect, float startAngle, float sweepAngle)
        {
            action += graphics => graphics.DrawArc(pen, rect, startAngle, sweepAngle);
        }

        public void DrawArc(Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            action += graphics => graphics.DrawArc(pen, x, y, width, height, startAngle, sweepAngle);
        }

        public void DrawBezier(Pen pen, Point pt1, Point pt2, Point pt3, Point pt4)
        {
            action += graphics => graphics.DrawBezier(pen, pt1, pt2, pt3, pt4);
        }

        public void DrawBezier(Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4)
        {
            action += graphics => graphics.DrawBezier(pen, pt1, pt2, pt3, pt4);
        }

        public void DrawBezier(Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            action += graphics => graphics.DrawBezier(pen, x1, y1, x2, y2, x3, y3, x4, y4);
        }

        public void DrawBeziers(Pen pen, Point[] points)
        {
            action += graphics => graphics.DrawBeziers(pen, points);
        }

        public void DrawBeziers(Pen pen, PointF[] points)
        {
            action += graphics => graphics.DrawBeziers(pen, points);
        }

        public void DrawClosedCurve(Pen pen, Point[] points, float tension, FillMode fillmode)
        {
            action += graphics => graphics.DrawClosedCurve(pen, points, tension, fillmode);
        }

        public void DrawClosedCurve(Pen pen, Point[] points)
        {
            action += graphics => graphics.DrawClosedCurve(pen, points);
        }

        public void DrawClosedCurve(Pen pen, PointF[] points, float tension, FillMode fillmode)
        {
            action += graphics => graphics.DrawClosedCurve(pen, points, tension, fillmode);
        }

        public void DrawClosedCurve(Pen pen, PointF[] points)
        {
            action += graphics => graphics.DrawClosedCurve(pen, points);
        }

        public void DrawCurve(Pen pen, Point[] points, int offset, int numberOfSegments, float tension)
        {
            action += graphics => graphics.DrawCurve(pen, points, offset, numberOfSegments, tension);
        }

        public void DrawCurve(Pen pen, Point[] points, float tension)
        {
            action += graphics => graphics.DrawCurve(pen, points, tension);
        }

        public void DrawCurve(Pen pen, Point[] points)
        {
            action += graphics => graphics.DrawCurve(pen, points);
        }

        public void DrawCurve(Pen pen, PointF[] points, int offset, int numberOfSegments, float tension)
        {
            action += graphics => graphics.DrawCurve(pen, points, offset, numberOfSegments, tension);
        }

        public void DrawCurve(Pen pen, PointF[] points, int offset, int numberOfSegments)
        {
            action += graphics => graphics.DrawCurve(pen, points, offset, numberOfSegments);
        }

        public void DrawCurve(Pen pen, PointF[] points, float tension)
        {
            action += graphics => graphics.DrawCurve(pen, points, tension);
        }

        public void DrawCurve(Pen pen, PointF[] points)
        {
            action += graphics => graphics.DrawCurve(pen, points);
        }

        public void DrawEllipse(Pen pen, int x, int y, int width, int height)
        {
            action += graphics => graphics.DrawEllipse(pen, x, y, width, height);
        }

        public void DrawEllipse(Pen pen, Rectangle rect)
        {
            action += graphics => graphics.DrawEllipse(pen, rect);
        }

        public void DrawEllipse(Pen pen, float x, float y, float width, float height)
        {
            action += graphics => graphics.DrawEllipse(pen, x, y, width, height);
        }

        public void DrawEllipse(Pen pen, RectangleF rect)
        {
            action += graphics => graphics.DrawEllipse(pen, rect);
        }

        public void DrawIcon(Icon icon, Rectangle targetRect)
        {
            action += graphics => graphics.DrawIcon(icon, targetRect);
        }

        public void DrawIcon(Icon icon, int x, int y)
        {
            action += graphics => graphics.DrawIcon(icon, x, y);
        }

        public void DrawIconUnstretched(Icon icon, Rectangle targetRect)
        {
            action += graphics => graphics.DrawIconUnstretched(icon, targetRect);
        }

        public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr)
        {
            action += graphics => graphics.DrawImage(image, destRect, srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttr);
        }

        public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit)
        {
            action += graphics => graphics.DrawImage(image, destRect, srcX, srcY, srcWidth, srcHeight, srcUnit);
        }

        public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs)
        {
            action += graphics => graphics.DrawImage(image, destRect, srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttrs);
        }

        public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit)
        {
            action += graphics => graphics.DrawImage(image, destRect, srcX, srcY, srcWidth, srcHeight, srcUnit);
        }

        public void DrawImage(Image image, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr)
        {
            action += graphics => graphics.DrawImage(image, destPoints, srcRect, srcUnit, imageAttr);
        }

        public void DrawImage(Image image, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit)
        {
            action += graphics => graphics.DrawImage(image, destPoints, srcRect, srcUnit);
        }

        public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, Graphics.DrawImageAbort callback)
        {
            action += graphics => graphics.DrawImage(image, destPoints, srcRect, srcUnit, imageAttr, callback);
        }

        public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr)
        {
            action += graphics => graphics.DrawImage(image, destPoints, srcRect, srcUnit, imageAttr);
        }

        public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit)
        {
            action += graphics => graphics.DrawImage(image, destPoints, srcRect, srcUnit);
        }

        public void DrawImage(Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit)
        {
            action += graphics => graphics.DrawImage(image, destRect, srcRect, srcUnit);
        }

        public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit)
        {
            action += graphics => graphics.DrawImage(image, destRect, srcRect, srcUnit);
        }

        public void DrawImage(Image image, int x, int y, Rectangle srcRect, GraphicsUnit srcUnit)
        {
            action += graphics => graphics.DrawImage(image, x, y, srcRect, srcUnit);
        }

        public void DrawImage(Image image, float x, float y, RectangleF srcRect, GraphicsUnit srcUnit)
        {
            action += graphics => graphics.DrawImage(image, x, y, srcRect, srcUnit);
        }

        public void DrawImage(Image image, Point[] destPoints)
        {
            action += graphics => graphics.DrawImage(image, destPoints);
        }

        public void DrawImage(Image image, PointF[] destPoints)
        {
            action += graphics => graphics.DrawImage(image, destPoints);
        }

        public void DrawImage(Image image, int x, int y, int width, int height)
        {
            action += graphics => graphics.DrawImage(image, x, y, width, height);
        }

        public void DrawImage(Image image, Rectangle rect)
        {
            action += graphics => graphics.DrawImage(image, rect);
        }

        public void DrawImage(Image image, int x, int y)
        {
            action += graphics => graphics.DrawImage(image, x, y);
        }

        public void DrawImage(Image image, Point point)
        {
            action += graphics => graphics.DrawImage(image, point);
        }

        public void DrawImage(Image image, float x, float y, float width, float height)
        {
            action += graphics => graphics.DrawImage(image, x, y, width, height);
        }

        public void DrawImage(Image image, RectangleF rect)
        {
            action += graphics => graphics.DrawImage(image, rect);
        }

        public void DrawImage(Image image, float x, float y)
        {
            action += graphics => graphics.DrawImage(image, x, y);
        }

        public void DrawImage(Image image, PointF point)
        {
            action += graphics => graphics.DrawImage(image, point);
        }

        public void DrawImageUnscaled(Image image, int x, int y, int width, int height)
        {
            action += graphics => graphics.DrawImageUnscaled(image, x, y, width, height);
        }

        public void DrawImageUnscaled(Image image, Rectangle rect)
        {
            action += graphics => graphics.DrawImageUnscaled(image, rect);
        }

        public void DrawImageUnscaled(Image image, int x, int y)
        {
            action += graphics => graphics.DrawImageUnscaled(image, x, y);
        }

        public void DrawImageUnscaled(Image image, Point point)
        {
            action += graphics => graphics.DrawImageUnscaled(image, point);
        }

        public void DrawLine(Pen pen, Point pt1, Point pt2)
        {
            action += graphics => graphics.DrawLine(pen, pt1, pt2);
        }

        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            action += graphics => graphics.DrawLine(pen, x1, y1, x2, y2);
        }

        public void DrawLine(Pen pen, PointF pt1, PointF pt2)
        {
            action += graphics => graphics.DrawLine(pen, pt1, pt2);
        }

        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            action += graphics => graphics.DrawLine(pen, x1, y1, x2, y2);
        }

        public void DrawLines(Pen pen, Point[] points)
        {
            action += graphics => graphics.DrawLines(pen, points);
        }

        public void DrawLines(Pen pen, PointF[] points)
        {
            action += graphics => graphics.DrawLines(pen, points);
        }

        public void DrawPath(Pen pen, GraphicsPath path)
        {
            action += graphics => graphics.DrawPath(pen, path);
        }

        public void DrawPie(Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
        {
            action += graphics => graphics.DrawPie(pen, x, y, width, height, startAngle, sweepAngle);
        }

        public void DrawPie(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
        {
            action += graphics => graphics.DrawPie(pen, rect, startAngle, sweepAngle);
        }

        public void DrawPie(Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            action += graphics => graphics.DrawPie(pen, x, y, width, height, startAngle, sweepAngle);
        }

        public void DrawPie(Pen pen, RectangleF rect, float startAngle, float sweepAngle)
        {
            action += graphics => graphics.DrawPie(pen, rect, startAngle, sweepAngle);
        }

        public void DrawPolygon(Pen pen, Point[] points)
        {
            action += graphics => graphics.DrawPolygon(pen, points);
        }

        public void DrawPolygon(Pen pen, PointF[] points)
        {
            action += graphics => graphics.DrawPolygon(pen, points);
        }

        public void DrawRectangle(Pen pen, int x, int y, int width, int height)
        {
            action += graphics => graphics.DrawRectangle(pen, x, y, width, height);
        }

        public void DrawRectangle(Pen pen, float x, float y, float width, float height)
        {
            action += graphics => graphics.DrawRectangle(pen, x, y, width, height);
        }

        public void DrawRectangle(Pen pen, Rectangle rect)
        {
            action += graphics => graphics.DrawRectangle(pen, rect);
        }

        public void DrawRectangles(Pen pen, Rectangle[] rects)
        {
            action += graphics => graphics.DrawRectangles(pen, rects);
        }

        public void DrawRectangles(Pen pen, RectangleF[] rects)
        {
            action += graphics => graphics.DrawRectangles(pen, rects);
        }

        public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
        {
            action += graphics => graphics.DrawString(s, font, brush, layoutRectangle, format);
        }

        public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle)
        {
            action += graphics => graphics.DrawString(s, font, brush, layoutRectangle);
        }

        public void DrawString(string s, Font font, Brush brush, PointF point, StringFormat format)
        {
            action += graphics => graphics.DrawString(s, font, brush, point, format);
        }

        public void DrawString(string s, Font font, Brush brush, float x, float y, StringFormat format)
        {
            action += graphics => graphics.DrawString(s, font, brush, x, y, format);
        }

        public void DrawString(string s, Font font, Brush brush, PointF point)
        {
            action += graphics => graphics.DrawString(s, font, brush, point);
        }

        public void DrawString(string s, Font font, Brush brush, float x, float y)
        {
            action += graphics => graphics.DrawString(s, font, brush, x, y);
        }

        public void EndContainer(GraphicsContainer container)
        {
            action += graphics => graphics.EndContainer(container);
        }

        public void ExcludeClip(Region region)
        {
            action += graphics => graphics.ExcludeClip(region);
        }

        public void ExcludeClip(Rectangle rect)
        {
            action += graphics => graphics.ExcludeClip(rect);
        }

        public void FillClosedCurve(Brush brush, Point[] points, FillMode fillmode, float tension)
        {
            action += graphics => graphics.FillClosedCurve(brush, points, fillmode, tension);
        }

        public void FillClosedCurve(Brush brush, Point[] points, FillMode fillmode)
        {
            action += graphics => graphics.FillClosedCurve(brush, points, fillmode);
        }

        public void FillClosedCurve(Brush brush, Point[] points)
        {
            action += graphics => graphics.FillClosedCurve(brush, points);
        }

        public void FillClosedCurve(Brush brush, PointF[] points, FillMode fillmode, float tension)
        {
            action += graphics => graphics.FillClosedCurve(brush, points, fillmode, tension);
        }

        public void FillClosedCurve(Brush brush, PointF[] points, FillMode fillmode)
        {
            action += graphics => graphics.FillClosedCurve(brush, points, fillmode);
        }

        public void FillClosedCurve(Brush brush, PointF[] points)
        {
            action += graphics => graphics.FillClosedCurve(brush, points);
        }

        public void FillEllipse(Brush brush, int x, int y, int width, int height)
        {
            action += graphics => graphics.FillEllipse(brush, x, y, width, height);
        }

        public void FillEllipse(Brush brush, Rectangle rect)
        {
            action += graphics => graphics.FillEllipse(brush, rect);
        }

        public void FillEllipse(Brush brush, float x, float y, float width, float height)
        {
            action += graphics => graphics.FillEllipse(brush, x, y, width, height);
        }

        public void FillEllipse(Brush brush, RectangleF rect)
        {
            action += graphics => graphics.FillEllipse(brush, rect);
        }

        public void FillPath(Brush brush, GraphicsPath path)
        {
            action += graphics => graphics.FillPath(brush, path);
        }

        public void FillPie(Brush brush, int x, int y, int width, int height, int startAngle, int sweepAngle)
        {
            action += graphics => graphics.FillPie(brush, x, y, width, height, startAngle, sweepAngle);
        }

        public void FillPie(Brush brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            action += graphics => graphics.FillPie(brush, x, y, width, height, startAngle, sweepAngle);
        }

        public void FillPie(Brush brush, Rectangle rect, float startAngle, float sweepAngle)
        {
            action += graphics => graphics.FillPie(brush, rect, startAngle, sweepAngle);
        }

        public void FillPolygon(Brush brush, Point[] points, FillMode fillMode)
        {
            action += graphics => graphics.FillPolygon(brush, points, fillMode);
        }

        public void FillPolygon(Brush brush, Point[] points)
        {
            action += graphics => graphics.FillPolygon(brush, points);
        }

        public void FillPolygon(Brush brush, PointF[] points, FillMode fillMode)
        {
            action += graphics => graphics.FillPolygon(brush, points, fillMode);
        }

        public void FillPolygon(Brush brush, PointF[] points)
        {
            action += graphics => graphics.FillPolygon(brush, points);
        }

        public void FillRectangle(Brush brush, int x, int y, int width, int height)
        {
            action += graphics => graphics.FillRectangle(brush, x, y, width, height);
        }

        public void FillRectangle(Brush brush, Rectangle rect)
        {
            action += graphics => graphics.FillRectangle(brush, rect);
        }

        public void FillRectangle(Brush brush, float x, float y, float width, float height)
        {
            action += graphics => graphics.FillRectangle(brush, x, y, width, height);
        }

        public void FillRectangle(Brush brush, RectangleF rect)
        {
            action += graphics => graphics.FillRectangle(brush, rect);
        }

        public void FillRectangles(Brush brush, Rectangle[] rects)
        {
            action += graphics => graphics.FillRectangles(brush, rects);
        }

        public void FillRectangles(Brush brush, RectangleF[] rects)
        {
            action += graphics => graphics.FillRectangles(brush, rects);
        }

        public void FillRegion(Brush brush, Region region)
        {
            action += graphics => graphics.FillRegion(brush, region);
        }

        public void Flush(FlushIntention intention)
        {
            action += graphics => graphics.Flush(intention);
        }

        public void Flush()
        {
            action+=graphics=>graphics.Flush();
        }

        public Color GetNearestColor(Color color)
        {
            throw new NotImplementedException();
        }

        public InterpolationMode InterpolationMode
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                action += graphics => graphics.InterpolationMode = value;
            }
        }

        public void IntersectClip(Region region)
        {
            action += graphics => graphics.IntersectClip(region);
        }

        public void IntersectClip(RectangleF rect)
        {
            action += graphics => graphics.IntersectClip(rect);
        }

        public void IntersectClip(Rectangle rect)
        {
            action += graphics => graphics.IntersectClip(rect);
        }

        public bool IsClipEmpty
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsVisible(RectangleF rect)
        {
            throw new NotImplementedException();
        }

        public bool IsVisible(float x, float y, float width, float height)
        {
            throw new NotImplementedException();
        }

        public bool IsVisible(Rectangle rect)
        {
            throw new NotImplementedException();
        }

        public bool IsVisible(int x, int y, int width, int height)
        {
            throw new NotImplementedException();
        }

        public bool IsVisible(PointF point)
        {
            throw new NotImplementedException();
        }

        public bool IsVisible(float x, float y)
        {
            throw new NotImplementedException();
        }

        public bool IsVisible(Point point)
        {
            throw new NotImplementedException();
        }

        public bool IsVisible(int x, int y)
        {
            throw new NotImplementedException();
        }

        public bool IsVisibleClipEmpty
        {
            get { throw new NotImplementedException(); }
        }

        public Region[] MeasureCharacterRanges(string text, Font font, RectangleF layoutRect, StringFormat stringFormat)
        {
            throw new NotImplementedException();
        }

        public SizeF MeasureString(string text, Font font, int width, StringFormat format)
        {
            throw new NotImplementedException();
        }

        public SizeF MeasureString(string text, Font font, int width)
        {
            throw new NotImplementedException();
        }

        public SizeF MeasureString(string text, Font font)
        {
            throw new NotImplementedException();
        }

        public SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat stringFormat)
        {
            throw new NotImplementedException();
        }

        public SizeF MeasureString(string text, Font font, SizeF layoutArea)
        {
            throw new NotImplementedException();
        }

        public SizeF MeasureString(string text, Font font, PointF origin, StringFormat stringFormat)
        {
            throw new NotImplementedException();
        }

        public SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat stringFormat, out int charactersFitted, out int linesFilled)
        {
            throw new NotImplementedException();
        }

        public void MultiplyTransform(Matrix matrix, MatrixOrder order)
        {
            action += graphics => graphics.MultiplyTransform(matrix, order);
        }

        public void MultiplyTransform(Matrix matrix)
        {
            action += graphics => graphics.MultiplyTransform(matrix);
        }

        public float PageScale
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                action += graphics => graphics.PageScale = value;
            }
        }

        public GraphicsUnit PageUnit
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                action += graphics => graphics.PageUnit = value;
            }
        }

        public PixelOffsetMode PixelOffsetMode
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                action += graphics => graphics.PixelOffsetMode = value;
            }
        }

        public Point RenderingOrigin
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                action += graphics => graphics.RenderingOrigin = value;
            }
        }

        public void ResetClip()
        {
            action += graphics => graphics.ResetClip();
        }

        public void ResetTransform()
        {
            action += graphics => graphics.ResetTransform();
        }

        public void Restore(GraphicsState gstate)
        {
            action += graphics => graphics.Restore(gstate);
        }

        public void RotateTransform(float angle, MatrixOrder order)
        {
            action += graphics => graphics.RotateTransform(angle, order);
        }

        public void RotateTransform(float angle)
        {
            action += graphics => graphics.RotateTransform(angle);
        }

        public GraphicsState Save()
        {
            throw new NotImplementedException();
        }

        public void ScaleTransform(float sx, float sy, MatrixOrder order)
        {
            action += graphics => graphics.ScaleTransform(sx, sy, order);
        }

        public void ScaleTransform(float sx, float sy)
        {
            action += graphics => graphics.ScaleTransform(sx, sy);
        }

        public void SetClip(Region region, CombineMode combineMode)
        {
            action += graphics => graphics.SetClip(region, combineMode);
        }

        public void SetClip(GraphicsPath path, CombineMode combineMode)
        {
            action += graphics => graphics.SetClip(path, combineMode);
        }

        public void SetClip(GraphicsPath path)
        {
            action += graphics => graphics.SetClip(path);
        }

        public void SetClip(RectangleF rect, CombineMode combineMode)
        {
            action += graphics => graphics.SetClip(rect, combineMode);
        }

        public void SetClip(RectangleF rect)
        {
            action += graphics => graphics.SetClip(rect);
        }

        public void SetClip(Rectangle rect, CombineMode combineMode)
        {
            action += graphics => graphics.SetClip(rect, combineMode);
        }

        public void SetClip(Rectangle rect)
        {
            action += graphics => graphics.SetClip(rect);
        }

        public void SetClip(Graphics g, CombineMode combineMode)
        {
            action += graphics => graphics.SetClip(g, combineMode);
        }

        public void SetClip(Graphics g)
        {
            action += graphics => graphics.SetClip(g);
        }

        public SmoothingMode SmoothingMode
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                action += graphics => graphics.SmoothingMode = value;
            }
        }

        public int TextContrast
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                action += graphics => graphics.TextContrast = value;
            }
        }

        public TextRenderingHint TextRenderingHint
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                action += graphics => graphics.TextRenderingHint = value;
            }
        }

        public Matrix Transform
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                action += graphics => graphics.Transform = value;
            }
        }

        public void TransformPoints(CoordinateSpace destSpace, CoordinateSpace srcSpace, Point[] pts)
        {
            action += graphics => graphics.TransformPoints(destSpace, srcSpace, pts);
        }

        public void TransformPoints(CoordinateSpace destSpace, CoordinateSpace srcSpace, PointF[] pts)
        {
            action += graphics => graphics.TransformPoints(destSpace, srcSpace, pts);
        }

        public void TranslateClip(int dx, int dy)
        {
            action += graphics => graphics.TranslateClip(dx, dy);
        }

        public void TranslateClip(float dx, float dy)
        {
            action += graphics => graphics.TranslateClip(dx, dy);
        }

        public void TranslateTransform(float dx, float dy, MatrixOrder order)
        {
            action += graphics => graphics.TranslateTransform(dx, dy, order);
        }

        public void TranslateTransform(float dx, float dy)
        {
            action += graphics => graphics.TranslateTransform(dx, dy);
        }

        public RectangleF VisibleClipBounds
        {
            get { throw new NotImplementedException(); }
        }
    }
}
