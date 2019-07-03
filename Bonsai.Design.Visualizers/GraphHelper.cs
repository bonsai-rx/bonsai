using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZedGraph;

namespace Bonsai.Design.Visualizers
{
    static class GraphHelper
    {
        internal static void SetAxisLabel(Axis axis, string label)
        {
            axis.Title.Text = label;
            axis.Title.IsVisible = !string.IsNullOrEmpty(label);
        }

        internal static void FormatDateAxis(Axis axis)
        {
            axis.Type = AxisType.DateAsOrdinal;
            axis.Scale.Format = "HH:mm:ss";
            axis.Scale.MajorUnit = DateUnit.Second;
            axis.Scale.MinorUnit = DateUnit.Millisecond;
            axis.MinorTic.IsAllTics = false;
        }
    }
}
