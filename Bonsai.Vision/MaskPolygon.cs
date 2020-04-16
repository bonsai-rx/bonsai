using System.ComponentModel;

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
