using OpenCV.Net;

namespace Bonsai.Vision
{
    public class ScalarHistogram
    {
        public ScalarHistogram(Histogram val0, Histogram val1, Histogram val2, Histogram val3)
        {
            Val0 = val0;
            Val1 = val1;
            Val2 = val2;
            Val3 = val3;
        }

        public Histogram Val0 { get; private set; }

        public Histogram Val1 { get; private set; }

        public Histogram Val2 { get; private set; }

        public Histogram Val3 { get; private set; }
    }
}
