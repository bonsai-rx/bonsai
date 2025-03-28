using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using ZedGraph;

namespace Bonsai.Design.Visualizers
{
    /// <summary>
    /// Represents an operator that configures a visualizer to plot each element
    /// of the sequence as a line graph.
    /// </summary>
    [DefaultProperty(nameof(ValueSelector))]
    [TypeVisualizer(typeof(LineGraphVisualizer))]
    [Obsolete(ObsoleteMessages.TypeTransferredToGuiPackage)]
    [Description("A visualizer that plots each element of the sequence as a line graph.")]
    public class LineGraphBuilder : SingleArgumentExpressionBuilder
    {
        /// <summary>
        /// Gets or sets the names of the properties to be displayed in the graph.
        /// Each selected property must have a point pair compatible type.
        /// </summary>
        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The names of the properties to be displayed in the graph. Each selected property must have a point pair compatible type.")]
        public string ValueSelector { get; set; }

        /// <summary>
        /// Gets or sets the optional symbol type to use for the line graph.
        /// </summary>
        [Category(nameof(CategoryAttribute.Appearance))]
        [Description("The optional symbol type to use for the line graph.")]
        public SymbolType SymbolType { get; set; } = SymbolType.None;

        /// <summary>
        /// Gets or sets the width, in points, to be used for the line graph. Use a value of zero to hide the line.
        /// </summary>
        [Category(nameof(CategoryAttribute.Appearance))]
        [Description("The width, in points, to be used for the line graph. Use a value of zero to hide the line.")]
        public float LineWidth { get; set; } = 1;

        /// <summary>
        /// Gets or sets the optional capacity used for rolling line graphs. If no capacity is specified, all data points will be displayed.
        /// </summary>
        [Category("Range")]
        [Description("The optional capacity used for rolling line graphs. If no capacity is specified, all data points will be displayed.")]
        public int? Capacity { get; set; }

        /// <summary>
        /// Gets or sets a value specifying a fixed lower limit for the x-axis range.
        /// If no fixed range is specified, the graph limits can be edited online.
        /// </summary>
        [Category("Range")]
        [Description("Specifies the optional fixed lower limit of the x-axis range.")]
        public double? XMin { get; set; }

        /// <summary>
        /// Gets or sets a value specifying a fixed upper limit for the x-axis range.
        /// If no fixed range is specified, the graph limits can be edited online.
        /// </summary>
        [Category("Range")]
        [Description("Specifies the optional fixed upper limit of the x-axis range.")]
        public double? XMax { get; set; }

        /// <summary>
        /// Gets or sets a value specifying a fixed lower limit for the y-axis range.
        /// If no fixed range is specified, the graph limits can be edited online.
        /// </summary>
        [Category("Range")]
        [Description("Specifies the optional fixed lower limit of the y-axis range.")]
        public double? YMin { get; set; }

        /// <summary>
        /// Gets or sets a value specifying a fixed upper limit for the y-axis range.
        /// If no fixed range is specified, the graph limits can be edited online.
        /// </summary>
        [Category("Range")]
        [Description("Specifies the optional fixed upper limit of the y-axis range.")]
        public double? YMax { get; set; }

        internal VisualizerController Controller { get; set; }

        internal class VisualizerController
        {
            internal int? Capacity;
            internal double? XMin;
            internal double? XMax;
            internal double? YMin;
            internal double? YMax;
            internal bool LabelAxes;
            internal string[] ValueLabels;
            internal SymbolType SymbolType;
            internal float LineWidth;
            internal Action<object, LineGraphVisualizer> AddValues;
        }

        /// <summary>
        /// Builds the expression tree for configuring and calling the
        /// line graph visualizer on the specified input argument.
        /// </summary>
        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var parameterType = source.Type.GetGenericArguments()[0];
            var valueParameter = Expression.Parameter(typeof(object));
            var viewParameter = Expression.Parameter(typeof(LineGraphVisualizer));
            var elementVariable = Expression.Variable(parameterType);
            Controller = new VisualizerController
            {
                Capacity = Capacity,
                XMin = XMin,
                XMax = XMax,
                YMin = YMin,
                YMax = YMax,
                SymbolType = SymbolType,
                LineWidth = LineWidth
            };

            var selectedValues = GraphHelper.SelectDataPoints(
                elementVariable,
                ValueSelector,
                out Controller.ValueLabels,
                out Controller.LabelAxes);
            var addValuesBody = Expression.Block(new[] { elementVariable },
                Expression.Assign(elementVariable, Expression.Convert(valueParameter, parameterType)),
                Expression.Call(viewParameter, nameof(LineGraphVisualizer.AddValues), null, selectedValues));
            Controller.AddValues = Expression.Lambda<Action<object, LineGraphVisualizer>>(addValuesBody, valueParameter, viewParameter).Compile();
            return Expression.Call(typeof(LineGraphBuilder), nameof(Process), new[] { parameterType }, source);
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source;
        }
    }
}
