using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that updates the value of a uniform variable on the
    /// specified shader.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Sink)]
    [Description("Updates the value of a uniform variable on the specified shader.")]
    public class UpdateUniform
    {
        /// <summary>
        /// Gets or sets the name of the uniform variable to update.
        /// </summary>
        [Description("The name of the uniform variable to update.")]
        public string UniformName { get; set; }

        /// <summary>
        /// Gets or sets the name of the shader program.
        /// </summary>
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

                            GL.GetUniformIndices(shader.Program, 1, new[] { name }, out int uniformIndex);
                            if (uniformIndex >= 0)
                            {
                                GL.GetActiveUniform(shader.Program, uniformIndex, out int uniformSize, out ActiveUniformType uniformType);
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

        /// <summary>
        /// Updates a sampler 2D uniform variable on the specified shader with
        /// the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<TextureUnit> Process(IObservable<TextureUnit> source)
        {
            return Process(source, (location, input) => GL.Uniform1(location, input - TextureUnit.Texture0),
                           ActiveUniformType.Sampler2D);
        }

        /// <summary>
        /// Updates a 32-bit integer uniform variable on the specified shader with
        /// the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<int> Process(IObservable<int> source)
        {
            return Process(source, (location, input) => GL.Uniform1(location, input),
                           ActiveUniformType.Int);
        }

        /// <summary>
        /// Updates a single-precision floating-point uniform variable on the
        /// specified shader with the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<float> Process(IObservable<float> source)
        {
            return Process(source, (location, input) => GL.Uniform1(location, input),
                           ActiveUniformType.Float);
        }

        /// <summary>
        /// Updates a double-precision floating-point uniform variable on the
        /// specified shader with the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<double> Process(IObservable<double> source)
        {
            return Process(source, (location, input) => GL.Uniform1(location, input),
                           ActiveUniformType.Double);
        }

        /// <summary>
        /// Updates an ivec2 uniform variable on the specified shader with
        /// the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<Tuple<int, int>> Process(IObservable<Tuple<int, int>> source)
        {
            return Process(source, (location, input) => GL.Uniform2(location, input.Item1, input.Item2),
                           ActiveUniformType.IntVec2);
        }

        /// <summary>
        /// Updates a vec2 uniform variable on the specified shader with
        /// the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<Tuple<float, float>> Process(IObservable<Tuple<float, float>> source)
        {
            return Process(source, (location, input) => GL.Uniform2(location, input.Item1, input.Item2),
                           ActiveUniformType.FloatVec2);
        }

        /// <summary>
        /// Updates a dvec2 uniform variable on the specified shader with
        /// the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<Tuple<double, double>> Process(IObservable<Tuple<double, double>> source)
        {
            return Process(source, (location, input) => GL.Uniform2(location, input.Item1, input.Item2),
                           ActiveUniformType.DoubleVec2);
        }

        /// <summary>
        /// Updates an ivec3 uniform variable on the specified shader with
        /// the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<Tuple<int, int, int>> Process(IObservable<Tuple<int, int, int>> source)
        {
            return Process(source, (location, input) => GL.Uniform3(location, input.Item1, input.Item2, input.Item3),
                           ActiveUniformType.IntVec3);
        }

        /// <summary>
        /// Updates a vec3 uniform variable on the specified shader with
        /// the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<Tuple<float, float, float>> Process(IObservable<Tuple<float, float, float>> source)
        {
            return Process(source, (location, input) => GL.Uniform3(location, input.Item1, input.Item2, input.Item3),
                           ActiveUniformType.FloatVec3);
        }

        /// <summary>
        /// Updates a dvec3 uniform variable on the specified shader with
        /// the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<Tuple<double, double, double>> Process(IObservable<Tuple<double, double, double>> source)
        {
            return Process(source, (location, input) => GL.Uniform3(location, input.Item1, input.Item2, input.Item3),
                           ActiveUniformType.DoubleVec3);
        }

        /// <summary>
        /// Updates an ivec4 uniform variable on the specified shader with
        /// the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<Tuple<int, int, int, int>> Process(IObservable<Tuple<int, int, int, int>> source)
        {
            return Process(source, (location, input) => GL.Uniform4(location, input.Item1, input.Item2, input.Item3, input.Item4),
                           ActiveUniformType.IntVec4);
        }

        /// <summary>
        /// Updates a vec4 uniform variable on the specified shader with
        /// the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<Tuple<float, float, float, float>> Process(IObservable<Tuple<float, float, float, float>> source)
        {
            return Process(source, (location, input) => GL.Uniform4(location, input.Item1, input.Item2, input.Item3, input.Item4),
                           ActiveUniformType.FloatVec4);
        }

        /// <summary>
        /// Updates a dvec4 uniform variable on the specified shader with
        /// the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<Tuple<double, double, double, double>> Process(IObservable<Tuple<double, double, double, double>> source)
        {
            return Process(source, (location, input) => GL.Uniform4(location, input.Item1, input.Item2, input.Item3, input.Item4),
                           ActiveUniformType.DoubleVec4);
        }

        /// <summary>
        /// Updates a vec2 uniform variable on the specified shader with
        /// the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<Vector2> Process(IObservable<Vector2> source)
        {
            return Process(source, (location, input) => GL.Uniform2(location, ref input),
                           ActiveUniformType.FloatVec2);
        }

        /// <summary>
        /// Updates a vec3 uniform variable on the specified shader with
        /// the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<Vector3> Process(IObservable<Vector3> source)
        {
            return Process(source, (location, input) => GL.Uniform3(location, ref input),
                           ActiveUniformType.FloatVec3);
        }

        /// <summary>
        /// Updates a vec4 uniform variable on the specified shader with
        /// the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<Vector4> Process(IObservable<Vector4> source)
        {
            return Process(source, (location, input) => GL.Uniform4(location, ref input),
                           ActiveUniformType.FloatVec4);
        }

        /// <summary>
        /// Updates a vec4 uniform variable on the specified shader with
        /// the quaternion values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of quaternion values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<Quaternion> Process(IObservable<Quaternion> source)
        {
            return Process(source, (location, input) => GL.Uniform4(location, input),
                           ActiveUniformType.FloatVec4);
        }

        /// <summary>
        /// Updates a mat2 uniform variable on the specified shader with
        /// the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<Matrix2> Process(IObservable<Matrix2> source)
        {
            return Process(source, (location, input) => GL.UniformMatrix2(location, false, ref input),
                           ActiveUniformType.FloatMat2);
        }

        /// <summary>
        /// Updates a mat3 uniform variable on the specified shader with
        /// the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<Matrix3> Process(IObservable<Matrix3> source)
        {
            return Process(source, (location, input) => GL.UniformMatrix3(location, false, ref input),
                           ActiveUniformType.FloatMat3);
        }

        /// <summary>
        /// Updates a mat4 uniform variable on the specified shader with
        /// the values from an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values used to update the uniform variable.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of assigning the
        /// uniform variable to the values of the sequence.
        /// </returns>
        public IObservable<Matrix4> Process(IObservable<Matrix4> source)
        {
            return Process(source, (location, input) => GL.UniformMatrix4(location, false, ref input),
                           ActiveUniformType.FloatMat4);
        }
    }
}
