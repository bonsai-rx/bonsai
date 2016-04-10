﻿using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlInclude(typeof(TexturedQuad))]
    [XmlInclude(typeof(TexturedModel))]
    public class MeshConfiguration : ResourceConfiguration<Mesh>
    {
        public override Mesh CreateResource()
        {
            return new Mesh();
        }
    }
}
