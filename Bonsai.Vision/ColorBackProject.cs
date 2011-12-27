using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class ColorBackProject : Filter<IplImage, IplImage>
    {
        IplImage hue;
        IplImage saturation;
        IplImage output;
        CvHistogram histogram;

        public const float HueMax = 180;
        public const float SatMax = 255;
        public const int HueBins = (int)HueMax;
        public const int SatBins = (int)SatMax;

        public ColorBackProject()
        {
            histogram = new CvHistogram(
                2, new[] { HueBins, SatBins },
                HistogramType.Array,
                new[] { new[] { 0, HueMax }, new[] { 0, SatMax } }, true);
        }

        [Editor("Bonsai.Vision.Design.HueSaturationHistogramEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public CvHistogram Histogram
        {
            get { return histogram; }
            set { histogram = value; }
        }

        public override IplImage Process(IplImage input)
        {
            if (output == null)
            {
                hue = new IplImage(input.Size, 8, 1);
                saturation = new IplImage(input.Size, 8, 1);
                output = new IplImage(input.Size, 8, 1);
            }

            Core.cvSplit(input, hue, saturation, CvArr.Null, CvArr.Null);
            ImgProc.cvCalcBackProject(new IplImage[] { hue, saturation }, output, histogram);
            return output;
        }

        public override void Unload(WorkflowContext context)
        {
            if (output != null)
            {
                output.Close();
                output = null;
            }
            base.Unload(context);
        }
    }
}
