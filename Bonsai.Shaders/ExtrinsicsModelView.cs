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
    [Description("Converts extrinsics rotation and translation vectors into a model-view matrix, and vice-versa.")]
    public class ExtrinsicsModelView : Transform<Tuple<Point3d, Point3d>, Matrix4>
    {
        public override IObservable<Matrix4> Process(IObservable<Tuple<Point3d, Point3d>> source)
        {
            return source.Select(input =>
            {
                var rotation = input.Item1;
                var translation = input.Item2;
                var rotationM = new double[9];
                using (var rotationHeader = Mat.CreateMatHeader(rotationM, 3, 3, Depth.F64, 1))
                using (var rotationVector = Mat.FromArray(new[] { rotation.X, -rotation.Y, -rotation.Z }))
                {
                    CV.Rodrigues2(rotationVector, rotationHeader);
                }

                return new Matrix4(
                    (float)rotationM[1], (float)rotationM[4], (float)rotationM[7], 0.0f,
                    (float)rotationM[2], (float)rotationM[5], (float)rotationM[8], 0.0f,
                    (float)rotationM[0], (float)rotationM[3], (float)rotationM[6], 0.0f,
                    (float)translation.X, (float)-translation.Y, (float)-translation.Z, 1.0f);
            });
        }

        public IObservable<Tuple<Point3d, Point3d>> Process(IObservable<Matrix4> source)
        {
            return source.Select(input =>
            {
                var translation = new Point3d(input.M41, -input.M42, -input.M43);
                var rotationV = new double[3];
                var rotationM = new double[]
                {
                    input.M31, input.M11, input.M21,
                    input.M32, input.M12, input.M22,
                    input.M33, input.M13, input.M23
                };

                using (var rotationHeader = Mat.CreateMatHeader(rotationM, 3, 3, Depth.F64, 1))
                using (var rotationVector = Mat.CreateMatHeader(rotationV))
                {
                    CV.Rodrigues2(rotationHeader, rotationVector);
                }
                
                var rotation = new Point3d(rotationV[0], -rotationV[1], -rotationV[2]);
                return Tuple.Create(rotation, translation);
            });
        }
    }
}
