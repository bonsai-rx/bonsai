using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration.Design
{
    class DragTextureConfiguration : DragResourceConfiguration
    {
        static readonly string[] AllowedExtensions = new[] { ".png", ".bmp", ".jpg", ".jpeg", ".tif" };

        public DragTextureConfiguration(CollectionEditorControl editor)
            : base(editor)
        {
        }

        protected override bool IsResourceAllowed(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return Array.Exists(AllowedExtensions, extension.Equals);
        }

        protected override object CreateResourceConfiguration(string fileName)
        {
            var configuration = new ImageTexture();
            configuration.FileName = PathConvert.GetProjectPath(fileName);
            configuration.Name = Path.GetFileNameWithoutExtension(fileName);
            return configuration;
        }
    }
}
