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
            internal int NumSeries;
            internal Type IndexType;
            internal string IndexLabel;
            internal string[] ValueLabels;
            internal Action<object, RollingGraphView> Show;
        }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var parameterType = source.Type.GetGenericArguments()[0];
            var valueParameter = Expression.Parameter(typeof(object));
            var viewParameter = Expression.Parameter(typeof(RollingGraphView));
            var elementVariable = Expression.Variable(parameterType);
            Controller = new VisualizerController();

            var selectedIndex = GraphHelper.SelectIndexMember(elementVariable, IndexSelector, out Controller.IndexLabel);
            Controller.IndexType = selectedIndex.Type;
            if (selectedIndex.Type != typeof(double) && selectedIndex.Type != typeof(string))
            {
                selectedIndex = Expression.Convert(selectedIndex, typeof(double));
            }

            var selectedValues = GraphHelper.SelectDataValues(elementVariable, ValueSelector, out Controller.ValueLabels);
            Controller.NumSeries = Controller.ValueLabels == null ? 1 : Controller.ValueLabels.Length;
            var showBody = Expression.Block(new[] { elementVariable },
                Expression.Assign(elementVariable, Expression.Convert(valueParameter, parameterType)),
                Expression.Call(typeof(RollingGraphBuilder), nameof(ShowArrayValues), null, viewParameter, selectedIndex, selectedValues));
            Controller.Show = Expression.Lambda<Action<object, RollingGraphView>>(showBody, valueParameter, viewParameter).Compile();
            return Expression.Call(typeof(RollingGraphBuilder), nameof(Process), new[] { parameterType }, source);
        }

        static void ShowArrayValues(RollingGraphView view, string index, double[] values)
        {
            if (values.Length != view.Graph.NumSeries) view.Graph.NumSeries = values.Length;
            view.Graph.AddValues(0, index, values);
        }

        static void ShowArrayValues(RollingGraphView view, double index, double[] values)
        {
            if (values.Length != view.Graph.NumSeries) view.Graph.NumSeries = values.Length;
            view.Graph.AddValues(index, values);
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source;
        }
    }
}
