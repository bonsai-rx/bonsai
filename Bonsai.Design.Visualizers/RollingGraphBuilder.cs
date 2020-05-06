using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Bonsai.Design.Visualizers
{
    [DefaultProperty("ElementSelector")]
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

            Expression selectedIndex;
            var indexSelector = IndexSelector;
            if (!string.IsNullOrEmpty(indexSelector))
            {
                selectedIndex = ExpressionHelper.SelectMembers(elementVariable, indexSelector).First();
                Controller.IndexLabel = indexSelector;
            }
            else
            {
                selectedIndex = Expression.Property(null, typeof(DateTime), nameof(DateTime.Now));
                Controller.IndexLabel = "Time";
            }

            if (selectedIndex.Type == typeof(DateTimeOffset)) selectedIndex = Expression.Property(selectedIndex, nameof(DateTimeOffset.DateTime));
            if (selectedIndex.Type == typeof(DateTime))
            {
                selectedIndex = Expression.Convert(selectedIndex, typeof(ZedGraph.XDate));
            }
            Controller.IndexType = selectedIndex.Type;
            if (selectedIndex.Type != typeof(double) && selectedIndex.Type != typeof(string))
            {
                selectedIndex = Expression.Convert(selectedIndex, typeof(double));
            }

            Expression showBody;
            var memberNames = ExpressionHelper.SelectMemberNames(ValueSelector).ToArray();
            if (memberNames.Length == 0) memberNames = new[] { ExpressionHelper.ImplicitParameterName };
            var selectedMembers = memberNames.Select(name => ExpressionHelper.MemberAccess(elementVariable, name))
                .SelectMany(GraphHelper.UnwrapMemberAccess)
                .Select(x => x.Type.IsArray ? x : Expression.Convert(x, typeof(object))).ToArray();
            if (selectedMembers.Length == 1 && selectedMembers[0].Type.IsArray)
            {
                var selectedValues = Expression.Convert(selectedMembers[0], typeof(Array));
                showBody = Expression.Block(new[] { elementVariable },
                    Expression.Assign(elementVariable, Expression.Convert(valueParameter, parameterType)),
                    Expression.Call(typeof(RollingGraphBuilder), nameof(ShowArrayValues), null, viewParameter, selectedIndex, selectedValues));
            }
            else
            {
                Controller.ValueLabels = memberNames;
                var selectedValues = Expression.NewArrayInit(typeof(object), selectedMembers);
                showBody = Expression.Block(new[] { elementVariable },
                    Expression.Assign(elementVariable, Expression.Convert(valueParameter, parameterType)),
                    Expression.Call(viewParameter, nameof(RollingGraphView.AddValues), null, selectedIndex, selectedValues));
            }

            Controller.NumSeries = selectedMembers.Length;
            Controller.Show = Expression.Lambda<Action<object, RollingGraphView>>(showBody, valueParameter, viewParameter).Compile();
            return Expression.Call(typeof(RollingGraphBuilder), nameof(Process), new[] { parameterType }, source);
        }

        static void ShowArrayValues(RollingGraphView view, string index, Array values)
        {
            if (values.Length != view.Graph.NumSeries) view.Graph.NumSeries = values.Length;
            view.Graph.AddValues(0, index, values);
        }

        static void ShowArrayValues(RollingGraphView view, double index, Array values)
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
