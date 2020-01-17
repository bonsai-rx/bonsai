using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Bonsai.Vision
{
    [DefaultProperty("Regions")]
    [Description("Calculates activation intensity inside specified regions of interest.")]
    public class RoiActivity : Transform<IplImage, RegionActivityCollection>
    {
        public RoiActivity()
        {
            Operation = ReduceOperation.Sum;
        }

        [Description("The regions of interest for which to calculate activation intensity.")]
        [Editor("Bonsai.Vision.Design.IplImageInputLabeledRoiEditor, Bonsai.Vision.Design", DesignTypes.UITypeEditor)]
        public Point[][] Regions { get; set; }

        [Description("The reduction operation used to calculate activation intensity.")]
        public ReduceOperation Operation { get; set; }

        public override IObservable<RegionActivityCollection> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                var roi = default(IplImage);
                var mask = default(IplImage);
                var currentRegions = default(Point[][]);
                var boundingRegions = default(Rect[]);
                return source.Select(input =>
                {
                    var operation = Operation;
                    var output = new RegionActivityCollection();
                    mask = IplImageHelper.EnsureImageFormat(mask, input.Size, IplDepth.U8, 1);
                    if (operation != ReduceOperation.Sum) roi = null;
                    else roi = IplImageHelper.EnsureImageFormat(roi, input.Size, input.Depth, input.Channels);
                    if (Regions != currentRegions)
                    {
                        currentRegions = Regions;
                        if (currentRegions != null)
                        {
                            mask.SetZero();
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
                    }

                    if (currentRegions != null)
                    {
                        var activeMask = mask;
                        if (roi != null)
                        {
                            roi.SetZero();
                            CV.Copy(input, roi, mask);
                            activeMask = roi;
                        }

                        var activation = ActivationFunction(operation);
                        for (int i = 0; i < boundingRegions.Length; i++)
                        {
                            var rect = boundingRegions[i];
                            var polygon = currentRegions[i];
                            using (var region = input.GetSubRect(rect))
                            using (var regionMask = activeMask.GetSubRect(rect))
                            {
                                output.Add(new RegionActivity
                                {
                                    Roi = polygon,
                                    Rect = rect,
                                    Activity = activation(region, regionMask)
                                });
                            }
                        }
                    }

                    return output;
                });
            });
        }

        static Func<IplImage, IplImage, Scalar> ActivationFunction(ReduceOperation operation)
        {
            switch (operation)
            {
                case ReduceOperation.Avg: return CV.Avg;
                case ReduceOperation.Max: return (image, mask) =>
                {
                    Scalar min, max;
                    MinMaxLoc(image, mask, out min, out max);
                    return max;
                };
                case ReduceOperation.Min: return (image, mask) =>
                {
                    Scalar min, max;
                    MinMaxLoc(image, mask, out min, out max);
                    return min;
                };
                case ReduceOperation.Sum: return (image, mask) => CV.Sum(mask);
                default: throw new InvalidOperationException("The specified reduction operation is invalid.");
            }
        }

        static void MinMaxLoc(IplImage image, IplImage mask, out Scalar min, out Scalar max)
        {
            Point minLoc, maxLoc;
            if (image.Channels == 1)
            {
                CV.MinMaxLoc(image, out min.Val0, out max.Val0, out minLoc, out maxLoc, mask);
                min.Val1 = max.Val1 = min.Val2 = max.Val2 = min.Val3 = max.Val3 = 0;
            }
            else
            {
                using (var coi = image.GetSubRect(new Rect(0, 0, image.Width, image.Height)))
                {
                    coi.ChannelOfInterest = 1;
                    CV.MinMaxLoc(coi, out min.Val0, out max.Val0, out minLoc, out maxLoc, mask);
                    coi.ChannelOfInterest = 2;
                    CV.MinMaxLoc(coi, out min.Val1, out max.Val1, out minLoc, out maxLoc, mask);
                    if (image.Channels > 2)
                    {
                        coi.ChannelOfInterest = 3;
                        CV.MinMaxLoc(coi, out min.Val2, out max.Val2, out minLoc, out maxLoc, mask);
                        if (image.Channels > 3)
                        {
                            coi.ChannelOfInterest = 4;
                            CV.MinMaxLoc(coi, out min.Val3, out max.Val3, out minLoc, out maxLoc, mask);
                        }
                        else min.Val3 = max.Val3 = 0;
                    }
                    else min.Val2 = max.Val2 = min.Val3 = max.Val3 = 0;
                }
            }
        }
    }
}
