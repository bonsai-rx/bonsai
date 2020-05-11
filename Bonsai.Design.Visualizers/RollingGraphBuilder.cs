using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Bonsai.Design.Visualizers
{
    [DefaultProperty(nameof(ValueSelector))]
    [TypeVisualizer(typeof(RollingGraphVisualizer))]
    [Description("A visualizer that plots each element of the sequence as a rolling graph.")]
    public class RollingGraphBuilder : SingleArgumentExpressionBuilder
    {
        [Editor("Bonsai.Design.MemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The inner property that will be used as index for the graph.")]
        public string IndexSelector { get; set; }

        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The inner properties that will be used as values for the graph.")]
        public string ValueSelector { get; set; }

        [Description("The optional capacity used for rolling line graphs. If no capacity is specified, all data points will be displayed.")]
        public int? Capacity { get; set; }

        public bool ShouldSerializeElementSelector() => false;

        [Browsable(false)]
        public string ElementSelector
        {
            get { return ValueSelector; }
            set { ValueSelector = value; }
        }

        internal VisualizerController Controller { get; set; }

        internal class VisualizerController
        {
            internal int Capacity;
            internal Type IndexType;
            internal string IndexLabel;
            internal string[] ValueLabels;
            internal Action<object, RollingGraphVisualizer> AddValues;
        }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var parameterType = source.Type.GetGenericArguments()[0];
            var valueParameter = Expression.Parameter(typeof(object));
            var viewParameter = Expression.Parameter(typeof(RollingGraphVisualizer));
            var elementVariable = Expression.Variable(parameterType);
            Controller = new VisualizerController();
            Controller.Capacity = Capacity.GetValueOrDefault();

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
