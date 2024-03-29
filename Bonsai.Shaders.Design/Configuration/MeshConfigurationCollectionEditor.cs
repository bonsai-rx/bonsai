﻿using Bonsai.Design;
using Bonsai.Resources.Design;
using System;
using System.IO;

namespace Bonsai.Shaders.Configuration.Design
{
    class MeshConfigurationCollectionEditor : ResourceCollectionEditor
    {
        public MeshConfigurationCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(MeshConfiguration);
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new[] { typeof(MeshConfiguration), typeof(TexturedQuad), typeof(TexturedModel) };
        }

        protected override string[] CreateSupportedExtensions()
        {
            return new[] { ".obj" };
        }

        protected override object CreateResourceConfiguration(string fileName)
        {
            var configuration = new TexturedModel();
            configuration.FileName = PathConvert.GetProjectPath(fileName);
            configuration.Name = Path.GetFileNameWithoutExtension(fileName);
            return configuration;
        }
    }
}
