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
    }
}
