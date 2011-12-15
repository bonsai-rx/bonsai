using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenCV.Net;
using System.Drawing.Imaging;

namespace Bonsai
{
    public partial class IplImageControl : UserControl
    {
        IplImage image;

        public IplImageControl()
        {
            InitializeComponent();
        }

        public IplImage Image
        {
            get { return image; }
            set
            {
                image = value;
                if (image != null)
                {
                    var bitmap = new Bitmap(image.Width, image.Height, image.WidthStep, GetPixelFormat(image), image.ImageData);
                    pictureBox.Image = bitmap;
                }
                else pictureBox.Image = null;
            }
        }

        PixelFormat GetPixelFormat(IplImage image)
        {
            switch (image.NumChannels)
            {
                case 1:
                    switch (image.Depth)
                    {
                        case 8: return PixelFormat.Format8bppIndexed;
                        case 16: return PixelFormat.Format16bppGrayScale;
                        default: return PixelFormat.DontCare;
                    }
                case 3:
                    switch (image.Depth)
                    {
                        case 8: return PixelFormat.Format24bppRgb;
                        default: return PixelFormat.DontCare;
                    }
                case 4:
                    switch (image.Depth)
                    {
                        case 8: return PixelFormat.Format32bppArgb;
                        default: return PixelFormat.DontCare;
                    }
                default: return PixelFormat.DontCare;
            }
        }
    }
}
