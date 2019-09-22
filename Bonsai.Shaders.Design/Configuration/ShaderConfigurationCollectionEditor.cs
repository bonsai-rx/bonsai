using Bonsai.Resources.Design;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration.Design
{
    class ShaderConfigurationCollectionEditor : ResourceCollectionEditor
    {
        public ShaderConfigurationCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(ShaderConfiguration);
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new[] { typeof(MaterialConfiguration), typeof(ViewportEffectConfiguration), typeof(ComputeProgramConfiguration) };
        }

        protected override string[] CreateSupportedExtensions()
        {
            return new[] { ".mtl" };
        }

        protected override object CreateResourceConfiguration(string fileName)
        {
            var material = new MaterialConfiguration();
            material.Name = Path.GetFileNameWithoutExtension(fileName);
            MtlReader.ReadMaterial(material, fileName);
            return material;
        }
    }
}
