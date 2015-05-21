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
    public class UpdateUniform
    {
        [Editor("Bonsai.Shaders.Design.ShaderConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string ShaderName { get; set; }

        public string Name { get; set; }

        IObservable<TSource> Process<TSource>(IObservable<TSource> source, Action<int, TSource> update)
        {
            return Observable.Create<TSource>(observer =>
            {
                int location = 0;
                var resource = ShaderManager.ReserveShader(ShaderName);
                resource.Shader.Subscribe(observer);
                resource.Shader.Update(() =>
                {
                    var name = Name;
                    if (string.IsNullOrEmpty(name))
                    {
                        throw new InvalidOperationException("A uniform variable name must be specified.");
                    }

                    location = GL.GetUniformLocation(resource.Shader.Program, Name);
                    if (location < 0)
                    {
                        throw new InvalidOperationException(string.Format(
                            "The uniform variable \"{0}\" was not found in shader program \"{1}\".",
                            Name,
                            ShaderName));
                    }
                });

                return source.Do(input =>
                {
                    resource.Shader.Update(() =>
                    {
                        update(location, input);
                    });
                })
                .Finally(resource.Dispose)
                .SubscribeSafe(observer);
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
