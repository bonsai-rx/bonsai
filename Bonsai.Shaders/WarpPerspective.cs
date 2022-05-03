using OpenCV.Net;
using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that creates a warp perspective transform matrix
    /// for planar projection mapping.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Creates a warp perspective transform matrix for planar projection mapping.")]
    public class WarpPerspective
    {
        /// <summary>
        /// Gets or sets the coordinates of the four quadrangle vertices specifying
        /// the perspective transform.
        /// </summary>
        [Description("The coordinates of the four quadrangle vertices specifying the perspective transform.")]
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

        /// <summary>
        /// Creates a warp perspective transform matrix for planar projection
        /// mapping whenever an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of notifications used to create the perspective transform.
        /// </param>
        /// <returns>
        /// The sequence of created <see cref="Matrix4"/> objects representing
        /// the perspective transform.
        /// </returns>
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
                    currentSource ??= InitializeQuadrangle();
                    Destination ??= InitializeQuadrangle();

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
