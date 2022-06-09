using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Bonsai.Design.Visualizers
{
    /// <summary>
    /// Represents an operator that configures a visualizer to plot each element
    /// of the sequence as a rolling graph.
    /// </summary>
    [DefaultProperty(nameof(ValueSelector))]
    [TypeVisualizer(typeof(RollingGraphVisualizer))]
    [Description("A visualizer that plots each element of the sequence as a rolling graph.")]
    public class RollingGraphBuilder : SingleArgumentExpressionBuilder
    {
        /// <summary>
        /// Gets or sets the name of the property that will be used as index for the graph.
        /// </summary>
        [Editor("Bonsai.Design.MemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the property that will be used as index for the graph.")]
        public string IndexSelector { get; set; }

        /// <summary>
        /// Gets or sets the names of the properties that will be displayed in the graph.
        /// </summary>
        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The names of the properties that will be displayed in the graph.")]
        public string ValueSelector { get; set; }

        /// <summary>
        /// Gets or sets the optional capacity used for rolling line graphs. If no capacity is specified,
        /// all data points will be displayed.
        /// </summary>
        [Obsolete]
        [Browsable(false)]
        public int? Capacity { get; set; }

        /// <summary>
        /// Gets a value indicating whether to serialize the element selector property.
        /// </summary>
        /// <returns>
        /// This method always returns <see langword="false"/> as this is an obsolete property.
        /// </returns>
        [Obsolete]
        public bool ShouldSerializeElementSelector() => false;

        /// <summary>
        /// Gets or sets the names of the properties that will be displayed in the graph.
        /// </summary>
        [Obsolete]
        [Browsable(false)]
        public string ElementSelector
        {
            get { return ValueSelector; }
            set { ValueSelector = value; }
        }

        internal VisualizerController Controller { get; set; }

        internal class VisualizerController
        {
            internal Type IndexType;
            internal string IndexLabel;
            internal string[] ValueLabels;
            internal Action<object, RollingGraphVisualizer> AddValues;
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
            var viewParameter = Expression.Parameter(typeof(RollingGraphVisualizer));
            var elementVariable = Expression.Variable(parameterType);
            Controller = new VisualizerController();

            var selectedIndex = GraphHelper.SelectIndexMember(elementVariable, IndexSelector, out Controller.IndexLabel);
            Controller.IndexType = selectedIndex.Type;
            if (selectedIndex.Type != typeof(double) && selectedIndex.Type != typeof(string))
            {
                selectedIndex = Expression.Convert(selectedIndex, typeof(double));
            }

            var selectedValues = GraphHelper.SelectDataValues(elementVariable, ValueSelector, out Controller.ValueLabels);
            var showBody = Expression.Block(new[] { elementVariable },
                Expression.Assign(elementVariable, Expression.Convert(valueParameter, parameterType)),
                Expression.Call(viewParameter, nameof(RollingGraphVisualizer.AddValues), null, selectedIndex, selectedValues));
            Controller.AddValues = Expression.Lambda<Action<object, RollingGraphVisualizer>>(showBody, valueParameter, viewParameter).Compile();
            return Expression.Call(typeof(RollingGraphBuilder), nameof(Process), new[] { parameterType }, source);
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source;
        }
    }
}
