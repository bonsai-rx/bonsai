using OpenCV.Net;
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
    [Description("Converts extrinsics rotation and translation vectors into a transform matrix, and vice-versa.")]
    public class ExtrinsicsTransform : Transform<Tuple<Point3d, Point3d>, Matrix4>
    {
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("Specifies the optional scale factor for rotation and translation vectors.")]
        public Vector3? Scale { get; set; }

        [Browsable(false)]
        public bool ScaleSpecified
        {
            get { return Scale.HasValue; }
        }

        public override IObservable<Matrix4> Process(IObservable<Tuple<Point3d, Point3d>> source)
        {
            return source.Select(input =>
            {
                Matrix4 result;
                var rotation = input.Item1;
                var translation = input.Item2;
                var scale = Scale.GetValueOrDefault(Vector3.One);
                var axis = new Vector3((float)rotation.X * scale.X, (float)rotation.Y * scale.Y, (float)rotation.Z * scale.Z);
                var angle = axis.Length;
                if (axis.Length > 0)
                {
                    Vector3.Divide(ref axis, angle, out axis);
                    Matrix4.CreateFromAxisAngle(axis, angle, out result);
                }
                else result = Matrix4.Identity;
                result.Row3.X = (float)translation.X * scale.X;
                result.Row3.Y = (float)translation.Y * scale.Y;
                result.Row3.Z = (float)translation.Z * scale.Z;
                return result;
            });
        }

        public IObservable<Tuple<Point3d, Point3d>> Process(IObservable<Matrix4> source)
        {
            return source.Select(input =>
            {
                float angle;
                Vector3 axis;
                var scale = Scale.GetValueOrDefault(Vector3.One);
                var translation = new Point3d(input.M41 * scale.X, input.M42 * scale.Y, input.M43 * scale.Z);
                input.ExtractRotation().ToAxisAngle(out axis, out angle);
                Vector3.Multiply(ref axis, angle, out axis);
                var rotation = new Point3d(axis.X * scale.X, axis.Y * scale.Y, axis.Z * scale.Z);
                return Tuple.Create(rotation, translation);
            });
        }
    }
}
