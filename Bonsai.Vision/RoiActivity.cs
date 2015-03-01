using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.InteropServices;

namespace Bonsai.Vision
{
    [DefaultProperty("Regions")]
    [Description("Sums all the activation intensity inside specified regions of interest.")]
    public class RoiActivity : Transform<IplImage, RegionActivityCollection>
    {
        IplImage roi;
        IplImage mask;
        Point[][] currentRegions;
        Rect[] boundingRegions;

        [Description("The regions of interest for which to sum activation intensity.")]
        [Editor("Bonsai.Vision.Design.IplImageInputRoiEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public Point[][] Regions { get; set; }

        public override IObservable<RegionActivityCollection> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new RegionActivityCollection();
                mask = IplImageHelper.EnsureImageFormat(mask, input.Size, IplDepth.U8, 1);
                roi = IplImageHelper.EnsureImageFormat(roi, input.Size, input.Depth, input.Channels);
                if (Regions != currentRegions)
                {
                    mask.SetZero();
                    currentRegions = Regions;
                    CV.FillPoly(mask, currentRegions, Scalar.All(255));
                    boundingRegions = currentRegions.Select(polygon =>
                    {
                        var points = polygon.SelectMany(point => new[] { point.X, point.Y }).ToArray();
                        using (var mat = new Mat(1, polygon.Length, Depth.S32, 2))
                        {
                            Marshal.Copy(points, 0, mat.Data, points.Length);
                            return CV.BoundingRect(mat);
                        }
                    }).ToArray();
                }

                if (currentRegions != null)
                {
                    roi.SetZero();
                    CV.Copy(input, roi, mask);
                    for (int i = 0; i < boundingRegions.Length; i++)
                    {
                        var region = boundingRegions[i];
                        var polygon = currentRegions[i];
                        roi.RegionOfInterest = region;
                        output.Add(new RegionActivity
                        {
                            Roi = polygon,
                            Rect = region,
                            Activity = CV.Sum(roi)
                        });
                    }
                    roi.ResetRegionOfInterest();
                }

                return output;
            });
        }
    }
}
