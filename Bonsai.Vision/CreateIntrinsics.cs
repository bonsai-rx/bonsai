using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [Description("Creates a set of parameters specifying the camera intrinsics.")]
    public class CreateIntrinsics : Source<Intrinsics>
    {
        public CreateIntrinsics()
        {
            FocalLength = new Point2d(1, 1);
        }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The optional image size, in pixels, for the camera intrinsics.")]
        public Size? ImageSize { get; set; }

        [Description("The focal length of the camera, expressed in pixel units.")]
        public Point2d FocalLength { get; set; }

        [Description("The principal point of the camera, usually at the image center.")]
        public Point2d PrincipalPoint { get; set; }

        [Description("The radial distortion coefficients.")]
        public Point3d RadialDistortion { get; set; }

        [Description("The tangential distortion coefficients.")]
        public Point2d TangentialDistortion { get; set; }

        Intrinsics Create()
        {
            return new Intrinsics
            {
                ImageSize = ImageSize,
                FocalLength = FocalLength,
                PrincipalPoint = PrincipalPoint,
                RadialDistortion = RadialDistortion,
                TangentialDistortion = TangentialDistortion
            };
        }

        public override IObservable<Intrinsics> Generate()
        {
            return Observable.Defer(() => Observable.Return(Create()));
        }

        public IObservable<Intrinsics> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => Create());
        }
    }
}
