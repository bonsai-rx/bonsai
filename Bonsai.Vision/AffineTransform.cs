using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
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

        public AffineTransform()
        {
            Scale = new Point2f(1, 1);
        }

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
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(value);
            }
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

        public IObservable<Mat> Process()
        {
            return Observable
                .Defer(() => Observable.Return(transform))
                .Concat(Observable.FromEvent<Mat>(
                    handler => PropertyChanged += handler,
                    handler => PropertyChanged -= handler));
        }

        public override IObservable<Mat> Process<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => transform);
        }
    }
}
