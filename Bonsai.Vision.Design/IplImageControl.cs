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

namespace Bonsai.Vision.Design
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
                var bitmap = ConvertImage(image);
                SetPictureBoxImage(bitmap);
            }
        }

        void SetPictureBoxImage(Bitmap bitmap)
        {
            if (pictureBox.InvokeRequired)
            {
                pictureBox.BeginInvoke((Action<Bitmap>)SetPictureBoxImage, bitmap);
            }
            else pictureBox.Image = bitmap;
        }

        Bitmap ConvertImage(IplImage image)
        {
            if (image == null) return null;
            if (image.Depth != 8) throw new ArgumentException("Non 8-bit depth images are not supported by the control.", "image");

            var bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var bitmapImage = new IplImage(image.Size, 8, 4, bitmapData.Scan0);

            try
            {
                switch (image.NumChannels)
                {
                    case 1: ImgProc.cvCvtColor(image, bitmapImage, ColorConversion.GRAY2BGRA); break;
                    case 3: ImgProc.cvCvtColor(image, bitmapImage, ColorConversion.BGR2BGRA); break;
                    case 4: Core.cvCopy(image, bitmapImage); break;
                    default: throw new ArgumentException("Image has an unsupported number of channels.", "image");
                }
            }
            finally
            {
                bitmapImage.Close();
                bitmap.UnlockBits(bitmapData);
            }

            return bitmap;
        }
    }
}
