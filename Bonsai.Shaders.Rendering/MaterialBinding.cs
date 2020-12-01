using Bonsai.Resources;
using OpenTK.Graphics.OpenGL4;
using System;

namespace Bonsai.Shaders.Rendering
{
    class MaterialBinding
    {
        static readonly Action EmptyAction = () => { };
        readonly Assimp.Material resource;
        readonly Action bind;

        public MaterialBinding(Shader shader, ResourceManager resourceManager, Assimp.Material material)
        {
            if (shader == null)
            {
                throw new ArgumentNullException(nameof(shader));
            }

            if (resourceManager == null)
            {
                throw new ArgumentNullException(nameof(resourceManager));
            }

            resource = material ?? throw new ArgumentNullException(nameof(material));
            bind = EmptyAction;

            if (resource.HasBumpScaling) BindUniform(ref bind, shader, ShaderConstants.BumpScaling, resource.BumpScaling);
            if (resource.HasColorAmbient) BindUniform(ref bind, shader, ShaderConstants.ColorAmbient, material.ColorAmbient);
            if (resource.HasColorDiffuse) BindUniform(ref bind, shader, ShaderConstants.ColorDiffuse, material.ColorDiffuse);
            if (resource.HasColorEmissive) BindUniform(ref bind, shader, ShaderConstants.ColorEmissive, material.ColorEmissive);
            if (resource.HasColorReflective) BindUniform(ref bind, shader, ShaderConstants.ColorReflective, material.ColorReflective);
            if (resource.HasColorSpecular) BindUniform(ref bind, shader, ShaderConstants.ColorSpecular, material.ColorSpecular);
            if (resource.HasColorTransparent) BindUniform(ref bind, shader, ShaderConstants.ColorTransparent, material.ColorTransparent);
            if (resource.HasOpacity) BindUniform(ref bind, shader, ShaderConstants.Opacity, material.Opacity);
            if (resource.HasReflectivity) BindUniform(ref bind, shader, ShaderConstants.Reflectivity, material.Reflectivity);
            if (resource.HasShininess) BindUniform(ref bind, shader, ShaderConstants.Shininess, material.Shininess);
            if (resource.HasShininessStrength) BindUniform(ref bind, shader, ShaderConstants.ShininessStrength, material.ShininessStrength);

            var textures = resource.GetAllMaterialTextures();
            for (int i = 0; i < textures.Length; i++)
            {
            }
        }

        internal void Bind()
        {
            bind();
        }

        static void BindUniform(ref Action bind, Shader shader, string name, Assimp.Color4D color)
        {
            var location = GL.GetUniformLocation(shader.Program, name);
            if (location >= 0)
            {
                bind += () => GL.Uniform4(location, color.R, color.G, color.B, color.A);
            }
        }

        static void BindUniform(ref Action bind, Shader shader, string name, float value)
        {
            var location = GL.GetUniformLocation(shader.Program, name);
            if (location >= 0)
            {
                bind += () => GL.Uniform1(location, value);
            }
        }

        static void BindUniform(ref Action bind, Shader shader, string name, Assimp.TextureSlot texture)
        {
            var location = GL.GetUniformLocation(shader.Program, name);
            if (location >= 0)
            {
            }
        }
    }
}
