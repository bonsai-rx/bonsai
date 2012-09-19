using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class GoodFeaturesToTrack : Projection<IplImage, KeyPointCollection>
    {
        IplImage temp;
        IplImage eigen;
        CvPoint2D32f[] corners;

        public GoodFeaturesToTrack()
        {
            MaxFeatures = 100;
            QualityLevel = 0.01;
        }

        public int MaxFeatures { get; set; }

        public double QualityLevel { get; set; }

        public double MinDistance { get; set; }

        public override KeyPointCollection Process(IplImage input)
        {
            var result = new KeyPointCollection(input);
            temp = IplImageHelper.EnsureImageFormat(temp, input.Size, 32, 1);
            eigen = IplImageHelper.EnsureImageFormat(eigen, input.Size, 32, 1);
            if (corners == null || corners.Length != MaxFeatures)
            {
                corners = new CvPoint2D32f[MaxFeatures];
            }

            int cornerCount = corners.Length;
            ImgProc.cvGoodFeaturesToTrack(input, eigen, temp, corners, ref cornerCount, QualityLevel, MinDistance, CvArr.Null, 3, 0, 0.04);
            for (int i = 0; i < cornerCount; i++)
            {
                result.Add(new KeyPoint(corners[i]));
            }

            return result;
        }

        protected override void Unload()
        {
            if (temp != null)
            {
                temp.Close();
                eigen.Close();
                temp = eigen = null;
            }
            base.Unload();
        }
    }
}
