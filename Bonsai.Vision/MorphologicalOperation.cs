using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class MorphologicalOperation : Projection<IplImage, IplImage>
    {
        CvSize size;
        CvPoint anchor;
        StructuringElementShape shape;
        IplConvKernel strel;
        bool propertyChanged;

        public MorphologicalOperation()
        {
            Size = new CvSize(3, 3);
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

        public MorphologicalOperator Operator { get; set; }

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
            switch (Operator)
            {
                case MorphologicalOperator.Erode:
                    ImgProc.cvErode(input, output, StructuringElement, Iterations);
                    break;
                case MorphologicalOperator.Dilate:
                    ImgProc.cvDilate(input, output, StructuringElement, Iterations);
                    break;
                case MorphologicalOperator.Open:
                    ImgProc.cvErode(input, output, StructuringElement, Iterations);
                    ImgProc.cvDilate(output, output, StructuringElement, Iterations);
                    break;
                case MorphologicalOperator.Close:
                    ImgProc.cvDilate(input, output, StructuringElement, Iterations);
                    ImgProc.cvErode(output, output, StructuringElement, Iterations);
                    break;
                case MorphologicalOperator.TopHat:
                    ImgProc.cvErode(input, output, StructuringElement, Iterations);
                    ImgProc.cvDilate(output, output, StructuringElement, Iterations);
                    Core.cvSub(input, output, output, CvArr.Null);
                    break;
                case MorphologicalOperator.BotHat:
                    ImgProc.cvDilate(input, output, StructuringElement, Iterations);
                    ImgProc.cvErode(output, output, StructuringElement, Iterations);
                    Core.cvSub(output, input, output, CvArr.Null);
                    break;
                default:
                    break;
            }
            return output;
        }

        protected override void Unload()
        {
            if (strel != null)
            {
                strel.Close();
                strel = null;
            }
            base.Unload();
        }
    }

    public enum MorphologicalOperator
    {
        Erode,
        Dilate,
        Open,
        Close,
        TopHat,
        BotHat
    }
}
