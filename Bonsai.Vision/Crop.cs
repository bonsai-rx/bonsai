using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [DefaultProperty("RegionOfInterest")]
    [Description("Crops out a subregion of the input image.")]
    public class Crop : Transform<IplImage, IplImage>
    {
        [Description("The region of interest inside the input image.")]
        [Editor("Bonsai.Vision.Design.IplImageInputRectangleEditor, Bonsai.Vision.Design", DesignTypes.UITypeEditor)]
        public Rect RegionOfInterest { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var rect = RegionOfInterest;
                if (rect.Width > 0 && rect.Height > 0)
                {
                    return input.GetSubRect(rect);
                }

                return input;
            });
        }
    }
}
