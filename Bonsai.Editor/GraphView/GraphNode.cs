using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using Bonsai.Expressions;

namespace Bonsai.Design
{
    class GraphNode
    {
        static readonly Range<int> EmptyRange = Range.Create(0, 0);
        static readonly Brush DisabledBrush = new HatchBrush(HatchStyle.BackwardDiagonal, Color.Black, Color.Transparent);
        static readonly Brush ObsoleteBrush = new HatchBrush(HatchStyle.OutlinedDiamond, Color.Black, Color.Transparent);
        static readonly Pen DashPen = new Pen(Brushes.DarkGray) { DashPattern = new[] { 4f, 2f } };
        static readonly Pen SolidPen = Pens.DarkGray;

        public GraphNode(ExpressionBuilder value, int layer, IEnumerable<GraphEdge> successors)
        {
            Value = value;
            Layer = layer;
            Successors = successors;

            Pen = SolidPen;
            Brush = Brushes.White;
            if (value != null)
            {
                var expressionBuilder = ExpressionBuilder.Unwrap(value);
                var elementAttributes = TypeDescriptor.GetAttributes(expressionBuilder);
                var elementCategoryAttribute = (WorkflowElementCategoryAttribute)elementAttributes[typeof(WorkflowElementCategoryAttribute)];
                var obsolete = (ObsoleteAttribute)elementAttributes[typeof(ObsoleteAttribute)] != null;
                if (expressionBuilder is DisableBuilder) Flags |= NodeFlags.Disabled;

                var workflowElement = ExpressionBuilder.GetWorkflowElement(expressionBuilder);
                if (workflowElement != expressionBuilder)
                {
                    var builderCategoryAttribute = elementCategoryAttribute;
                    elementAttributes = TypeDescriptor.GetAttributes(workflowElement);
                    elementCategoryAttribute = (WorkflowElementCategoryAttribute)elementAttributes[typeof(WorkflowElementCategoryAttribute)];
                    obsolete |= (ObsoleteAttribute)elementAttributes[typeof(ObsoleteAttribute)] != null;
                    if (elementCategoryAttribute == WorkflowElementCategoryAttribute.Default)
                    {
                        elementCategoryAttribute = builderCategoryAttribute;
                    }
                }

                if (obsolete) Flags |= NodeFlags.Obsolete;
                Category = elementCategoryAttribute.Category;
                Pen = expressionBuilder.IsBuildDependency() ? DashPen : SolidPen;
                Icon = new ElementIcon(expressionBuilder);

                switch (elementCategoryAttribute.Category)
                {
                    case ElementCategory.Source: Brush = Brushes.Violet; break;
                    case ElementCategory.Condition: Brush = Brushes.LightGreen; break;
                    case ElementCategory.Transform: Brush = Brushes.White; break;
                    case ElementCategory.Sink: Brush = Brushes.DarkGray; break;
                    case ElementCategory.Nested:
                    case ElementCategory.Workflow: Brush = Brushes.Goldenrod; break;
                    case ElementCategory.Property: Brush = Brushes.Orange; break;
                    case ElementCategory.Combinator:
                    default: Brush = Brushes.LightBlue; break;
                }
            }

            InitializeDummySuccessors();
        }

        void InitializeDummySuccessors()
        {
            foreach (var successor in Successors)
            {
                if (successor.Node.Value == null)
                {
                    successor.Node.Pen = Pen;
                    successor.Node.InitializeDummySuccessors();
                }
            }
        }

        private NodeFlags Flags { get; set; }

        public int Layer { get; internal set; }

        public int LayerIndex { get; internal set; }

        public int ArgumentCount { get; internal set; }

        public Range<int> ArgumentRange
        {
            get { return (Flags & NodeFlags.Disabled) != 0 || Value == null ? EmptyRange : Value.ArgumentRange; }
        }

        public ExpressionBuilder Value { get; private set; }

        public IEnumerable<GraphEdge> Successors { get; private set; }

        public object Tag { get; set; }

        public Brush Brush { get; private set; }

        public Brush ModifierBrush
        {
            get
            {
                if ((Flags & NodeFlags.Disabled) != 0) return DisabledBrush;
                else if ((Flags & NodeFlags.Obsolete) != 0) return ObsoleteBrush;
                else return null;
            }
        }

        public ElementCategory Category { get; private set; }

        public ElementIcon Icon { get; private set; }

        public Pen Pen { get; private set; }

        public string Text
        {
            get { return Value != null ? ExpressionBuilder.GetElementDisplayName(Value) : string.Empty; }
        }

        public bool Highlight
        {
            get { return (Flags & NodeFlags.Highlight) != 0; }
            set
            {
                if (value) Flags |= NodeFlags.Highlight;
                else Flags &= ~NodeFlags.Highlight;
            }
        }


        /// <summary>
        /// Returns a string that represents the value of this <see cref="GraphNode"/> instance.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="GraphNode"/> object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{{{0}}}", Text);
        }

        [Flags]
        enum NodeFlags
        {
            None = 0x0,
            Highlight = 0x1,
            Obsolete = 0x2,
            Disabled = 0x4
        }
    }
}
