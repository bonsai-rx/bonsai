using System;
using OpenCV.Net;
using System.Xml.Serialization;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents a collection of measurements extracted from a cluster of
    /// connected pixels in an image.
    /// </summary>
    public class ConnectedComponent
    {
        /// <summary>
        /// Gets or sets the center of mass of the connected component.
        /// </summary>
        public Point2f Centroid { get; set; }

        /// <summary>
        /// Gets or sets the angle, in radians, between the x-axis and the
        /// major axis of the ellipse fit to the connected component.
        /// </summary>
        public double Orientation { get; set; }

        /// <summary>
        /// Gets or sets the length, in pixels, of the major axis of the
        /// ellipse fit to the connected component.
        /// </summary>
        public double MajorAxisLength { get; set; }

        /// <summary>
        /// Gets or sets the length, in pixels, of the minor axis of the
        /// ellipse fit to the connected component.
        /// </summary>
        public double MinorAxisLength { get; set; }

        /// <summary>
        /// Gets or sets the number of pixels in the connected component.
        /// </summary>
        public double Area { get; set; }

        /// <summary>
        /// Gets or sets the image subregion corresponding to the bounding
        /// box of the connected component. This property might be <see langword="null"/>
        /// if the connected component was created from a polygonal contour.
        /// </summary>
        [XmlIgnore]
        public IplImage Patch { get; set; }

        /// <summary>
        /// Gets or sets the polygonal contour from which the connected component
        /// properties were extracted. This property might be <see langword="null"/>
        /// if the connected component was created from an image.
        /// </summary>
        [XmlIgnore]
        public Contour Contour { get; set; }

        /// <summary>
        /// Returns a <see cref="ConnectedComponent"/> derived from the spatial 
        /// moments of the specified image.
        /// </summary>
        /// <param name="image">
        /// The image from which to derive the spatial moments used to initialize
        /// the <see cref="ConnectedComponent"/>.
        /// </param>
        /// <param name="binary">
        /// Specifies whether all non-zero pixels should be treated as having a
        /// weight of one.
        /// </param>
        /// <returns>
        /// A <see cref="ConnectedComponent"/> object derived from the spatial 
        /// moments of the image. If the area of the connected component is zero,
        /// the centroid and orientation angle will be set to <see cref="float.NaN"/>.
        /// </returns>
        public static ConnectedComponent FromImage(IplImage image, bool binary = false)
        {
            var moments = new Moments(image, binary);
            var component = FromMoments(moments);
            component.Patch = image;
            return component;
        }

        /// <summary>
        /// Returns a <see cref="ConnectedComponent"/> derived from the spatial 
        /// moments of the specified polygon.
        /// </summary>
        /// <param name="currentContour">
        /// The polygon from which to derive the spatial moments used to initialize
        /// the <see cref="ConnectedComponent"/>.
        /// </param>
        /// <returns>
        /// A <see cref="ConnectedComponent"/> object derived from the spatial 
        /// moments of the polygon. If the area of the connected component is zero,
        /// the centroid and orientation angle will be set to <see cref="float.NaN"/>.
        /// </returns>
        public static ConnectedComponent FromContour(Seq currentContour)
        {
            var moments = new Moments(currentContour);
            var component = FromMoments(moments);
            component.Contour = Contour.FromSeq(currentContour);
            return component;
        }

        /// <summary>
        /// Returns a <see cref="ConnectedComponent"/> derived from the spatial 
        /// moments of a polygon or rasterized shape.
        /// </summary>
        /// <param name="moments">
        /// The spatial moments up to third order of a polygon or rasterized shape.
        /// </param>
        /// <returns>
        /// A <see cref="ConnectedComponent"/> object derived from the specified 
        /// spatial moments. If the area of the connected component is zero,
        /// the centroid and orientation angle will be set to <see cref="float.NaN"/>.
        /// </returns>
        public static ConnectedComponent FromMoments(Moments moments)
        {
            var component = new ConnectedComponent();
            component.Area = moments.M00;

            // Cemtral moments can only be computed for components with non-zero area
            if (moments.M00 > 0)
            {
                // Compute centroid components
                var x = moments.M10 / moments.M00;
                var y = moments.M01 / moments.M00;
                component.Centroid = new Point2f((float)x, (float)y);

                // Compute covariance matrix of image intensity
                var miu20 = moments.Mu20 / moments.M00;
                var miu02 = moments.Mu02 / moments.M00;
                var miu11 = moments.Mu11 / moments.M00;

                // Compute orientation and major/minor axis length
                var b = 2 * miu11;
                var a = miu20 - miu02;
                var deviation = Math.Sqrt(b * b + a * a);
                component.Orientation = 0.5 * Math.Atan2(b, a);
                component.MajorAxisLength = 2.75 * Math.Sqrt(miu20 + miu02 + deviation);
                component.MinorAxisLength = 2.75 * Math.Sqrt(miu20 + miu02 - deviation);
            }
            else
            {
                component.Centroid = new Point2f(float.NaN, float.NaN);
                component.Orientation = double.NaN;
            }

            return component;
        }
    }
}
