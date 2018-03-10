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
    [Description("Creates a set of parameters specifying the camera extrinsics.")]    
    public class CreateExtrinsics : Source<Extrinsics>
    {
        [Description("The rotation vector transforming object coordinates into camera coordinates.")]
        public Point3d Rotation { get; set; }

        [Description("The translation vector transforming object coordinates into camera coordinates.")]
        public Point3d Translation { get; set; }

        Extrinsics Create()
        {
            return new Extrinsics
            {
                Rotation = Rotation,
                Translation = Translation
            };
        }

        public override IObservable<Extrinsics> Generate()
        {
            return Observable.Defer(() => Observable.Return(Create()));
        }

        public IObservable<Extrinsics> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => Create());
        }
    }
}
