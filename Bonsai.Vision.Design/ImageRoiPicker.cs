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
using Point = OpenCV.Net.Point;
using Size = OpenCV.Net.Size;
using Bonsai.Design;

namespace Bonsai.Vision.Design
{
    class ImageRoiPicker : ImageBox
    {
        int? selectedRoi;
        const float LineWidth = 1;
        const float PointSize = 2;
        Collection<Point[]> regions = new Collection<Point[]>();
        CommandExecutor commandExecutor = new CommandExecutor();

        public ImageRoiPicker()
        {
            Canvas.KeyDown += Canvas_KeyDown;
            commandExecutor.StatusChanged += commandExecutor_StatusChanged;
            var mouseDoubleClick = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseDoubleClick").Select(e => e.EventArgs);
            var mouseMove = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseMove").Select(e => e.EventArgs);
            var mouseDown = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseDown").Select(e => e.EventArgs);
            var mouseUp = Observable.FromEventPattern<MouseEventArgs>(Canvas, "MouseUp").Select(e => e.EventArgs);
            
            var roiSelected = from downEvt in mouseDown
                              let location = NormalizedLocation(downEvt.X, downEvt.Y)
                              let selection = (from region in regions.Select((polygon, i) => new { polygon, i = (int?)i })
                                               let distance = TestIntersection(region.polygon, location)
                                               where distance > 0
                                               orderby distance
                                               select region.i)
                                               .FirstOrDefault()
                              select new Action(() => selectedRoi = selection);

            var roiMove = (from downEvt in mouseDown
                           where downEvt.Button == MouseButtons.Left && selectedRoi.HasValue
                           let location = NormalizedLocation(downEvt.X, downEvt.Y)
                           let selection = selectedRoi.Value
                           let region = regions[selection]
                           select (from moveEvt in mouseMove.TakeUntil(mouseUp)
                                   let target = NormalizedLocation(moveEvt.X, moveEvt.Y)
                                   let displacement = target - location
                                   let displacedRegion = region.Select(point => point + displacement).ToArray()
                                   select displacedRegion)
                                   .Publish(ps =>
                                       ps.TakeLast(1).Do(displacedRegion =>
                                           commandExecutor.Execute(
                                               () => regions[selection] = displacedRegion,
                                               () => regions[selection] = region))
                                         .Merge(ps))
                                   .Select(displacedRegion => new Action(() => regions[selectedRoi.Value] = displacedRegion)))
                           .Switch();

            var pointMove = (from downEvt in mouseDown
                             where downEvt.Button == MouseButtons.Right && selectedRoi.HasValue
                             let location = NormalizedLocation(downEvt.X, downEvt.Y)
                             let selection = selectedRoi.Value
                             let region = regions[selection]
                             let nearestPoint = NearestPoint(region, location)
                             let source = regions[selection][nearestPoint]
                             select (from moveEvt in mouseMove.TakeUntil(mouseUp)
                                     let target = NormalizedLocation(moveEvt.X, moveEvt.Y)
                                     select target)
                                     .Publish(ps =>
                                         ps.TakeLast(1).Do(target =>
                                             commandExecutor.Execute(
                                                 () => regions[selection][nearestPoint] = target,
                                                 () => regions[selection][nearestPoint] = source))
                                           .Merge(ps))
                                     .Select(target => new Action(() => regions[selection][nearestPoint] = target)))
                            .Switch();

            var regionInsertion = (from downEvt in mouseDown
                                   where downEvt.Button == MouseButtons.Left && !selectedRoi.HasValue
                                   let count = regions.Count
                                   let origin = NormalizedLocation(downEvt.X, downEvt.Y)
                                   select (from moveEvt in mouseMove.TakeUntil(mouseUp)
                                           let location = NormalizedLocation(moveEvt.X, moveEvt.Y)
                                           select new[]
                                           {
                                               origin, new Point(location.X, origin.Y),
                                               location, new Point(origin.X, location.Y)
                                           })
                                           .Publish(ps =>
                                               ps.TakeLast(1).Do(region =>
                                                   commandExecutor.Execute(
                                                       () => { if (count == regions.Count) AddRegion(region); },
                                                       () => { regions.Remove(region); selectedRoi = null; }))
                                                 .Merge(ps))
                                           .Select(region => new Action(() =>
                                           {
                                               if (selectedRoi.HasValue) regions[selectedRoi.Value] = region;
                                               else AddRegion(region);
                                           })))
                                   .Switch();

            var pointInsertion = from clickEvt in mouseDoubleClick
                                 where clickEvt.Button == MouseButtons.Left && selectedRoi.HasValue
                                 let location = NormalizedLocation(clickEvt.X, clickEvt.Y)
                                 let region = regions[selectedRoi.Value]
                                 let nearestLine = NearestLine(region, location)
                                 select new Action(() =>
                                 {
                                     var resizeRegion = region;
                                     var line0 = region[nearestLine.Item1];
                                     var line1 = region[nearestLine.Item2];
                                     var midPoint = new Point((line0.X + line1.X) / 2, (line0.Y + line1.Y) / 2);
                                     Array.Resize(ref resizeRegion, resizeRegion.Length + 1);
                                     for (int i = resizeRegion.Length - 1; i > nearestLine.Item2; i--)
                                     {
                                         resizeRegion[i] = resizeRegion[i - 1];
                                     }

                                     var selection = selectedRoi.Value;
                                     resizeRegion[nearestLine.Item2] = midPoint;
                                     commandExecutor.Execute(
                                         () => regions[selection] = resizeRegion,
                                         () => regions[selection] = region);
                                 });

            var pointDeletion = from clickEvt in mouseDoubleClick
                                where clickEvt.Button == MouseButtons.Right && selectedRoi.HasValue
                                let region = regions[selectedRoi.Value]
                                where region.Length > 3
                                let location = NormalizedLocation(clickEvt.X, clickEvt.Y)
                                let nearestPoint = NearestPoint(region, location)
                                select new Action(() =>
                                {
                                    var selection = selectedRoi.Value;
                                    var resizeRegion = new Point[region.Length - 1];
                                    Array.Copy(region, resizeRegion, nearestPoint);
                                    Array.Copy(region, nearestPoint + 1, resizeRegion, nearestPoint, region.Length - nearestPoint - 1);
                                    commandExecutor.Execute(
                                         () => regions[selection] = resizeRegion,
                                         () => regions[selection] = region);
                                });

            var roiActions = Observable.Merge(roiSelected, pointMove, roiMove, pointInsertion, pointDeletion, regionInsertion);
            roiActions.Subscribe(action =>
            {
                action();
            });
        }

