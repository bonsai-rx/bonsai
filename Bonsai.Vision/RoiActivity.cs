using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.InteropServices;

namespace Bonsai.Vision
{
    public class RoiActivity : Selector<IplImage, RegionActivityCollection>
    {
        IplImage roi;
        IplImage mask;
        int[] polygonLength;
        CvPoint[][] currentRegions;
        CvRect[] boundingRegions;

        [Editor("Bonsai.Vision.Design.IplImageInputRoiEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public CvPoint[][] Regions { get; set; }

        public override RegionActivityCollection Process(IplImage input)
        {
            var output = new RegionActivityCollection();
            mask = IplImageHelper.EnsureImageFormat(mask, input.Size, 8, 1);
            roi = IplImageHelper.EnsureImageFormat(roi, input.Size, input.Depth, input.NumChannels);
            if (Regions != currentRegions)
            {
                mask.SetZero();
                currentRegions = Regions;
                polygonLength = currentRegions.Select(polygon => polygon.Length).ToArray();
                Core.cvFillPoly(mask, currentRegions, polygonLength, currentRegions.Length, CvScalar.All(255), 8, 0);
                boundingRegions = currentRegions.Select(polygon =>
                {
                    var points = polygon.SelectMany(point => new[] { point.X, point.Y }).ToArray();
                    using (var mat = new CvMat(1, polygon.Length, CvMatDepth.CV_32S, 2))
                    {
                        Marshal.Copy(points, 0, mat.Data, points.Length);
                        return ImgProc.cvBoundingRect(mat, 0);
                    }
                }).ToArray();
            }

            if (currentRegions != null)
            {
                roi.SetZero();
                Core.cvCopy(input, roi, mask);
                for (int i = 0; i < boundingRegions.Length; i++)
                {
                    var region = boundingRegions[i];
                    var polygon = currentRegions[i];
                    roi.ImageROI = region;
                    output.Add(new RegionActivity
                    {
                        Roi = polygon,
                        Rect = region,
                        Activity = Core.cvSum(roi)
                    });
                }
                roi.ResetImageROI();
            }

            return output;
        }
    }
}
