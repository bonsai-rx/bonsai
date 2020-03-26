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
    [Description("Creates a perspective projection matrix from field of view information.")]
    public class CreatePerspectiveFieldOfView : Source<Matrix4>
    {
        public CreatePerspectiveFieldOfView()
        {
            NearClip = 0.1f;
            FarClip = 1000f;
            AspectRatio = 1;
            FovY = MathHelper.PiOver3;
        }

        [Range(Math.PI / 180, Math.PI - Math.PI / 180)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The angle of the field of view in the y direction, in radians.")]
        public float FovY { get; set; }

        [Description("The aspect ratio of the viewport.")]
        public float AspectRatio { get; set; }

        [Category("Z-Clipping")]
        [Description("The distance to the near clip plane.")]
        public float NearClip { get; set; }

        [Category("Z-Clipping")]
        [Description("The distance to the far clip plane.")]
        public float FarClip { get; set; }

        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer(() => Observable.Return(Matrix4.CreatePerspectiveFieldOfView(FovY, AspectRatio, NearClip, FarClip)));
        }

        public IObservable<Matrix4> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => Matrix4.CreatePerspectiveFieldOfView(FovY, AspectRatio, NearClip, FarClip));
        }
    }
}
