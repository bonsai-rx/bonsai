using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class ImageTexture : Texture2D
    {
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        [FileNameFilter("Image Files|*.png;*.bmp;*.jpg;*.jpeg;*.tif|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|TIFF Files (*.tif)|*.tif")]
        [Description("The name of the image file.")]
        public string FileName { get; set; }

        [Description("Specifies optional conversions applied to the loaded image.")]
        public LoadImageFlags Mode { get; set; }

        public override void Load(Shader shader)
        {
            base.Load(shader);
            var texture = GetTexture();
            var image = CV.LoadImage(FileName, Mode);
            TextureHelper.UpdateTexture(texture, image);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