        void commandExecutor_StatusChanged(object sender, EventArgs e)
        {
            Canvas.Invalidate();
        }

        static float PointLineSegmentDistance(Point point, Point line0, Point line1)
        {
            return PointLineSegmentDistance(new Vector2(point.X, point.Y), new Vector2(line0.X, line0.Y), new Vector2(line1.X, line1.Y));
        }

        static float PointLineSegmentDistance(Vector2 point, Vector2 line0, Vector2 line1)
        {
            var segmentLengthSquared = (line1 - line0).LengthSquared;
            if (segmentLengthSquared == 0) return (point - line0).Length; // line0 == line1

            // The line that extends the segment is parameterized as line0 + t (line1 - line0).
            // Projection of point on line then has t = [(point-line0) . (line1-line0)] / |line1-line0|^2
            var t = Vector2.Dot(point - line0, line1 - line0) / segmentLengthSquared;

            if (t < 0.0) return (point - line0).Length; // Beyond line0 end of the segment
            else if (t > 1.0) return (point - line1).Length; // Beyond line1 end of the segment

            // Projection falls on the segment
            var projection = line0 + t * (line1 - line0);
            return (point - projection).Length;
        }

        void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z) commandExecutor.Undo();
            if (e.Control && e.KeyCode == Keys.Y) commandExecutor.Redo();
            if (e.Control && e.KeyCode == Keys.V)
            {
                var roiText = (string)Clipboard.GetData(DataFormats.Text);
                try
                {
                    var mousePosition = PointToClient(MousePosition);
                    var offset = NormalizedLocation(mousePosition.X, mousePosition.Y);
                    var roiData = (int[])ArrayConvert.ToArray(roiText, 1, typeof(int));
                    var roi = new Point[roiData.Length / 2];
                    for (int i = 0, k = 0; i < roi.Length && k < roiData.Length; i++, k += 2)
                    {
                        roi[i].X = roiData[k + 0] - roiData[0] + offset.X;
                        roi[i].Y = roiData[k + 1] - roiData[1] + offset.Y;
                    }
                    
                    var selection = selectedRoi;
                    commandExecutor.Execute(
                        () => AddRegion(roi),
                        () => { regions.Remove(roi); selectedRoi = selection; });
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
                    var roiData = new int[roi.Length * 2];
                    for (int i = 0, k = 0; i < roi.Length && k < roiData.Length; i++, k += 2)
                    {
                        roiData[k + 0] = roi[i].X;
                        roiData[k + 1] = roi[i].Y;
                    }
                    Clipboard.SetData(DataFormats.Text, ArrayConvert.ToString(roiData));
                }

                if (e.KeyCode == Keys.Delete)
                {
                    var selection = selectedRoi.Value;
                    var region = regions[selection];
                    commandExecutor.Execute(
                        () => { regions.RemoveAt(selection); selectedRoi = null; },
                        () => { regions.Insert(selection, region); selectedRoi = selection; });
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Tab && regions.Count > 0)
            {
                selectedRoi = ((selectedRoi ?? 0) + 1) % regions.Count;
                Canvas.Invalidate();
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        void AddRegion(Point[] region)
        {
            regions.Add(region);
            selectedRoi = regions.Count - 1;
        }

        double TestIntersection(Point[] region, Point point)
        {
            var regionHandle = GCHandle.Alloc(region, GCHandleType.Pinned);
            try
            {
                using (var mat = new Mat(region.Length, 1, Depth.S32, 2, regionHandle.AddrOfPinnedObject()))
                {
                    return CV.PointPolygonTest(mat, new Point2f(point.X, point.Y), true);
                }
            }
            finally { regionHandle.Free(); }
        }

        Rect ClipRectangle(Rect rect)
        {
            var clipX = rect.X < 0 ? -rect.X : 0;
            var clipY = rect.Y < 0 ? -rect.Y : 0;
            clipX += Math.Max(0, rect.X + rect.Width - Image.Width);
            clipY += Math.Max(0, rect.Y + rect.Height - Image.Height);

            rect.X = Math.Max(0, rect.X);
            rect.Y = Math.Max(0, rect.Y);
            rect.Width = rect.Width - clipX;
            rect.Height = rect.Height - clipY;
            return rect;
        }

        Point NormalizedLocation(int x, int y)
        {
            return new Point(
                (int)(x * Image.Width / (float)Canvas.Width),
                (int)(y * Image.Height / (float)Canvas.Height));
        }

        Rect NormalizedRectangle(Rect rect)
        {
            return new Rect(
                (int)(rect.X * Image.Width / (float)Canvas.Width),
                (int)(rect.Y * Image.Height / (float)Canvas.Height),
                (int)(rect.Width * Image.Width / (float)Canvas.Width),
                (int)(rect.Height * Image.Width / (float)Canvas.Width));
        }

        Tuple<int, int> NearestLine(Point[] region, Point location)
        {
            var pointIndex = region
                .Concat(Enumerable.Repeat(region[0], 1))
                .Select((p, i) => new { p, i = i % region.Length });

            return (from line in pointIndex.Zip(pointIndex.Skip(1), (l0, l1) => Tuple.Create(l0, l1))
                    let lineDistance = PointLineSegmentDistance(location, line.Item1.p, line.Item2.p)
                    orderby lineDistance ascending
                    select Tuple.Create(line.Item1.i, line.Item2.i))
                    .FirstOrDefault();
        }

        int NearestPoint(Point[] region, Point location)
        {
            return (from point in region.Select((p, i) => new { p, i })
                    let distanceX = location.X - point.p.X
                    let distanceY = location.Y - point.p.Y
                    orderby distanceX * distanceX + distanceY * distanceY ascending
                    select point.i)
                    .FirstOrDefault();
        }

        public int? MaxRegions { get; set; }

        public int? SelectedRegion
        {
            get { return selectedRoi; }
            set { selectedRoi = value; }
        }

        public Collection<Point[]> Regions
        {
            get { return regions; }
        }

        public event EventHandler RegionsChanged;

        protected virtual void OnRegionsChanged(EventArgs e)
        {
            var handler = RegionsChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void RenderRegion(Point[] region, PrimitiveType mode, Color color, Size imageSize)
        {
            GL.Color3(color);
            GL.Begin(mode);
            for (int i = 0; i < region.Length; i++)
            {
                GL.Vertex2(DrawingHelper.NormalizePoint(region[i], imageSize));
            }
            GL.End();
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.LineWidth(LineWidth);
            GL.PointSize(PointSize);
            GL.Enable(EnableCap.PointSmooth);
            base.OnLoad(e);
        }

        protected override void OnRenderFrame(EventArgs e)
        {
            GL.Color3(Color.White);
            base.OnRenderFrame(e);

            var image = Image;
            if (image != null)
            {
                GL.Disable(EnableCap.Texture2D);
                foreach (var region in regions.Where((region, i) => i != selectedRoi))
                {
                    RenderRegion(region, PrimitiveType.LineLoop, Color.Red, image.Size);
                }

                if (selectedRoi.HasValue)
                {
                    var region = regions[selectedRoi.Value];
                    RenderRegion(region, PrimitiveType.LineLoop, Color.LimeGreen, image.Size);
                    RenderRegion(region, PrimitiveType.Points, Color.Blue, image.Size);
                }
            }
        }
    }
}
