using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public static class IplImageHelper
    {
        public static IplImage EnsureImageFormat(IplImage output, CvSize size, int depth, int channels)
        {
            if (output == null || output.Size != size || output.Depth != depth || output.NumChannels != channels)
            {
                if (output != null) output.Close();
                return new IplImage(size, depth, channels);
            }

            return output;
        }

        public static IplImage EnsureColorCopy(IplImage output, IplImage image)
        {
            output = EnsureImageFormat(output, image.Size, image.Depth, 3);
            if (image.NumChannels == 1) ImgProc.cvCvtColor(image, output, ColorConversion.GRAY2BGR);
            else Core.cvCopy(image, output);
            return output;
        }
    }
}
