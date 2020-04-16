using OpenCV.Net;

namespace Bonsai.Vision
{
    public class Contours
    {
        public Contours(Seq firstContour, Size imageSize)
        {
            FirstContour = firstContour;
            ImageSize = imageSize;
        }

        public Seq FirstContour { get; private set; }

        public Size ImageSize { get; private set; }
    }
}
