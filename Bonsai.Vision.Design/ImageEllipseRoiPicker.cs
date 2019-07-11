using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Windows.Forms;
using System.Reactive;
using Bonsai.Vision.Design;
using System.Collections.ObjectModel;
using System.Drawing;
using Bonsai.Vision;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using Bonsai.Design;
using System.Globalization;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Collections.Specialized;
using Point = OpenCV.Net.Point;
using Font = System.Drawing.Font;

namespace Bonsai.Vision.Design
{
    class ImageEllipseRoiPicker : ImageBox
    {
        bool disposed;
        int? selectedRoi;
        float scaleFactor = 1;
        const int FillOpacity = 85;
        const float LabelFontScale = 0.1f;
        const double ScaleIncrement = 0.1;
        ObservableCollection<RotatedRect> regions = new ObservableCollection<RotatedRect>();
        CommandExecutor commandExecutor = new CommandExecutor();
        IplImage labelImage;
        IplImageTexture labelTexture;
        bool refreshLabels;
        Font labelFont;

        public ImageEllipseRoiPicker()
        {
            Canvas.KeyDown += Canvas_KeyDown;
            commandExecutor.StatusChanged += commandExecutor_StatusChanged;
            regions.CollectionChanged += regions_CollectionChanged;
            var mouseDoubleClick = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseDoubleClick").Select(e => e.EventArgs);
            var mouseMove = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseMove").Select(e => e.EventArgs);
            var mouseDown = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseDown").Select(e => e.EventArgs);
            var mouseUp = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseUp").Select(e => e.EventArgs);

            var roiSelected = from downEvt in mouseDown
                              where Image != null
                              let location = NormalizedLocation(downEvt.X, downEvt.Y)
                              let selection = (from region in regions.Select((rect, i) => new { rect, i = (int?)i })
                                               let distance = TestIntersection(region.rect, location)
                                               where distance < 1
                                               orderby distance
                                               select region.i)
                                               .FirstOrDefault()
                              select new Action(() => SelectedRegion = selection);

            var roiMoveScale = (from downEvt in mouseDown
                                where Image != null && selectedRoi.HasValue
                                let location = NormalizedLocation(downEvt.X, downEvt.Y)
                                let selection = selectedRoi.Value
                                let region = regions[selection]
                                select (from moveEvt in mouseMove.TakeUntil(mouseUp)
                                        let target = NormalizedLocation(moveEvt.X, moveEvt.Y)
                                        let modifiedRegion = downEvt.Button == MouseButtons.Right
                                            ? ScaleRegion(region, target, ModifierKeys.HasFlag(Keys.Control))
                                            : MoveRegion(region, target - location)
                                        let modifiedRectangle = RegionRectangle(modifiedRegion)
                                        where modifiedRectangle.Width > 0 && modifiedRectangle.Height > 0 &&
                                              modifiedRectangle.Left >= 0 && modifiedRectangle.Top >= 0 &&
                                              modifiedRectangle.Right < Image.Width && modifiedRectangle.Bottom < Image.Height
                                        select modifiedRegion)
                                        .Publish(ps =>
                                            ps.TakeLast(1).Do(modifiedRegion =>
                                                commandExecutor.Execute(
                                                    () => regions[selection] = modifiedRegion,
                                                    () => regions[selection] = region))
                                                .Merge(ps))
                                        .Select(displacedRegion => new Action(() => regions[selectedRoi.Value] = displacedRegion)))
                                .Switch();

            var regionInsertion = (from downEvt in mouseDown
                                   where Image != null && downEvt.Button == MouseButtons.Left && !selectedRoi.HasValue
                                   let count = regions.Count
                                   let origin = NormalizedLocation(downEvt.X, downEvt.Y)
                                   select (from moveEvt in mouseMove.TakeUntil(mouseUp)
                                           let location = EnsureSizeRatio(origin, NormalizedLocation(moveEvt.X, moveEvt.Y), ModifierKeys.HasFlag(Keys.Control))
                                           where location.X - origin.X != 0 && location.Y - origin.Y != 0
                                           select CreateEllipseRegion(origin, location))
                                           .Publish(ps =>
                                               ps.TakeLast(1).Do(region =>
                                                   commandExecutor.Execute(
                                                       () => { if (count == regions.Count) AddRegion(region); },
                                                       () => { regions.Remove(region); SelectedRegion = null; }))
                                                 .Merge(ps))
                                           .Select(region => new Action(() =>
                                           {
                                               if (selectedRoi.HasValue) regions[selectedRoi.Value] = region;
                                               else AddRegion(region);
                                           })))
                                   .Switch();

            var roiActions = Observable.Merge(roiSelected, roiMoveScale, regionInsertion);
            roiActions.Subscribe(action =>
            {
                action();
            });
        }

