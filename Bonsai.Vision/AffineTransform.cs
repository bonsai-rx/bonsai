using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that creates an affine transformation matrix specified
    /// by a translation, rotation and scale.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("Creates an affine transformation matrix specified by a translation, rotation and scale.")]
    public class AffineTransform : Combinator<Mat>
    {
        Mat transform;
        Point2f pivot;
        Point2f translation;
        float rotation;
        Point2f scale;
        event Action<Mat> PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="AffineTransform"/> class.
        /// </summary>
        public AffineTransform()
        {
            Scale = new Point2f(1, 1);
        }

        /// <summary>
        /// Gets or sets the pivot around which to scale or rotate the image.
        /// </summary>
        [Description("The pivot around which to scale or rotate the image.")]
        public Point2f Pivot
        {
            get { return pivot; }
            set
            {
                pivot = value;
                transform = CreateTransform(translation, rotation, scale, pivot);
                OnPropertyChanged(transform);
            }
        }

        /// <summary>
        /// Gets or sets the translation vector to apply to the image.
        /// </summary>
        [Description("The translation vector to apply to the image.")]
        public Point2f Translation
        {
            get { return translation; }
            set
            {
                translation = value;
                transform = CreateTransform(translation, rotation, scale, pivot);
                OnPropertyChanged(transform);
            }
        }

        /// <summary>
        /// Gets or sets the rotation angle around the pivot, in radians.
        /// </summary>
        [Range(-Math.PI, Math.PI)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The rotation angle around the pivot, in radians.")]
        public float Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                transform = CreateTransform(translation, rotation, scale, pivot);
                OnPropertyChanged(transform);
            }
        }

        /// <summary>
        /// Gets or sets the scale factor to apply to individual image dimensions.
        /// </summary>
        [Description("The scale factor to apply to individual image dimensions.")]
        public Point2f Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                transform = CreateTransform(translation, rotation, scale, pivot);
                OnPropertyChanged(transform);
            }
        }

        void OnPropertyChanged(Mat value)
        {
            PropertyChanged?.Invoke(value);
        }

        static Mat CreateTransform(Point2f translation, double rotation, Point2f scale, Point2f pivot)
        {
            var rcos = (float)Math.Cos(rotation);
            var rsin = (float)Math.Sin(rotation);
            var pivotOffsetX = -pivot.X * scale.X * rcos - pivot.Y * scale.X * rsin + pivot.X;
            var pivotOffsetY = pivot.X * scale.Y * rsin - pivot.Y * scale.Y * rcos + pivot.Y;
            return Mat.FromArray(new float[,]
            {
                { scale.X * rcos, scale.X * rsin, pivotOffsetX + translation.X },
                { -scale.Y * rsin, scale.Y * rcos, pivotOffsetY + translation.Y }
            });
        }

        /// <summary>
        /// Generates an observable sequence that contains an affine transformation
        /// matrix specified by a translation, rotation and scale.
        /// </summary>
        /// <returns>
        /// A sequence containing a single instance of the <see cref="Mat"/>
        /// class representing an affine transformation matrix.
        /// </returns>
        public IObservable<Mat> Process()
        {
            return Observable
                .Defer(() => Observable.Return(transform))
                .Concat(Observable.FromEvent<Mat>(
                    handler => PropertyChanged += handler,
                    handler => PropertyChanged -= handler));
        }

        /// <summary>
        /// Generates an observable sequence of affine transformation matrices using
        /// the specified translation, rotation and scale, and where each matrix is
        /// emitted only when an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting new affine
        /// transformation matrices.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects where each element
        /// represents an affine transformation matrix.
        /// </returns>
        public override IObservable<Mat> Process<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => transform);
        }
    }
}
