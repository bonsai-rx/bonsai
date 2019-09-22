using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Design.Visualizers
{
    [DefaultProperty("ElementSelector")]
    [TypeVisualizer(typeof(RollingGraphVisualizer))]
    [Description("A visualizer that plots each element of the sequence as a rolling graph.")]
    public class RollingGraphBuilder : SingleArgumentExpressionBuilder
    {
        [Editor(typeof(MemberSelectorEditor), typeof(UITypeEditor))]
        [Description("The inner property that will be used as index for the graph.")]
        public string IndexSelector { get; set; }

        [Editor(typeof(MultiMemberSelectorEditor), typeof(UITypeEditor))]
        [Description("The inner properties that will be displayed in the graph.")]
        public string ElementSelector { get; set; }

        internal VisualizerController Controller { get; set; }

        internal class VisualizerController
        {
            internal int NumSeries;
            internal Type IndexType;
            internal string IndexLabel;
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

            Expression selectedX;
            var selectorX = IndexSelector;
            if (!string.IsNullOrEmpty(selectorX))
            {
                selectedX = ExpressionHelper.SelectMembers(elementVariable, selectorX).First();
                Controller.IndexLabel = selectorX;
            }
            else
            {
                selectedX = Expression.Property(null, typeof(DateTime), "Now");
                Controller.IndexLabel = "Time";
            }

            if (selectedX.Type == typeof(DateTimeOffset)) selectedX = Expression.Property(selectedX, "DateTime");
            if (selectedX.Type == typeof(DateTime))
            {
                selectedX = Expression.Convert(selectedX, typeof(ZedGraph.XDate));
            }
            Controller.IndexType = selectedX.Type;
            selectedX = Expression.Convert(selectedX, typeof(double));

            Expression showBody;
            var selectedMembers = ExpressionHelper.SelectMembers(elementVariable, ElementSelector)
                .SelectMany(GraphHelper.UnwrapMemberAccess)
                .Select(x => x.Type.IsArray ? x : Expression.Convert(x, typeof(object))).ToArray();
            if (selectedMembers.Length == 1 && selectedMembers[0].Type.IsArray)
            {
                var selectedValues = Expression.Convert(selectedMembers[0], typeof(Array));
                showBody = Expression.Block(new[] { elementVariable },
                    Expression.Assign(elementVariable, Expression.Convert(valueParameter, parameterType)),
                    Expression.Call(typeof(RollingGraphBuilder), "ShowArrayValues", null, viewParameter, selectedX, selectedValues));
            }
            else
            {
                var selectedValues = Expression.NewArrayInit(typeof(object), selectedMembers);
                showBody = Expression.Block(new[] { elementVariable },
                    Expression.Assign(elementVariable, Expression.Convert(valueParameter, parameterType)),
                    Expression.Call(viewParameter, "AddValues", null, selectedX, selectedValues));
            }

            Controller.NumSeries = selectedMembers.Length;
            Controller.Show = Expression.Lambda<Action<object, RollingGraphView>>(showBody, valueParameter, viewParameter).Compile();
            return Expression.Call(typeof(RollingGraphBuilder), "Process", new[] { parameterType }, source);
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
