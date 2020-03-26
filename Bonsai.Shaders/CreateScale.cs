using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Creates a scale matrix.")]
    public class CreateScale : Source<Matrix4>
    {
        public CreateScale()
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

        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer(() => Observable.Return(Matrix4.CreateScale(X, Y, Z)));
        }

        public IObservable<Matrix4> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => Matrix4.CreateScale(X, Y, Z));
        }
    }
}
