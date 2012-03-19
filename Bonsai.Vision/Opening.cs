using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class Opening : Projection<IplImage, IplImage>
    {
        IplConvKernel strel;

        public Opening()
        {
            Size = 3;
            Iterations = 1;
        }

        public int Size { get; set; }

        public StructuringElementShape Shape { get; set; }

        public int Iterations { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, input.Depth, input.NumChannels);
            ImgProc.cvErode(input, output, strel, Iterations);
            ImgProc.cvDilate(output, output, strel, Iterations);
            return output;
        }

        public override IDisposable Load()
        {
            strel = new IplConvKernel(Size, Size, 0, 0, Shape);
            return base.Load();
        }

        protected override void Unload()
        {
            strel.Close();
            base.Unload();
        }
    }
}
