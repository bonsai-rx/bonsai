using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [Description("Masks a non-rectangular region of interest bounded by a set of polygonal contours.")]
    public class MaskPolygon : CropPolygon
    {
        public MaskPolygon()
            : base(false)
        {
        }
    }
}