        void regions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            refreshLabels = true;
        }

        void commandExecutor_StatusChanged(object sender, EventArgs e)
        {
            Canvas.Invalidate();
        }

        static RotatedRect CreateEllipseRegion(Point origin, Point location)
        {
            RotatedRect region;
            region.Size = new Size2f(Math.Abs(location.X - origin.X), Math.Abs(location.Y - origin.Y));
            region.Center = new Point2f((location.X + origin.X) / 2f, (location.Y + origin.Y) / 2f);
            region.Angle = 0;
            return region;
        }

        static RotatedRect MoveRegion(RotatedRect region, Point displacement)
        {
            region.Center += new Point2f(displacement);
            return region;
        }

        static RotatedRect ScaleRegion(RotatedRect region, Point target, bool uniformScaling)
        {
            var size = new Size2f(
                2 * Math.Abs(target.X - region.Center.X),
                2 * Math.Abs(target.Y - region.Center.Y));
            if (uniformScaling)
            {
                var sizeNorm = (float)Math.Sqrt(size.Width * size.Width + size.Height * size.Height);
                region.Size.Width = sizeNorm;
                region.Size.Height = sizeNorm;
            }
            else region.Size = size;
            return region;
        }

        void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.PageUp) ImageScale += ScaleIncrement;
            if (e.KeyCode == Keys.PageDown) ImageScale -= ScaleIncrement;
            if (e.Control && e.KeyCode == Keys.Z) commandExecutor.Undo();
            if (e.Control && e.KeyCode == Keys.Y) commandExecutor.Redo();
            if (e.Control && e.KeyCode == Keys.V)
            {
                var roiText = (string)Clipboard.GetData(DataFormats.Text);
                try
                {
                    var mousePosition = PointToClient(MousePosition);
                    var offset = NormalizedLocation(mousePosition.X, mousePosition.Y);
                    var roiData = (float[])ArrayConvert.ToArray(roiText, 1, typeof(float));
                    var center = new Point2f(offset.X, offset.Y);
                    var size = new Size2f(roiData[0], roiData[1]);
                    var roi = new RotatedRect(center, size, 0);

                    var selection = selectedRoi;
                    commandExecutor.Execute(
                        () => AddRegion(roi),
                        () => { regions.Remove(roi); SelectedRegion = selection; });
                }
                catch (ArgumentException) { }
                catch (InvalidCastException) { }
                catch (FormatException) { }
            }

            if (selectedRoi.HasValue)
            {
                if (e.Control && e.KeyCode == Keys.C)
                {
                    var roi = regions[selectedRoi.Value];
                    var roiData = new[] { roi.Size.Width, roi.Size.Height };
                    Clipboard.SetData(DataFormats.Text, ArrayConvert.ToString(roiData));
                }

                if (e.KeyCode == Keys.Delete)
                {
                    var selection = selectedRoi.Value;
                    var region = regions[selection];
                    commandExecutor.Execute(
                        () => { regions.RemoveAt(selection); SelectedRegion = null; },
                        () => { regions.Insert(selection, region); SelectedRegion = selection; });
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Tab && regions.Count > 0)
            {
                SelectedRegion = ((selectedRoi ?? 0) + 1) % regions.Count;
                Canvas.Invalidate();
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        void AddRegion(RotatedRect region)
        {
            regions.Add(region);
            SelectedRegion = regions.Count - 1;
        }

        float TestIntersection(RotatedRect region, Point point)
        {
            var dx = point.X - region.Center.X;
            var dy = point.Y - region.Center.Y;
            var a = region.Size.Width * region.Size.Width / 4;
            var b = region.Size.Height * region.Size.Height / 4;
            return dx * dx / a + dy * dy / b;
        }

        Point NormalizedLocation(int x, int y)
        {
            return new Point(
                Math.Max(0, Math.Min((int)(x * Image.Width / (float)Canvas.Width), Image.Width - 1)),
                Math.Max(0, Math.Min((int)(y * Image.Height / (float)Canvas.Height), Image.Height - 1)));
        }

        Point EnsureSizeRatio(Point origin, Point location, bool square)
        {
            if (square)
            {
                var dx = location.X - origin.X;
                var dy = location.Y - origin.Y;
                var width = Math.Abs(dx);
                var height = Math.Abs(dy);
                if (width < height) location.Y -= Math.Sign(dy) * (height - width);
                else location.X -= Math.Sign(dx) * (width - height);
            }
            return location;
        }

        RectangleF RegionRectangle(RotatedRect region)
        {
            var x = region.Center.X - region.Size.Width / 2f;
            var y = region.Center.Y - region.Size.Height / 2f;
            var width = region.Size.Width;
            var height = region.Size.Height;
            return new RectangleF(x, y, width, height);
        }

        public int? SelectedRegion
        {
            get { return selectedRoi; }
            set
            {
                selectedRoi = value;
                refreshLabels = true;
            }
        }

        public Collection<RotatedRect> Regions
        {
            get { return regions; }
        }

        public event EventHandler RegionsChanged
        {
            add { regions.CollectionChanged += new NotifyCollectionChangedEventHandler(value); }
            remove { regions.CollectionChanged -= new NotifyCollectionChangedEventHandler(value); }
        }

        protected override void OnLoad(EventArgs e)
        {
            if (DesignMode) return;
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            labelTexture = new IplImageTexture();
            base.OnLoad(e);
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            scaleFactor = factor.Width;
            base.ScaleControl(factor, specified);
        }

        private void UpdateLabelTexture()
        {
            if (labelImage != null)
            {
                if (labelFont == null)
                {
                    var emSize = Font.SizeInPoints * (labelImage.Height * LabelFontScale) / Font.Height;
                    labelFont = new Font(Font.FontFamily, emSize);
                }

                labelImage.SetZero();
                using (var labelBitmap = new Bitmap(labelImage.Width, labelImage.Height, labelImage.WidthStep, System.Drawing.Imaging.PixelFormat.Format32bppArgb, labelImage.ImageData))
                using (var graphics = Graphics.FromImage(labelBitmap))
                using (var regionBrush = new SolidBrush(Color.FromArgb(FillOpacity, Color.Red)))
                using (var selectedBrush = new SolidBrush(Color.FromArgb(FillOpacity, Color.LimeGreen)))
                using (var format = new StringFormat())
                {
                    graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;
                    for (int i = 0; i < regions.Count; i++)
                    {
                        var rect = RegionRectangle(regions[i]);
                        var brush = i == selectedRoi ? selectedBrush : regionBrush;
                        graphics.FillEllipse(brush, rect);
                        graphics.DrawString(i.ToString(CultureInfo.InvariantCulture), labelFont, Brushes.White, rect, format);
                    }
                }

                labelTexture.Update(labelImage);
            }
        }

        protected override void SetImage(IplImage image)
        {
            if (image == null) labelImage = null;
            else if (labelImage == null || labelImage.Width != image.Width || labelImage.Height != image.Height)
            {
                labelImage = new IplImage(image.Size, IplDepth.U8, 4);
                refreshLabels = true;
            }
            base.SetImage(image);
        }

        protected override void OnRenderFrame(EventArgs e)
        {
            base.OnRenderFrame(e);
            var image = Image;
            if (image != null)
            {
                GL.Disable(EnableCap.Texture2D);
                GL.Color3(Color.White);
                GL.Enable(EnableCap.Texture2D);
                if (labelImage != null)
                {
                    if (refreshLabels)
                    {
                        UpdateLabelTexture();
                        refreshLabels = false;
                    }
                    labelTexture.Draw();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    MakeCurrent();
                    if (labelTexture != null)
                    {
                        labelTexture.Dispose();
                        labelTexture = null;
                    }

                    if (labelFont != null)
                    {
                        labelFont.Dispose();
                        labelFont = null;
                    }
                    disposed = true;
                }
            }

            base.Dispose(disposing);
        }
    }
}
