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
    [DefaultProperty(nameof(CellSpans))]
    [TypeVisualizer(typeof(TableLayoutPanelVisualizer))]
    [Description("Specifies a mashup visualizer panel that can be used to arrange other visualizers in a grid.")]
    public class TableLayoutPanelBuilder : VariableArgumentExpressionBuilder
    {
        public TableLayoutPanelBuilder()
            : base(minArguments: 0, maxArguments: 1)
        {
            ColumnCount = 1;
            RowCount = 1;
        }
        
        [Description("The number of columns in the visualizer grid layout.")]
        public int ColumnCount { get; set; }

        [Description("The number of rows in the visualizer grid layout.")]
        public int RowCount { get; set; }

        [Category("Table Style")]
        [Description("Specifies the optional size ratio of the columns in the visualizer grid layout.")]
        public Collection<ColumnStyle> ColumnStyles { get; } = new Collection<ColumnStyle>();

        [Category("Table Style")]
        [Description("Specifies the optional size ratio of the rows in the visualizer grid layout.")]
        public Collection<RowStyle> RowStyles { get; } = new Collection<RowStyle>();

        [Category("Table Style")]
        [XmlArrayItem("CellSpan")]
        [Description("Specifies the optional column and row span of each cell in the visualizer grid layout.")]
        public Collection<TableLayoutPanelCellSpan> CellSpans { get; } = new Collection<TableLayoutPanelCellSpan>();

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.SingleOrDefault();
            if (source == null)
            {
                return Expression.Constant(Observable.Return(Unit.Default), typeof(IObservable<Unit>));
            }
            else return Expression.Call(typeof(TableLayoutPanelBuilder), nameof(Process), source.Type.GetGenericArguments(), source);
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source;
        }
    }
}
