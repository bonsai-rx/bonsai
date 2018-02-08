using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Applies a translation along the specified axes.")]
    public class Translate : MatrixTransform
    {
        [Description("The translation along the x-axis.")]
        public float X { get; set; }

        [Description("The translation along the y-axis.")]
        public float Y { get; set; }

        [Description("The translation along the z-axis.")]
        public float Z { get; set; }

        protected override void CreateTransform(out Matrix4 result)
        {
            Matrix4.CreateTranslation(X, Y, Z, out result);
        }
    }
}
