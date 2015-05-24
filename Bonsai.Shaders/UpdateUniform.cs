using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Sink)]
    public class UpdateUniform
    {
        public string UniformName { get; set; }

        [Editor("Bonsai.Shaders.Design.ShaderConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string ShaderName { get; set; }

        IObservable<TSource> Process<TSource>(IObservable<TSource> source, Action<int, TSource> update)
        {
            return ShaderManager.ReserveShader(ShaderName).Publish(ps =>
            {
                var location = -1;
                var name = UniformName;
                if (string.IsNullOrEmpty(name))
                {
                    throw new InvalidOperationException("A uniform variable name must be specified.");
                }

                return source.CombineLatest(ps,
                    (input, shader) =>
                    {
                        shader.Update(() =>
                        {
                            if (location < 0)
                            {
                                location = GL.GetUniformLocation(shader.Program, name);
                                if (location < 0)
                                {
                                    throw new InvalidOperationException(string.Format(
                                        "The uniform variable \"{0}\" was not found in shader program \"{1}\".",
                                        name,
                                        ShaderName));
                                }
                            }

                            update(location, input);
                        });
                        return input;
                    }).TakeUntil(ps.LastAsync());
            });
        }

        public IObservable<int> Process(IObservable<int> source)
        {
            return Process(source, (location, input) => GL.Uniform1(location, input));
        }

        public IObservable<float> Process(IObservable<float> source)
        {
            return Process(source, (location, input) => GL.Uniform1(location, input));
        }

        public IObservable<double> Process(IObservable<double> source)
        {
            return Process(source, (location, input) => GL.Uniform1(location, input));
        }

        public IObservable<Tuple<int, int>> Process(IObservable<Tuple<int, int>> source)
        {
            return Process(source, (location, input) => GL.Uniform2(location, input.Item1, input.Item2));
        }

        public IObservable<Tuple<float, float>> Process(IObservable<Tuple<float, float>> source)
        {
            return Process(source, (location, input) => GL.Uniform2(location, input.Item1, input.Item2));
        }

        public IObservable<Tuple<double, double>> Process(IObservable<Tuple<double, double>> source)
        {
            return Process(source, (location, input) => GL.Uniform2(location, input.Item1, input.Item2));
        }
    }
}
