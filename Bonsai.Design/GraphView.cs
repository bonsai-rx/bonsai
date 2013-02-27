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
        static readonly Pen WhitePen = new Pen(Brushes.White, PenWidth);
        static readonly Pen BlackPen = new Pen(Brushes.Black, PenWidth);

        static readonly object EventItemDrag = new object();
        static readonly object EventNodeMouseClick = new object();
        static readonly object EventNodeMouseDoubleClick = new object();
        static readonly object EventNodeMouseHover = new object();
        static readonly object EventSelectedNodeChanged = new object();
        LayoutNodeCollection layoutNodes = new LayoutNodeCollection();

        GraphNode selectedNode;
        IEnumerable<GraphNodeGrouping> nodes;

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
                .Select(evt => evt.EventArgs);

            var mouseUpEvent = Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                handler => canvas.MouseUp += handler,
                handler => canvas.MouseUp -= handler)
                .Select(evt => evt.EventArgs);

            var mouseMoveEvent = Observable.FromEventPattern<MouseEventHandler, MouseEventArgs>(
                handler => canvas.MouseMove += handler,
                handler => canvas.MouseMove -= handler)
                .Select(evt => evt.EventArgs);

            var itemDrag = (from mouseDown in mouseDownEvent
                            where mouseDown.Button == MouseButtons.Left
                            let node = GetNodeAt(mouseDown.Location)
                            where node != null
                            select from mouseMove in mouseMoveEvent.TakeUntil(mouseUpEvent)
                                   let displacementX = mouseMove.X - mouseDown.X
                                   let displacementY = mouseMove.Y - mouseDown.Y
                                   where mouseMove.Button == MouseButtons.Left &&
                                         displacementX * displacementX + displacementY * displacementY > 16
                                   select new { node, mouseMove.Button }).Switch();

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
                nodes = value;
                SelectedNode = null;
                UpdateModelLayout();
            }
        }

        public GraphNode SelectedNode
        {
            get { return selectedNode; }
            set
            {
                if (selectedNode != value)
                {
                    InvalidateNode(selectedNode);
                    selectedNode = value;
                    InvalidateNode(selectedNode);
                    OnSelectedNodeChanged(EventArgs.Empty);
                }
            }
        }

        void InvalidateNode(GraphNode node)
        {
            if (selectedNode != null)
            {
                var nodeLayout = layoutNodes[selectedNode];
                var boundingRectangle = nodeLayout.BoundingRectangle;
                boundingRectangle.X -= canvas.HorizontalScroll.Value;
                boundingRectangle.Y -= canvas.VerticalScroll.Value;

                canvas.Invalidate(boundingRectangle);
            }
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
            InvalidateNode(selectedNode);
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            InvalidateNode(selectedNode);
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
            switch (keyData)
            {
                case Keys.Up: selectionOffset = new Size(0, -NodeAirspace); break;
                case Keys.Down: selectionOffset = new Size(0, NodeAirspace); break;
                case Keys.Left: selectionOffset = new Size(-NodeAirspace, 0); break;
                case Keys.Right: selectionOffset = new Size(NodeAirspace, 0); break;
                default: selectionOffset = Size.Empty; break;
            }

            if (selectionOffset != Size.Empty)
            {
                selectionOffset -= new Size(canvas.HorizontalScroll.Value, canvas.VerticalScroll.Value);
                SelectedNode = SelectedNode == null
                    ? layoutNodes.Select(layoutNode => layoutNode.Node).FirstOrDefault()
                    : GetClosestNodeTo(Point.Add(layoutNodes[SelectedNode].Location, selectionOffset));
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        float SquaredDistance(ref Point point, ref Point center)
        {
            var xdiff = point.X - center.X;
            var ydiff = point.Y - center.Y;
            return xdiff * xdiff + ydiff * ydiff;
        }

        bool CircleIntersect(Point point, Point center, int radius)
        {
            return SquaredDistance(ref point, ref center) <= radius * radius;
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

                if (CircleIntersect(point, layout.Center, HalfSize))
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

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            var offset = new Size(-canvas.HorizontalScroll.Value, -canvas.VerticalScroll.Value);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (var layout in layoutNodes)
            {
                if (layout.Node.Value != null)
                {
                    var selected = layout.Node == SelectedNode;
                    var nodeRectangle = new Rectangle(
                        layout.Location.X + offset.Width,
                        layout.Location.Y + offset.Height,
                        NodeSize, NodeSize);

                    var pen = selected ? WhitePen : BlackPen;
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
        }

        private void canvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (!Focused) Select();

            var node = GetNodeAt(e.Location);
            SelectedNode = node;
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
