using OpenTK.Graphics.OpenGL4;

namespace Bonsai.Shaders.Design
{
    /// <summary>
    /// Provides example source code for an OpenGL Shader Language (GLSL) script.
    /// </summary>
    public class GlslScriptExample
    {
        /// <summary>
        /// Gets or sets the name of the GLSL example.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the GLSL source code for the example.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the shader type targeted by the example.
        /// </summary>
        public ShaderType Type { get; set; }
    }
}
