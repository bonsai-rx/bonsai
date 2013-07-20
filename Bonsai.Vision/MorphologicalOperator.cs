using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class MorphologicalOperator : Transform<IplImage, IplImage>
    {
        CvSize size;
        CvPoint anchor;
        StructuringElementShape shape;
        IplConvKernel strel;
        bool propertyChanged;
        IplImage temp;

        public MorphologicalOperator()
        {
            Size = new CvSize(3, 3);
            Anchor = new CvPoint(1, 1);
            Iterations = 1;
        }

        public CvSize Size
        {
            get { return size; }
            set
            {
                size = value;
                propertyChanged = true;
            }
        }

        public CvPoint Anchor
        {
            get { return anchor; }
            set
            {
                anchor = value;
                propertyChanged = true;
            }
        }

        public StructuringElementShape Shape
        {
            get { return shape; }
            set
            {
                shape = value;
                propertyChanged = true;
            }
        }

        public int Iterations { get; set; }

        public OpenCV.Net.MorphologicalOperation Operation { get; set; }

        protected IplConvKernel StructuringElement
        {
            get
            {
                if (strel == null || propertyChanged)
                {
                    propertyChanged = false;
                    if (strel != null) strel.Close();
                    strel = new IplConvKernel(Size.Width, Size.Height, Anchor.X, Anchor.Y, Shape);
                }

                return strel;
            }
        }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, input.Depth, input.NumChannels);
            temp = IplImageHelper.EnsureImageFormat(temp, input.Size, input.Depth, input.NumChannels);
            ImgProc.cvMorphologyEx(input, output, temp, StructuringElement, Operation, Iterations);
            return output;
        }

        protected override void Unload()
        {
            if (strel != null)
            {
                strel.Close();
                strel = null;
            }

            if (temp != null)
            {
                temp.Close();
                temp = null;
            }
            base.Unload();
        }
    }
}
