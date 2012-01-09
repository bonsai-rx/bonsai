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

namespace Bonsai.Design
{
    public partial class GraphView : UserControl
    {
        const int PenWidth = 3;
        const int NodeAirspace = 80;
        const int NodeSize = 30;
        const int HalfSize = NodeSize / 2;
        static readonly Pen WhitePen = new Pen(Brushes.White, PenWidth);
        static readonly Pen BlackPen = new Pen(Brushes.Black, PenWidth);

        static readonly object EventNodeMouseClick = new object();
        static readonly object EventNodeMouseDoubleClick = new object();
        static readonly object EventNodeMouseHover = new object();
        static readonly object EventSelectedNodeChanged = new object();
        LayoutNodeCollection nodes = new LayoutNodeCollection();

        GraphNode selectedNode;
        IEnumerable<GraphNodeGrouping> model;

        public GraphView()
        {
            InitializeComponent();
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

        public IEnumerable<GraphNodeGrouping> Model
        {
            get { return model; }
            set
            {
                model = value;
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
                var nodeLayout = nodes[selectedNode];
                canvas.Invalidate(nodeLayout.BoundingRectangle);
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

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            canvas.Invalidate(e.InvalidRect);
            base.OnInvalidated(e);
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

        public GraphNode GetNodeAt(Point point)
        {
            foreach (var layout in nodes)
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
            float minDistance = 0;
            GraphNode closest = null;
            foreach (var layout in nodes)
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
            nodes.Clear();
            var model = Model;
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
                        var entryPoint = new Point(location.X - PenWidth / 2, location.Y + NodeSize / 2);
                        var exitPoint = new Point(location.X + NodeSize + PenWidth / 2, location.Y + NodeSize / 2);
                        nodes.Add(new LayoutNode(node, location, entryPoint, exitPoint));
                    }
                }
            }
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            foreach (var layout in nodes)
            {
                if (layout.Node.Value != null)
                {
                    var selected = layout.Node == SelectedNode;
                    var nodeRectangle = new Rectangle(layout.Location, new Size(NodeSize, NodeSize));

                    var pen = selected ? WhitePen : BlackPen;
                    var brush = selected ? Brushes.Black : Brushes.White;
                    var textBrush = selected ? Brushes.White : Brushes.Black;

                    e.Graphics.DrawEllipse(pen, nodeRectangle);
                    e.Graphics.FillEllipse(brush, nodeRectangle);
                    e.Graphics.DrawString(layout.Node.Text.Substring(0, 1), Font, textBrush, new Point(layout.Location.X + 9, layout.Location.Y + 9));
                }
                else e.Graphics.DrawLine(Pens.Black, layout.EntryPoint, layout.ExitPoint);

                foreach (var successor in layout.Node.Successors)
                {
                    var successorLayout = nodes[successor];
                    e.Graphics.DrawLine(Pens.Black, layout.ExitPoint, successorLayout.EntryPoint);
                }
            }
        }

        private void canvas_MouseClick(object sender, MouseEventArgs e)
        {
            var node = GetNodeAt(e.Location);
            if (node != null)
            {
                SelectedNode = node;
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
            public LayoutNode(GraphNode node, Point location, Point entryPoint, Point exitPoint)
            {
                Node = node;
                Location = location;
                EntryPoint = entryPoint;
                ExitPoint = exitPoint;
            }

            public GraphNode Node { get; set; }

            public Point Location { get; set; }

            public Point Center
            {
                get { return new Point(Location.X + HalfSize, Location.Y + HalfSize); }
            }

            public Point EntryPoint { get; set; }

            public Point ExitPoint { get; set; }

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
