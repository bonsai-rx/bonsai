using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Creates an affine transformation matrix specified by a translation, rotation and scale.")]
    public class AffineTransform : Combinator<Mat>
    {
        public AffineTransform()
        {
            Scale = new Point2f(1, 1);
        }

        [Description("The pivot around which to scale or rotate the image.")]
        public Point2f Pivot { get; set; }

        [Description("The translation vector to apply to the image.")]
        public Point2f Translation { get; set; }

        [Precision(3, 0.001)]
        [Range(-Math.PI, Math.PI)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Description("The rotation angle around the pivot, in radians.")]
        public float Rotation { get; set; }

        [Description("The scale factor to apply to individual image dimensions.")]
        public Point2f Scale { get; set; }

        static Mat CreateTransform(Point2f translation, double rotation, Point2f scale, Point2f pivot)
        {
            var alpha = (float)(scale.X * Math.Cos(rotation));
            var beta = (float)(scale.Y * Math.Sin(rotation));
            return Mat.FromArray(new float[,]
            {
                { alpha, beta, (1 - alpha) * pivot.X - beta * pivot.Y + translation.X },
                { -beta, alpha, beta * pivot.X + (1 - alpha) * pivot.Y + translation.Y }
            });
        }

        public override IObservable<Mat> Process<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => CreateTransform(Translation, Rotation, Scale, Pivot));
        }
    }
}
