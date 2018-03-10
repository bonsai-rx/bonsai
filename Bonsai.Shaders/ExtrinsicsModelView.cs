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
    [Description("Converts extrinsics rotation and translation vectors into a model-view matrix.")]
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
    }
}
