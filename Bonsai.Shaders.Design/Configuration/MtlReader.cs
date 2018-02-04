using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration.Design
{
    static class MtlReader
    {
        static float[] ParseValues(string[] values)
        {
            var result = new float[values.Length - 1];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = float.Parse(values[i + 1]);
            }

            return result;
        }

        internal static void ReadMaterial(MaterialConfiguration material, string fileName)
        {
            foreach (var line in File.ReadAllLines(fileName))
            {
                var values = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length < 2) continue;

                var uniformName = values[0];
                if (uniformName == "newmtl" || uniformName.StartsWith("#")) continue;
                if (uniformName.StartsWith("map_"))
                {
                    var textureBinding = new TextureBindingConfiguration();
                    textureBinding.Name = uniformName;
                    textureBinding.TextureName = Path.GetFileNameWithoutExtension(values[1]);
                    textureBinding.TextureSlot = TextureUnit.Texture0 + material.BufferBindings.Count;
                    material.BufferBindings.Add(textureBinding);
                }
                else
                {
                    UniformConfiguration uniform;
                    var uniformValues = ParseValues(values);
                    switch (uniformValues.Length)
                    {
                        case 1: uniform = new FloatUniform { Value = uniformValues[0] }; break;
                        case 2: uniform = new Vec2Uniform { Value = new Vector2(uniformValues[0], uniformValues[1]) }; break;
                        case 3: uniform = new Vec3Uniform { Value = new Vector3(uniformValues[0], uniformValues[1], uniformValues[2]) }; break;
                        case 4: uniform = new Vec4Uniform { Value = new Vector4(uniformValues[0], uniformValues[1], uniformValues[2], uniformValues[3]) }; break;
                        default: throw new InvalidOperationException("Unsupported number of values in the configuration of uniform " + uniformName + ".");
                    }
                    uniform.Name = uniformName;
                    material.ShaderUniforms.Add(uniform);
                }
            }
        }
    }
}
