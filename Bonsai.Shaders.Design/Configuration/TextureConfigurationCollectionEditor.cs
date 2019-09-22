using Bonsai.Design;
using Bonsai.Resources.Design;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration.Design
{
    class TextureConfigurationCollectionEditor : ResourceCollectionEditor
    {
        static readonly string[] ImageExtensions = new[] { ".png", ".bmp", ".jpg", ".jpeg", ".tif" };

        public TextureConfigurationCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(TextureConfiguration);
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new[] { typeof(Texture2D), typeof(Cubemap), typeof(ImageTexture), typeof(ImageCubemap) };
        }

        protected override string[] CreateSupportedExtensions()
        {
            return ImageExtensions;
        }

        protected override object CreateResourceConfiguration(string fileName)
        {
            var configuration = new ImageTexture();
            configuration.FlipMode = FlipMode.Vertical;
            configuration.FileName = PathConvert.GetProjectPath(fileName);
            configuration.Name = Path.GetFileNameWithoutExtension(fileName);
            return configuration;
        }
    }
}
