using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Applies a scale factor along the specified axes.")]
    public class Scale : MatrixTransform
    {
        public Scale()
        {
            X = Y = Z = 1;
        }

        [Range(0, 2)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The scale factor for the x-axis.")]
        public float X { get; set; }

        [Range(0, 2)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The scale factor for the y-axis.")]
        public float Y { get; set; }

        [Range(0, 2)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The scale factor for the z-axis.")]
        public float Z { get; set; }

        protected override void CreateTransform(out Matrix4 result)
        {
            Matrix4.CreateScale(X, Y, Z, out result);
        }
    }
}
