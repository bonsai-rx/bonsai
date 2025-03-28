using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Bonsai.Design.Visualizers
{
    /// <summary>
    /// Represents an operator that specifies a mashup visualizer panel that can be used
    /// to arrange other visualizers in a grid.
    /// </summary>
    [DefaultProperty(nameof(CellSpans))]
    [TypeVisualizer(typeof(TableLayoutPanelVisualizer))]
    [Obsolete(ObsoleteMessages.TypeTransferredToGuiPackage)]
    [Description("Specifies a mashup visualizer panel that can be used to arrange other visualizers in a grid.")]
    public class TableLayoutPanelBuilder : VariableArgumentExpressionBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableLayoutPanelBuilder"/> class.
        /// </summary>
        public TableLayoutPanelBuilder()
            : base(minArguments: 0, maxArguments: 1)
        {
            ColumnCount = 1;
            RowCount = 1;
        }

        /// <summary>
        /// Gets or sets the name of the visualizer window.
        /// </summary>
        [Category(nameof(CategoryAttribute.Design))]
        [Description("The name of the visualizer window.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the number of columns in the visualizer grid layout.
        /// </summary>
        [Description("The number of columns in the visualizer grid layout.")]
        public int ColumnCount { get; set; }

        /// <summary>
        /// Gets or sets the number of rows in the visualizer grid layout.
        /// </summary>
        [Description("The number of rows in the visualizer grid layout.")]
        public int RowCount { get; set; }

        /// <summary>
        /// Gets a collection of <see cref="ColumnStyle"/> objects specifying the size
        /// ratio of the columns in the visualizer grid layout.
        /// </summary>
        [Category("Table Style")]
        [Description("Specifies the optional size ratio of the columns in the visualizer grid layout.")]
        public Collection<ColumnStyle> ColumnStyles { get; } = new Collection<ColumnStyle>();

        /// <summary>
        /// Gets a collection of <see cref="RowStyle"/> objects specifying the size ratio
        /// of the rows in the visualizer grid layout.
        /// </summary>
        [Category("Table Style")]
        [Description("Specifies the optional size ratio of the rows in the visualizer grid layout.")]
        public Collection<RowStyle> RowStyles { get; } = new Collection<RowStyle>();

        /// <summary>
        /// Gets a collection of <see cref="TableLayoutPanelCellSpan"/> objects specifying the
        /// column and row span of each cell in the visualizer grid layout.
        /// </summary>
        [Category("Table Style")]
        [XmlArrayItem("CellSpan")]
        [Description("Specifies the optional column and row span of each cell in the visualizer grid layout.")]
        public Collection<TableLayoutPanelCellSpan> CellSpans { get; } = new Collection<TableLayoutPanelCellSpan>();

        /// <summary>
        /// Builds the expression tree for configuring and calling the
        /// table layout panel visualizer.
        /// </summary>
        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.SingleOrDefault();
            if (source == null)
            {
                return Expression.Call(typeof(Observable), nameof(Observable.Never), new[] { typeof(Unit) });
            }
            else return Expression.Call(typeof(TableLayoutPanelBuilder), nameof(Process), source.Type.GetGenericArguments(), source);
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source;
        }
    }
}
