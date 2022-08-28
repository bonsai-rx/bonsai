using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that approximates each polygonal curve in the sequence
    /// with the specified precision.
    /// </summary>
    [Description("Approximates each polygonal curve in the sequence with the specified precision.")]
    public class ApproximatePolygon : Transform<Contours, Contours>
    {
        /// <summary>
        /// Gets or sets a value specifying the polygon approximation method.
        /// </summary>
        [Description("Specifies the polygon approximation method.")]
        public PolygonApproximation Method { get; set; }

        /// <summary>
        /// Gets or sets the desired approximation accuracy.
        /// </summary>
        [Description("The desired approximation accuracy.")]
        public double Eps { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether approximation should proceed
        /// for all hierarchical levels.
        /// </summary>
        [Description("Specifies whether approximation should proceed for all hierarchical levels.")]
        public bool Recursive { get; set; }

        /// <summary>
        /// Approximates each polygonal curve in an observable sequence with the
        /// specified precision.
        /// </summary>
        /// <param name="source">A sequence of polygonal curves to approximate.</param>
        /// <returns>A sequence of the approximated polygonal curves.</returns>
        public override IObservable<Contours> Process(IObservable<Contours> source)
        {
            return source.Select(input =>
            {
                Seq output = input.FirstContour;
                if (output != null)
                {
                    output = CV.ApproxPoly(output, Contour.HeaderSize, output.Storage, Method, Eps, Recursive);
                }

                return new Contours(output, input.ImageSize);
            });
        }
    }
}
