using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Design
{
    static class HatchBrushes
    {
        const HatchStyle Style = HatchStyle.BackwardDiagonal;
        internal static readonly HatchBrush Violet = new HatchBrush(Style, Color.Black, Color.Violet);
        internal static readonly HatchBrush LightGreen = new HatchBrush(Style, Color.Black, Color.LightGreen);
        internal static readonly HatchBrush White = new HatchBrush(Style, Color.Black, Color.White);
        internal static readonly HatchBrush DarkGray = new HatchBrush(Style, Color.Black, Color.DarkGray);
        internal static readonly HatchBrush Goldenrod = new HatchBrush(Style, Color.Black, Color.Goldenrod);
        internal static readonly HatchBrush Orange = new HatchBrush(Style, Color.Black, Color.Orange);
        internal static readonly HatchBrush LightBlue = new HatchBrush(Style, Color.Black, Color.LightBlue);
    }
}
