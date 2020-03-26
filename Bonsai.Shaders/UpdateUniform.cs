using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Sink)]
    [Description("Updates an active uniform variable on the specified shader.")]
    public class UpdateUniform
    {
        [Description("The name of the uniform variable to update.")]
        public string UniformName { get; set; }

        [TypeConverter(typeof(ShaderNameConverter))]
        [Description("The name of the shader program.")]
        public string ShaderName { get; set; }

        IObservable<TSource> Process<TSource>(IObservable<TSource> source, Action<int, TSource> update, ActiveUniformType type)
        {
            return Observable.Create<TSource>(observer =>
            {
                var location = 0;
                var name = UniformName;
                if (string.IsNullOrEmpty(name))
                {
                    throw new InvalidOperationException("A uniform variable name must be specified.");
                }

                return source.CombineEither(
                    ShaderManager.ReserveShader(ShaderName).Do(shader =>
                    {
                        shader.Update(() =>
                        {
                            location = GL.GetUniformLocation(shader.Program, name);
                            if (location < 0)
                            {
                                observer.OnError(new InvalidOperationException(string.Format(
                                    "The uniform variable \"{0}\" was not found in shader \"{1}\".",
                                    name,
                                    ShaderName)));
                                return;
                            }

                            int uniformIndex;
                            GL.GetUniformIndices(shader.Program, 1, new[] { name }, out uniformIndex);
                            if (uniformIndex >= 0)
                            {
                                int uniformSize;
                                ActiveUniformType uniformType;
                                GL.GetActiveUniform(shader.Program, uniformIndex, out uniformSize, out uniformType);
                                if (uniformType != type)
                                {
                                    observer.OnError(new InvalidOperationException(string.Format(
                                        "Expected a {2} uniform, but the variable \"{0}\" in shader \"{1}\" has type {3}.",
                                        name,
                                        ShaderName,
                                        type,
                                        uniformType)));
                                    return;
                                }
                            }
                        });
                    }),
                    (input, shader) =>
                    {
                        shader.Update(() => update(location, input));
                        return input;
                    }).SubscribeSafe(observer);
            });
        }

        public IObservable<TextureUnit> Process(IObservable<TextureUnit> source)
        {
            return Process(source, (location, input) => GL.Uniform1(location, (int)(input - TextureUnit.Texture0)),
                           ActiveUniformType.Sampler2D);
        }

        public IObservable<int> Process(IObservable<int> source)
        {
            return Process(source, (location, input) => GL.Uniform1(location, input),
                           ActiveUniformType.Int);
        }

        public IObservable<float> Process(IObservable<float> source)
        {
            return Process(source, (location, input) => GL.Uniform1(location, input),
                           ActiveUniformType.Float);
        }

        public IObservable<double> Process(IObservable<double> source)
        {
            return Process(source, (location, input) => GL.Uniform1(location, input),
                           ActiveUniformType.Double);
        }

        public IObservable<Tuple<int, int>> Process(IObservable<Tuple<int, int>> source)
        {
            return Process(source, (location, input) => GL.Uniform2(location, input.Item1, input.Item2),
                           ActiveUniformType.IntVec2);
        }

        public IObservable<Tuple<float, float>> Process(IObservable<Tuple<float, float>> source)
        {
            return Process(source, (location, input) => GL.Uniform2(location, input.Item1, input.Item2),
                           ActiveUniformType.FloatVec2);
        }

        public IObservable<Tuple<double, double>> Process(IObservable<Tuple<double, double>> source)
        {
            return Process(source, (location, input) => GL.Uniform2(location, input.Item1, input.Item2),
                           ActiveUniformType.DoubleVec2);
        }

        public IObservable<Tuple<int, int, int>> Process(IObservable<Tuple<int, int, int>> source)
        {
            return Process(source, (location, input) => GL.Uniform3(location, input.Item1, input.Item2, input.Item3),
                           ActiveUniformType.IntVec3);
        }

        public IObservable<Tuple<float, float, float>> Process(IObservable<Tuple<float, float, float>> source)
        {
            return Process(source, (location, input) => GL.Uniform3(location, input.Item1, input.Item2, input.Item3),
                           ActiveUniformType.FloatVec3);
        }

        public IObservable<Tuple<double, double, double>> Process(IObservable<Tuple<double, double, double>> source)
        {
            return Process(source, (location, input) => GL.Uniform3(location, input.Item1, input.Item2, input.Item3),
                           ActiveUniformType.DoubleVec3);
        }

        public IObservable<Tuple<int, int, int, int>> Process(IObservable<Tuple<int, int, int, int>> source)
        {
            return Process(source, (location, input) => GL.Uniform4(location, input.Item1, input.Item2, input.Item3, input.Item4),
                           ActiveUniformType.IntVec4);
        }

        public IObservable<Tuple<float, float, float, float>> Process(IObservable<Tuple<float, float, float, float>> source)
        {
            return Process(source, (location, input) => GL.Uniform4(location, input.Item1, input.Item2, input.Item3, input.Item4),
                           ActiveUniformType.FloatVec4);
        }

        public IObservable<Tuple<double, double, double, double>> Process(IObservable<Tuple<double, double, double, double>> source)
        {
            return Process(source, (location, input) => GL.Uniform4(location, input.Item1, input.Item2, input.Item3, input.Item4),
                           ActiveUniformType.DoubleVec4);
        }

        public IObservable<Vector2> Process(IObservable<Vector2> source)
        {
            return Process(source, (location, input) => GL.Uniform2(location, ref input),
                           ActiveUniformType.FloatVec2);
        }

        public IObservable<Vector3> Process(IObservable<Vector3> source)
        {
            return Process(source, (location, input) => GL.Uniform3(location, ref input),
                           ActiveUniformType.FloatVec3);
        }

        public IObservable<Vector4> Process(IObservable<Vector4> source)
        {
            return Process(source, (location, input) => GL.Uniform4(location, ref input),
                           ActiveUniformType.FloatVec4);
        }

        public IObservable<Quaternion> Process(IObservable<Quaternion> source)
        {
            return Process(source, (location, input) => GL.Uniform4(location, input),
                           ActiveUniformType.FloatVec4);
        }

        public IObservable<Matrix2> Process(IObservable<Matrix2> source)
        {
            return Process(source, (location, input) => GL.UniformMatrix2(location, false, ref input),
                           ActiveUniformType.FloatMat2);
        }

        public IObservable<Matrix3> Process(IObservable<Matrix3> source)
        {
            return Process(source, (location, input) => GL.UniformMatrix3(location, false, ref input),
                           ActiveUniformType.FloatMat3);
        }

        public IObservable<Matrix4> Process(IObservable<Matrix4> source)
        {
            return Process(source, (location, input) => GL.UniformMatrix4(location, false, ref input),
                           ActiveUniformType.FloatMat4);
        }
    }
}
