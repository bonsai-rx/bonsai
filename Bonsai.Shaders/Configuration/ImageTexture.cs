using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration
{
    public class ImageTexture : Texture2D
    {
        public ImageTexture()
        {
            Mode = LoadImageFlags.Unchanged;
        }

        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        [FileNameFilter("Image Files|*.png;*.bmp;*.jpg;*.jpeg;*.tif|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|TIFF Files (*.tif)|*.tif")]
        [Description("The name of the image file.")]
        public string FileName { get; set; }

        [Description("Specifies optional conversions applied to the loaded image.")]
        public LoadImageFlags Mode { get; set; }

        public override Texture CreateResource()
        {
            var texture = base.CreateResource();
            var image = CV.LoadImage(FileName, Mode);
            var width = Width.GetValueOrDefault();
            var height = Height.GetValueOrDefault();
            if (width > 0 && height > 0 && (image.Width != width || image.Height != height))
            {
                var resized = new IplImage(new Size(width, height), image.Depth, image.Channels);
                CV.Resize(image, resized);
                image = resized;
            }
            TextureHelper.UpdateTexture(texture.Id, InternalFormat, image);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return texture;
        }

        public override string ToString()
        {
            var name = Name;
            var fileName = FileName;
            var typeName = GetType().Name;
            if (string.IsNullOrEmpty(name)) return typeName;
            else if (string.IsNullOrEmpty(fileName)) return name;
            else return string.Format("{0} [{1}]", name, fileName);
        }
    }
}
