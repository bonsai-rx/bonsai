using OpenCV.Net;
using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Creates a warp perspective transform matrix for planar projection mapping.")]
    public class WarpPerspective
    {
        [Description("Coordinates of the four corresponding quadrangle vertices in the output image.")]
        [Editor("Bonsai.Vision.Design.IplImageOutputQuadrangleEditor, Bonsai.Vision.Design", DesignTypes.UITypeEditor)]
        public Point2f[] Destination { get; set; }

        static Point2f[] InitializeQuadrangle()
        {
            return new[]
            {
                new Point2f(-1, -1),
                new Point2f(-1, 1),
                new Point2f(1, 1),
                new Point2f(1, -1)
            };
        }

        public IObservable<Matrix4> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                Point2f[] currentSource = null;
                Point2f[] currentDestination = null;
                var projectionMatrix = default(Matrix4);
                var perspectiveTransform = new float[3 * 3];
                return source.Select(input =>
                {
                    currentSource = currentSource ?? InitializeQuadrangle();
                    Destination = Destination ?? InitializeQuadrangle();

                    if (Destination != currentDestination)
                    {
                        currentDestination = Destination;
                        var transformHandle = GCHandle.Alloc(perspectiveTransform, GCHandleType.Pinned);
                        try
                        {
                            using (var mapMatrix = new Mat(3, 3, Depth.F32, 1, transformHandle.AddrOfPinnedObject()))
                            {
                                CV.GetPerspectiveTransform(currentSource, currentDestination, mapMatrix);
                            }
                        }
                        finally { transformHandle.Free(); }
                        projectionMatrix.M11 = perspectiveTransform[0];
                        projectionMatrix.M12 = perspectiveTransform[1];
                        projectionMatrix.M14 = perspectiveTransform[2];
                        projectionMatrix.M21 = perspectiveTransform[3];
                        projectionMatrix.M22 = perspectiveTransform[4];
                        projectionMatrix.M24 = perspectiveTransform[5];
                        projectionMatrix.M41 = perspectiveTransform[6];
                        projectionMatrix.M42 = perspectiveTransform[7];
                        projectionMatrix.M44 = perspectiveTransform[8];
                        projectionMatrix.M33 = 1;
                    }

                    return projectionMatrix;
                });
            });
        }
    }
}
