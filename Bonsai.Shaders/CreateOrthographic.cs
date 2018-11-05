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
    [Description("Creates an orthographic projection matrix.")]
    public class CreateOrthographic : Source<Matrix4>
    {
        public CreateOrthographic()
        {
            Width = 2;
            Height = 2;
            NearClip = 0.1f;
            FarClip = 1000f;
        }

        [Description("The width of the projection volume.")]
        public float Width { get; set; }

        [Description("The height of the projection volume.")]
        public float Height { get; set; }

        [Category("Z-Clipping")]
        [Description("The distance to the near clip plane.")]
        public float NearClip { get; set; }

        [Category("Z-Clipping")]
        [Description("The distance to the far clip plane.")]
        public float FarClip { get; set; }

        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer(() => Observable.Return(Matrix4.CreateOrthographic(Width, Height, NearClip, FarClip)));
        }

        public IObservable<Matrix4> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => Matrix4.CreateOrthographic(Width, Height, NearClip, FarClip));
        }
    }
}
