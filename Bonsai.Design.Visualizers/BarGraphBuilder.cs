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
    /// of the sequence as a bar graph.
    /// </summary>
    [DefaultProperty(nameof(ValueSelector))]
    [TypeVisualizer(typeof(BarGraphVisualizer))]
    [Obsolete(ObsoleteMessages.TypeTransferredToGuiPackage)]
    [Description("A visualizer that plots each element of the sequence as a bar graph.")]
    public class BarGraphBuilder : SingleArgumentExpressionBuilder
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
        /// Gets or sets a value specifying the axis on which the bars in the graph will be displayed.
        /// </summary>
        [TypeConverter(typeof(BaseAxisConverter))]
        [Category(nameof(CategoryAttribute.Appearance))]
        [Description("Specifies the axis on which the bars in the graph will be displayed.")]
        public BarBase BaseAxis { get; set; }

        /// <summary>
        /// Gets or sets a value specifying how the different bars in the graph will be visually arranged.
        /// </summary>
        [Category(nameof(CategoryAttribute.Appearance))]
        [Description("Specifies how the different bars in the graph will be visually arranged.")]
        public BarType BarType { get; set; }

        /// <summary>
        /// Gets or sets the optional capacity used for rolling bar graphs. If no capacity is specified,
        /// all data points will be displayed.
        /// </summary>
        [Category("Range")]
        [Description("The optional capacity used for rolling bar graphs. If no capacity is specified, all data points will be displayed.")]
        public int? Capacity { get; set; }

        /// <summary>
        /// Gets or sets a value specifying a fixed lower limit for the y-axis range.
        /// If no fixed range is specified, the graph limits can be edited online.
        /// </summary>
        [Category("Range")]
        [Description("Specifies the optional fixed lower limit of the y-axis range.")]
        public double? Min { get; set; }

        /// <summary>
        /// Gets or sets a value specifying a fixed upper limit for the y-axis range.
        /// If no fixed range is specified, the graph limits can be edited online.
        /// </summary>
        [Category("Range")]
        [Description("Specifies the optional fixed upper limit of the y-axis range.")]
        public double? Max { get; set; }

        internal VisualizerController Controller { get; set; }

        internal class VisualizerController
        {
            internal int? Capacity;
            internal double? Min;
            internal double? Max;
            internal Type IndexType;
            internal string IndexLabel;
            internal string[] ValueLabels;
            internal Action<object, BarGraphVisualizer> AddValues;
            internal BarBase BaseAxis;
            internal BarType BarType;
        }

        /// <summary>
        /// Builds the expression tree for configuring and calling the
        /// bar graph visualizer on the specified input argument.
        /// </summary>
        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var parameterType = source.Type.GetGenericArguments()[0];
            var valueParameter = Expression.Parameter(typeof(object));
            var viewParameter = Expression.Parameter(typeof(BarGraphVisualizer));
            var elementVariable = Expression.Variable(parameterType);
            Controller = new VisualizerController
            {
                Capacity = Capacity,
                Min = Min,
                Max = Max,
                BaseAxis = BaseAxis,
                BarType = BarType
            };

            var selectedIndex = GraphHelper.SelectIndexMember(elementVariable, IndexSelector, out Controller.IndexLabel);
            Controller.IndexType = selectedIndex.Type;
            if (selectedIndex.Type != typeof(string))
            {
                selectedIndex = Expression.Call(selectedIndex, nameof(ToString), null);
            }

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
