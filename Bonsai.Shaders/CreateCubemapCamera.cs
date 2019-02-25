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
    [Description("Generates a sequence of perspective camera viewpoints which can be used to render a dynamic cubemap texture.")]
    public class CreateCubemapCamera : Source<Camera>
    {
        public CreateCubemapCamera()
        {
            NearClip = 0.1f;
            FarClip = 1000f;
        }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The eye, or camera position, in the world coordinate frame.")]
        public Vector3 Eye { get; set; }

        [Category("Z-Clipping")]
        [Description("The distance to the near clip plane.")]
        public float NearClip { get; set; }

        [Category("Z-Clipping")]
        [Description("The distance to the far clip plane.")]
        public float FarClip { get; set; }

        IEnumerable<Camera> GenerateCubemapViews()
        {
            var eye = Eye;
            var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1, NearClip, FarClip);
            yield return new Camera(Matrix4.LookAt(eye, eye + Vector3.UnitX, -Vector3.UnitY), projection);
            yield return new Camera(Matrix4.LookAt(eye, eye - Vector3.UnitX, -Vector3.UnitY), projection);
            yield return new Camera(Matrix4.LookAt(eye, eye + Vector3.UnitY, Vector3.UnitZ), projection);
            yield return new Camera(Matrix4.LookAt(eye, eye - Vector3.UnitY, -Vector3.UnitZ), projection);
            yield return new Camera(Matrix4.LookAt(eye, eye + Vector3.UnitZ, -Vector3.UnitY), projection);
            yield return new Camera(Matrix4.LookAt(eye, eye - Vector3.UnitZ, -Vector3.UnitY), projection);
        }

        public override IObservable<Camera> Generate()
        {
            return GenerateCubemapViews().ToObservable();
        }

        public IObservable<Camera> Generate<TSource>(IObservable<TSource> source)
        {
            return source.SelectMany(input => GenerateCubemapViews());
        }
    }
}
