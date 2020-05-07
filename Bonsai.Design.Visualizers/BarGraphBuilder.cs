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
    [TypeVisualizer(typeof(BarGraphVisualizer))]
    [Description("A visualizer that plots each element of the sequence as a bar graph.")]
    public class BarGraphBuilder : SingleArgumentExpressionBuilder
    {
        [Editor("Bonsai.Design.MemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The inner property that will be used as index for the graph.")]
        public string IndexSelector { get; set; }

        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The inner properties that will be displayed in the graph.")]
        public string ValueSelector { get; set; }

        [TypeConverter(typeof(BaseAxisConverter))]
        [Description("Specifies the axis from which the bars in the graph will be displayed.")]
        public BarBase BaseAxis { get; set; }

        [Description("Specifies how the different bars in the graph will be visually arranged.")]
        public BarType BarType { get; set; }

        [Description("The optional capacity used for rolling bar graphs. If no capacity is specified, all data points will be displayed.")]
        public int? Capacity { get; set; }

        internal VisualizerController Controller { get; set; }

        internal class VisualizerController
        {
            internal int Capacity;
            internal Type IndexType;
            internal string IndexLabel;
            internal string[] ValueLabels;
            internal Action<object, BarGraphVisualizer> AddValues;
            internal BarBase BaseAxis;
        }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var parameterType = source.Type.GetGenericArguments()[0];
            var valueParameter = Expression.Parameter(typeof(object));
            var viewParameter = Expression.Parameter(typeof(BarGraphVisualizer));
            var elementVariable = Expression.Variable(parameterType);
            Controller = new VisualizerController();
            Controller.Capacity = Capacity.GetValueOrDefault();
            Controller.BaseAxis = BaseAxis;

            var selectedIndex = GraphHelper.SelectIndexMember(elementVariable, IndexSelector, out Controller.IndexLabel);
            Controller.IndexType = selectedIndex.Type;
            if (selectedIndex.Type != typeof(string))
            {
                selectedIndex = Expression.Call(selectedIndex, nameof(ToString), null);
            }

            var baseAxis = BaseAxis;
            var ordinalValue = Expression.Default(typeof(double));
            var baseX = Expression.Constant(baseAxis <= BarBase.X2);
            var pointConstructor = typeof(PointPair).GetConstructor(new[] { typeof(double), typeof(double), typeof(string) });
            var selectedValues = GraphHelper.SelectDataValues(elementVariable, ValueSelector, out Controller.ValueLabels);
            var addValuesBody = Expression.Block(new[] { elementVariable },
                Expression.Assign(elementVariable, Expression.Convert(valueParameter, parameterType)),
                Expression.Call(viewParameter, nameof(BarGraphVisualizer.AddValues), null, selectedIndex, selectedValues));
            Controller.AddValues = Expression.Lambda<Action<object, BarGraphVisualizer>>(addValuesBody, valueParameter, viewParameter).Compile();
            return Expression.Call(typeof(BarGraphBuilder), nameof(Process), new[] { parameterType }, source);
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source;
        }

        class BaseAxisConverter : EnumConverter
        {
            public BaseAxisConverter(Type type)
                : base(type)
            {
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new[] { BarBase.X, BarBase.Y });
            }
        }
    }
}
