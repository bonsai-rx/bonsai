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
        const HatchStyle DiagonalHatch = HatchStyle.BackwardDiagonal;
        internal static readonly HatchBrush DiagonalViolet = new HatchBrush(DiagonalHatch, Color.Black, Color.Violet);
        internal static readonly HatchBrush DiagonalLightGreen = new HatchBrush(DiagonalHatch, Color.Black, Color.LightGreen);
        internal static readonly HatchBrush DiagonalWhite = new HatchBrush(DiagonalHatch, Color.Black, Color.White);
        internal static readonly HatchBrush DiagonalDarkGray = new HatchBrush(DiagonalHatch, Color.Black, Color.DarkGray);
        internal static readonly HatchBrush DiagonalGoldenrod = new HatchBrush(DiagonalHatch, Color.Black, Color.Goldenrod);
        internal static readonly HatchBrush DiagonalOrange = new HatchBrush(DiagonalHatch, Color.Black, Color.Orange);
        internal static readonly HatchBrush DiagonalLightBlue = new HatchBrush(DiagonalHatch, Color.Black, Color.LightBlue);

        const HatchStyle CrossHatch = HatchStyle.OutlinedDiamond;
        internal static readonly HatchBrush CrossViolet = new HatchBrush(CrossHatch, Color.Black, Color.Violet);
        internal static readonly HatchBrush CrossLightGreen = new HatchBrush(CrossHatch, Color.Black, Color.LightGreen);
        internal static readonly HatchBrush CrossWhite = new HatchBrush(CrossHatch, Color.Black, Color.White);
        internal static readonly HatchBrush CrossDarkGray = new HatchBrush(CrossHatch, Color.Black, Color.DarkGray);
        internal static readonly HatchBrush CrossGoldenrod = new HatchBrush(CrossHatch, Color.Black, Color.Goldenrod);
        internal static readonly HatchBrush CrossOrange = new HatchBrush(CrossHatch, Color.Black, Color.Orange);
        internal static readonly HatchBrush CrossLightBlue = new HatchBrush(CrossHatch, Color.Black, Color.LightBlue);
    }
}
