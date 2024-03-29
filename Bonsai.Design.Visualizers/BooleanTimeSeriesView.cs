﻿using System;
using ZedGraph;

namespace Bonsai.Design.Visualizers
{
    class BooleanTimeSeriesView : RollingGraphView
    {
        double[] previousValues;

        public BooleanTimeSeriesView()
        {
            Capacity = base.Capacity;
            StatusStrip.Items.RemoveAt(StatusStrip.Items.Count - 1);
            StatusStrip.Items.RemoveAt(StatusStrip.Items.Count - 1);
            StatusStrip.Items.RemoveAt(StatusStrip.Items.Count - 1);
            StatusStrip.Items.RemoveAt(StatusStrip.Items.Count - 1);
        }

        public override int Capacity
        {
            get { return base.Capacity / 2; }
            set { base.Capacity = value * 2; }
        }

        protected override void OnLoad(EventArgs e)
        {
            ((LineItem)Graph.GraphPane.CurveList[0]).Line.Width = 2;
            base.OnLoad(e);
        }

        public override void AddValues(double index, params double[] values)
        {
            if (previousValues != null) base.AddValues(index, previousValues);
            base.AddValues(index, values);
            previousValues = values;
        }
    }
}
