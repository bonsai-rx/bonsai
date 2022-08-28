using Bonsai.Design;
using Bonsai.Resources.Design;
using System;
using System.IO;
using System.Windows.Forms;

namespace Bonsai.Shaders.Configuration.Design
{
    class TextureConfigurationCollectionEditor : ResourceCollectionEditor
    {
        static readonly string[] ImageExtensions = new[] { ".png", ".bmp", ".jpg", ".jpeg", ".tif" };
        static readonly string[] VideoExtensions = new[] { ".avi", ".mp4", ".ogg", ".ogv", ".wmv" };

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
            return new[] { typeof(Texture2D), typeof(Cubemap), typeof(ImageTexture), typeof(ImageCubemap), typeof(ImageSequence), typeof(VideoTexture) };
        }

        protected override string[] CreateSupportedExtensions()
        {
            var extensions = new string[ImageExtensions.Length + VideoExtensions.Length];
            ImageExtensions.CopyTo(extensions, 0);
            VideoExtensions.CopyTo(extensions, ImageExtensions.Length);
            return extensions;
        }

        protected override object CreateResourceConfiguration(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            fileName = PathConvert.GetProjectPath(fileName);

            Texture2D configuration;
            if (Array.IndexOf(VideoExtensions, extension) >= 0)
            {
                if (Control.ModifierKeys == Keys.Control) configuration = new ImageSequence { FileName = fileName };
                else configuration = new VideoTexture { FileName = fileName };
            }
            else configuration = new ImageTexture { FileName = fileName };
            configuration.Name = Path.GetFileNameWithoutExtension(fileName);
            return configuration;
        }
    }
}
