using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Bonsai.Design
{
    public partial class GraphView : UserControl
    {
        const int PenWidth = 3;
        const int NodeAirspace = 80;
        const int NodeSize = 30;
        const int HalfSize = NodeSize / 2;
        static readonly Size TextOffset = new Size(9, 9);
        static readonly Size EntryOffset = new Size(-PenWidth / 2, NodeSize / 2);
        static readonly Size ExitOffset = new Size(NodeSize + PenWidth / 2, NodeSize / 2);
        static readonly Pen RubberBandPen = new Pen(Color.FromArgb(51, 153, 255));
        static readonly Brush RubberBandBrush = new SolidBrush(Color.FromArgb(128, 170, 204, 238));
        static readonly Pen CursorPen = new Pen(Brushes.Gray, PenWidth);
        static readonly Pen WhitePen = new Pen(Brushes.White, PenWidth);
        static readonly Pen BlackPen = new Pen(Brushes.Black, PenWidth);

        static readonly object EventItemDrag = new object();
        static readonly object EventNodeMouseClick = new object();
        static readonly object EventNodeMouseDoubleClick = new object();
        static readonly object EventNodeMouseHover = new object();
        static readonly object EventSelectedNodeChanged = new object();

        Rectangle rubberBand;
        Rectangle previousRectangle;
        GraphNode[] previousSelection;
        LayoutNodeCollection layoutNodes = new LayoutNodeCollection();
        HashSet<GraphNode> selectedNodes = new HashSet<GraphNode>();
        IEnumerable<GraphNodeGrouping> nodes;
        GraphNode pivot;
        GraphNode cursor;

        public GraphView()
        {
            FocusedSelectionBrush = Brushes.Black;
            UnfocusedSelectionBrush = Brushes.Gray;
            InitializeComponent();
            InitializeReactiveEvents();
        }

        void InitializeReactiveEvents()
        {
            var mouseDownEvent = Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                handler => canvas.MouseDown += handler,
                handler => canvas.MouseDown -= handler)
                .Select(evt => evt.EventArgs)
                .Where(mouseDown => mouseDown.Button == MouseButtons.Left);

            var mouseUpEvent = Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                handler => canvas.MouseUp += handler,
                handler => canvas.MouseUp -= handler)
                .Select(evt => evt.EventArgs)
                .Where(mouseUp => mouseUp.Button == MouseButtons.Left);

            var mouseMoveEvent = Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                handler => canvas.MouseMove += handler,
                handler => canvas.MouseMove -= handler)
                .Select(evt => evt.EventArgs);

            var canvasDrag = (from mouseDown in mouseDownEvent
                              let node = GetNodeAt(mouseDown.Location)
                              select new { mouseDown, node })
                              .Publish()
                              .RefCount();

            var selectionDrag = (from drag in canvasDrag
                                 where drag.node == null
                                 select (from mouseMove in mouseMoveEvent.TakeUntil(mouseUpEvent)
                                         let displacementX = mouseMove.X - drag.mouseDown.X
                                         let displacementY = mouseMove.Y - drag.mouseDown.Y
                                         where mouseMove.Button == MouseButtons.Left &&
                                               displacementX * displacementX + displacementY * displacementY > 16
                                         select (Rectangle?)GetNormalizedRectangle(drag.mouseDown.Location, mouseMove.Location))
                                         .Concat(Observable.Return<Rectangle?>(null)))
                                         .SelectMany(selection => selection.Select((rect, i) =>
                                         {
                                             if (i == 0) previousSelection = selectedNodes.ToArray();
                                             return rect;
                                         }).Finally(() => previousSelection = null));

            var itemDrag = (from drag in canvasDrag
                            where drag.node != null
                            select (from mouseMove in mouseMoveEvent.TakeUntil(mouseUpEvent)
                                    let displacementX = mouseMove.X - drag.mouseDown.X
                                    let displacementY = mouseMove.Y - drag.mouseDown.Y
                                    where mouseMove.Button == MouseButtons.Left &&
                                          displacementX * displacementX + displacementY * displacementY > 16
                                    select new { drag.node, mouseMove.Button })
                                    .Take(1)).Switch();

            var tooltipTimerTickEvent = Observable.FromEventPattern<EventHandler, EventArgs>(
                handler => tooltipTimer.Tick += handler,
                handler => tooltipTimer.Tick -= handler);

            var hideTooltip = false;
            var tooltipShown = false;
            tooltipTimerTickEvent.Subscribe(tick =>
            {
                if (tooltipShown) hideTooltip = true;
                tooltipTimer.Stop();
            });

            var showTooltip = from tick in tooltipTimerTickEvent
                              where !tooltipShown
                              let mousePosition = PointToClient(MousePosition)
                              let node = GetNodeAt(mousePosition)
                              where node != null
                              select new { node, mousePosition };

            selectionDrag.Subscribe(ProcessRubberBand);
            itemDrag.Subscribe(drag => OnItemDrag(new ItemDragEventArgs(drag.Button, drag.node)));
            showTooltip.Subscribe(show => { toolTip.Show(show.node.Text, canvas, show.mousePosition); tooltipShown = true; });
            mouseMoveEvent.Subscribe(mouseMove =>
            {
                if (hideTooltip)
                {
                    toolTip.Hide(canvas);
                    hideTooltip = false;
                    tooltipShown = false;
                }

                tooltipTimer.Stop();
                tooltipTimer.Start();
            });
        }

        public event ItemDragEventHandler ItemDrag
        {
            add { Events.AddHandler(EventItemDrag, value); }
            remove { Events.RemoveHandler(EventItemDrag, value); }
        }

        public event EventHandler<GraphNodeMouseClickEventArgs> NodeMouseClick
        {
            add { Events.AddHandler(EventNodeMouseClick, value); }
            remove { Events.RemoveHandler(EventNodeMouseClick, value); }
        }

        public event EventHandler<GraphNodeMouseClickEventArgs> NodeMouseDoubleClick
        {
            add { Events.AddHandler(EventNodeMouseDoubleClick, value); }
            remove { Events.RemoveHandler(EventNodeMouseDoubleClick, value); }
        }

        public event EventHandler<GraphNodeMouseHoverEventArgs> NodeMouseHover
        {
            add { Events.AddHandler(EventNodeMouseHover, value); }
            remove { Events.RemoveHandler(EventNodeMouseHover, value); }
        }

        public event EventHandler SelectedNodeChanged
        {
            add { Events.AddHandler(EventSelectedNodeChanged, value); }
            remove { Events.RemoveHandler(EventSelectedNodeChanged, value); }
        }

        public Brush FocusedSelectionBrush { get; set; }

        public Brush UnfocusedSelectionBrush { get; set; }

        public IEnumerable<GraphNodeGrouping> Nodes
        {
            get { return nodes; }
            set
            {
                pivot = null;
                nodes = value;
                SelectedNode = null;
                UpdateModelLayout();
            }
        }

        public GraphNode SelectedNode
        {
            get { return selectedNodes.FirstOrDefault(); }
            set
            {
                var selectedNode = SelectedNode;
                if (selectedNode != value)
                {
                    InvalidateSelection();
                    selectedNodes.Clear();
                    if (value != null) selectedNodes.Add(value);
                    InvalidateSelection();
                    OnSelectedNodeChanged(EventArgs.Empty);
                }
            }
        }

        public IEnumerable<GraphNode> SelectedNodes
        {
            get { return selectedNodes; }
        }

        void UpdateSelection(Action update)
        {
            InvalidateSelection();
            update();
            InvalidateSelection();
            OnSelectedNodeChanged(EventArgs.Empty);
        }

        void InvalidateSelection()
        {
            foreach (var selectedNode in selectedNodes)
            {
                InvalidateNode(selectedNode);
            }
        }

        void InvalidateNode(GraphNode node)
        {
            canvas.Invalidate(GetBoundingRectangle(node));
        }

        Rectangle GetBoundingRectangle(GraphNode node)
        {
            var nodeLayout = layoutNodes[node];
            var boundingRectangle = nodeLayout.BoundingRectangle;
            boundingRectangle.X -= canvas.HorizontalScroll.Value;
            boundingRectangle.Y -= canvas.VerticalScroll.Value;
            return boundingRectangle;
        }

        protected virtual void OnItemDrag(ItemDragEventArgs e)
        {
            var handler = Events[EventItemDrag] as ItemDragEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnNodeMouseClick(GraphNodeMouseClickEventArgs e)
        {
            var handler = Events[EventNodeMouseClick] as EventHandler<GraphNodeMouseClickEventArgs>;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnNodeMouseDoubleClick(GraphNodeMouseClickEventArgs e)
        {
            var handler = Events[EventNodeMouseDoubleClick] as EventHandler<GraphNodeMouseClickEventArgs>;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnNodeMouseHover(GraphNodeMouseHoverEventArgs e)
        {
            var handler = Events[EventNodeMouseHover] as EventHandler<GraphNodeMouseHoverEventArgs>;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSelectedNodeChanged(EventArgs e)
        {
            var handler = Events[EventSelectedNodeChanged] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            InvalidateSelection();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            InvalidateSelection();
            base.OnLostFocus(e);
        }

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            canvas.Invalidate(e.InvalidRect);
            base.OnInvalidated(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Size selectionOffset;
            var shift = keyData.HasFlag(Keys.Shift);
            if (shift) keyData &= ~Keys.Shift;
            var control = keyData.HasFlag(Keys.Control);
            if (control) keyData &= ~Keys.Control;

            switch (keyData)
            {
                case Keys.Up: selectionOffset = new Size(0, -NodeAirspace); break;
                case Keys.Down: selectionOffset = new Size(0, NodeAirspace); break;
                case Keys.Left: selectionOffset = new Size(-NodeAirspace, 0); break;
                case Keys.Right: selectionOffset = new Size(NodeAirspace, 0); break;
                default: selectionOffset = Size.Empty; break;
            }

            if (cursor != null && selectionOffset != Size.Empty)
            {
                selectionOffset -= new Size(canvas.HorizontalScroll.Value, canvas.VerticalScroll.Value);
                SetCursor(GetClosestNodeTo(Point.Add(layoutNodes[cursor].Location, selectionOffset)));
                if (shift)
                {
                    SelectRange(cursor, control);
                }
                else
                {
                    pivot = cursor;
                    if (!control) SelectNode(cursor, false);
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        IEnumerable<GraphNode> GetRubberBandSelection(Rectangle rect)
        {
            var offset = new Size(canvas.HorizontalScroll.Value, canvas.VerticalScroll.Value);
            var selectionRect = new Rectangle(Point.Add(rect.Location, offset), rect.Size);
            foreach (var layout in layoutNodes)
            {
                if (layout.Node.Value != null)
                {
                    var nodeCenter = new Point(
                        layout.Location.X + offset.Width,
                        layout.Location.Y + offset.Height);
                    var selected = CircleIntersect(layout.Center, HalfSize, selectionRect);
                    if (selected)
                    {
                        yield return layout.Node;
                    }
                }
            }
        }

        void ProcessRubberBand(Rectangle? rect)
        {
            if (!Focused) Select();
            var selectionRect = Rectangle.Empty;
            if (rect.HasValue)
            {
                var selection = new HashSet<GraphNode>(GetRubberBandSelection(rect.Value));
                if (Control.ModifierKeys.HasFlag(Keys.Control))
                {
                    selection.SymmetricExceptWith(previousSelection);
                }
                else if (Control.ModifierKeys.HasFlag(Keys.Shift))
                {
                    selection.UnionWith(previousSelection);
                }

                var selectionChanged = !selection.SetEquals(selectedNodes);
                if (selectionChanged)
                {
                    foreach (var node in selectedNodes) selectionRect = Rectangle.Union(selectionRect, GetBoundingRectangle(node));
                    selectedNodes.Clear();
                    foreach (var node in selection)
                    {
                        selectedNodes.Add(node);
                        selectionRect = Rectangle.Union(selectionRect, GetBoundingRectangle(node));
                    }
                    OnSelectedNodeChanged(EventArgs.Empty);
                }
            }

            rubberBand = rect.GetValueOrDefault();
            var invalidateRect = rubberBand;
            invalidateRect.Inflate(PenWidth, PenWidth);
            invalidateRect = (selectionRect.Width > 0 || selectionRect.Height > 0) ? Rectangle.Union(invalidateRect, selectionRect) : invalidateRect;
            canvas.Invalidate(Rectangle.Union(invalidateRect, previousRectangle));
            previousRectangle = invalidateRect;
        }

        Rectangle GetNormalizedRectangle(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p2.X - p1.X),
                Math.Abs(p2.Y - p1.Y));
        }

        float SquaredDistance(ref Point point, ref Point center)
        {
            var xdiff = point.X - center.X;
            var ydiff = point.Y - center.Y;
            return xdiff * xdiff + ydiff * ydiff;
        }

        bool CircleIntersect(Point center, int radius, Point point)
        {
            return SquaredDistance(ref point, ref center) <= radius * radius;
        }

        bool CircleIntersect(Point center, int radius, Rectangle rect)
        {
            float closestX = Math.Max(rect.Left, Math.Min(center.X, rect.Right));
            float closestY = Math.Max(rect.Top, Math.Min(center.Y, rect.Bottom));
            float distanceX = center.X - closestX;
            float distanceY = center.Y - closestY;
            float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
            return distanceSquared < (radius * radius);
        }

        public Point GetNodeLocation(GraphNode node)
        {
            return layoutNodes[node].Location;
        }

        public GraphNode GetNodeAt(Point point)
        {
            point.X += canvas.HorizontalScroll.Value;
            point.Y += canvas.VerticalScroll.Value;

            foreach (var layout in layoutNodes)
            {
                if (layout.Node.Value == null) continue;

                if (CircleIntersect(layout.Center, HalfSize, point))
                {
                    return layout.Node;
                }
            }

            return null;
        }

        public GraphNode GetClosestNodeTo(Point point)
        {
            point.X += canvas.HorizontalScroll.Value;
            point.Y += canvas.VerticalScroll.Value;

            float minDistance = 0;
            GraphNode closest = null;
            foreach (var layout in layoutNodes)
            {
                if (layout.Node.Value == null) continue;

                var center = layout.Center;
                var distance = SquaredDistance(ref point, ref center);
                if (closest == null || distance < minDistance)
                {
                    closest = layout.Node;
                    minDistance = distance;
                }
            }

            return closest;
        }

        private void UpdateModelLayout()
        {
            layoutNodes.Clear();
            var model = Nodes;
            Size size = Size.Empty;
            if (model != null)
            {
                var layerCount = model.Count();
                foreach (var layer in model)
                {
                    var column = layerCount - layer.Key - 1;
                    foreach (var node in layer)
                    {
                        if (pivot == null) pivot = cursor = node;
                        var row = node.LayerIndex;
                        var location = new Point(column * NodeAirspace + PenWidth, row * NodeAirspace + PenWidth);
                        layoutNodes.Add(new LayoutNode(node, location));
                    }

                    var rowHeight = layer.Count * NodeAirspace;
                    size.Height = Math.Max(rowHeight, size.Height);
                }

                size.Width = layerCount * NodeAirspace;
            }

            canvas.AutoScrollMinSize = size;
        }

        private static IEnumerable<GraphNode> GetAllPaths(GraphNode from, GraphNode to)
        {
            if (from == to) yield return from;
            else foreach (var successor in from.Successors)
            {
                var inPath = false;
                var successorPaths = GetAllPaths(successor.Node, to);
                foreach (var node in successorPaths)
                {
                    inPath = true;
                    yield return node;
                }

                if (inPath) yield return from;
            }
        }

        private static IEnumerable<TSource> ConcatEmpty<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            var any = false;
            foreach (var xs in first)
            {
                any = true;
                yield return xs;
            }

            if (!any) foreach (var xs in second) yield return xs;
        }

        private IEnumerable<GraphNode> GetSelectionRange(GraphNode from, GraphNode to)
        {
            return ConcatEmpty(
                ConcatEmpty(GetAllPaths(from, to), GetAllPaths(to, from)),
                selectedNodes);
        }

        private void SelectRange(GraphNode node, bool unionUpdate)
        {
            var path = GetSelectionRange(pivot, node);
            if (unionUpdate)
            {
                UpdateSelection(() => selectedNodes.UnionWith(path));
            }
            else
            {
                UpdateSelection(() =>
                {
                    selectedNodes.Clear();
                    foreach (var element in path) selectedNodes.Add(element);
                });
            }
        }

        private void SelectNode(GraphNode node, bool toggle)
        {
            if (toggle)
            {
                UpdateSelection(() =>
                {
                    var found = selectedNodes.Remove(node);
                    if (!found) selectedNodes.Add(node);
                });
            }
            else
            {
                UpdateSelection(() =>
                {
                    selectedNodes.Clear();
                    selectedNodes.Add(node);
                });
            }
        }

        private void ClearSelection()
        {
            UpdateSelection(() => selectedNodes.Clear());
        }

        private void SetCursor(GraphNode node)
        {
            InvalidateNode(cursor);
            cursor = node;
            InvalidateNode(node);
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            var offset = new Size(-canvas.HorizontalScroll.Value, -canvas.VerticalScroll.Value);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (var layout in layoutNodes)
            {
                if (layout.Node.Value != null)
                {
                    var selected = selectedNodes.Contains(layout.Node);
                    var nodeRectangle = new Rectangle(
                        layout.Location.X + offset.Width,
                        layout.Location.Y + offset.Height,
                        NodeSize, NodeSize);

                    var pen = cursor == layout.Node ? CursorPen : selected ? WhitePen : BlackPen;
                    var brush = selected ? (Focused ? FocusedSelectionBrush : UnfocusedSelectionBrush) : layout.Node.Brush;
                    var textBrush = selected ? Brushes.White : Brushes.Black;

                    e.Graphics.DrawEllipse(pen, nodeRectangle);
                    e.Graphics.FillEllipse(brush, nodeRectangle);
                    e.Graphics.DrawString(
                        layout.Node.Text.Substring(0, 1),
                        Font, textBrush,
                        Point.Add(layout.Location, Size.Add(offset, TextOffset)));
                }
                else e.Graphics.DrawLine(((GraphEdge)layout.Node.Tag).Pen, Point.Add(layout.EntryPoint, offset), Point.Add(layout.ExitPoint, offset));

                foreach (var successor in layout.Node.Successors)
                {
                    var successorLayout = layoutNodes[successor.Node];
                    e.Graphics.DrawLine(successor.Pen, Point.Add(layout.ExitPoint, offset), Point.Add(successorLayout.EntryPoint, offset));
                }
            }

            if (rubberBand.Width > 0 && rubberBand.Height > 0)
            {
                e.Graphics.FillRectangle(RubberBandBrush, rubberBand);
                e.Graphics.DrawRectangle(RubberBandPen, rubberBand);
            }
        }

        private void canvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (previousSelection != null) return;
            if (!Focused) Select();

            var node = GetNodeAt(e.Location);
            if (node != null)
            {
                if (Control.ModifierKeys.HasFlag(Keys.Shift))
                {
                    SelectRange(node, Control.ModifierKeys.HasFlag(Keys.Control));
                }
                else
                {
                    SetCursor(node);
                    pivot = cursor;
                    SelectNode(node, Control.ModifierKeys.HasFlag(Keys.Control));
                }
            }
            else if (Control.ModifierKeys == Keys.None)
            {
                ClearSelection();
            }

            if (node != null)
            {
                OnNodeMouseClick(new GraphNodeMouseClickEventArgs(node, e.Button, e.Clicks, e.X, e.Y, e.Delta));
            }
        }

        private void canvas_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var node = GetNodeAt(e.Location);
            if (node != null)
            {
                OnNodeMouseDoubleClick(new GraphNodeMouseClickEventArgs(node, e.Button, e.Clicks, e.X, e.Y, e.Delta));
            }
        }

        private void canvas_MouseHover(object sender, EventArgs e)
        {
            var node = GetNodeAt(PointToClient(MousePosition));
            if (node != null)
            {
                OnNodeMouseHover(new GraphNodeMouseHoverEventArgs(node));
            }
        }

        class LayoutNodeCollection : KeyedCollection<GraphNode, LayoutNode>
        {
            protected override GraphNode GetKeyForItem(LayoutNode item)
            {
                return item.Node;
            }
        }

        class LayoutNode
        {
            public LayoutNode(GraphNode node, Point location)
            {
                Node = node;
                Location = location;
            }

            public GraphNode Node { get; set; }

            public Point Location { get; set; }

            public Point Center
            {
                get { return Point.Add(Location, new Size(HalfSize, HalfSize)); }
            }

            public Point EntryPoint
            {
                get { return Point.Add(Location, EntryOffset); }
            }

            public Point ExitPoint
            {
                get { return Point.Add(Location, ExitOffset); }
            }

            public Rectangle BoundingRectangle
            {
                get
                {
                    return new Rectangle(
                        Location.X - PenWidth, Location.Y - PenWidth,
                        NodeSize + 2 * PenWidth, NodeSize + 2 * PenWidth);
                }
            }
        }
    }
}
