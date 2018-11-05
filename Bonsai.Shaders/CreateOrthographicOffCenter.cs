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
    [Description("Creates an orthographic projection matrix from specified projection volume boundaries.")]
    public class CreateOrthographicOffCenter : Source<Matrix4>
    {
        public CreateOrthographicOffCenter()
        {
            Left = -1;
            Right = 1;
            Bottom = -1;
            Top = 1;
            NearClip = 0.1f;
            FarClip = 1000f;
        }

        [Description("The left edge of the projection volume.")]
        public float Left { get; set; }

        [Description("The right edge of the projection volume.")]
        public float Right { get; set; }

        [Description("The bottom edge of the projection volume.")]
        public float Bottom { get; set; }

        [Description("The top edge of the projection volume.")]
        public float Top { get; set; }

        [Category("Z-Clipping")]
        [Description("The distance to the near clip plane.")]
        public float NearClip { get; set; }

        [Category("Z-Clipping")]
        [Description("The distance to the far clip plane.")]
        public float FarClip { get; set; }

        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer(() => Observable.Return(Matrix4.CreateOrthographicOffCenter(Left, Right, Bottom, Top, NearClip, FarClip)));
        }

        public IObservable<Matrix4> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => Matrix4.CreateOrthographicOffCenter(Left, Right, Bottom, Top, NearClip, FarClip));
        }
    }
}
