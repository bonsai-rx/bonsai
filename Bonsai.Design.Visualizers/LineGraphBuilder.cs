using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using ZedGraph;

namespace Bonsai.Design.Visualizers
{
    [DefaultProperty(nameof(ValueSelector))]
    [TypeVisualizer(typeof(LineGraphVisualizer))]
    [Description("A visualizer that plots each element of the sequence as a line graph.")]
    public class LineGraphBuilder : SingleArgumentExpressionBuilder
    {
        public LineGraphBuilder()
        {
            SymbolType = SymbolType.None;
            LineWidth = 1;
        }

        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The values to be displayed in the graph. Each selected value must be a point pair compatible type.")]
        public string ValueSelector { get; set; }

        [Description("The optional symbol type to use for the line graph.")]
        public SymbolType SymbolType { get; set; }

        [Description("The width (in points) to be used for the line graph. Use a value of zero to hide the line.")]
        public float LineWidth { get; set; }

        [Description("The optional capacity used for rolling line graphs. If no capacity is specified, all data points will be displayed.")]
        public int? Capacity { get; set; }

        internal VisualizerController Controller { get; set; }

        internal class VisualizerController
        {
            internal int Capacity;
            internal float LineWidth;
            internal string[] ValueLabels;
            internal SymbolType SymbolType;
            internal Action<object, LineGraphVisualizer> AddValues;
        }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var parameterType = source.Type.GetGenericArguments()[0];
            var valueParameter = Expression.Parameter(typeof(object));
            var viewParameter = Expression.Parameter(typeof(LineGraphVisualizer));
            var elementVariable = Expression.Variable(parameterType);
            Controller = new VisualizerController();
            Controller.Capacity = Capacity.GetValueOrDefault();
            Controller.SymbolType = SymbolType;
            Controller.LineWidth = LineWidth;

            var selectedValues = GraphHelper.SelectDataPoints(elementVariable, ValueSelector, out Controller.ValueLabels);
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
