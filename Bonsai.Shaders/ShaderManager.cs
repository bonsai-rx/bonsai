using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    public static class ShaderManager
    {
        public const string DefaultConfigurationFile = "Shaders.config";
        static readonly IObservable<ShaderWindow> windowSource = CreateWindow();

        static IObservable<ShaderWindow> CreateWindow()
        {
            return ObservableCombinators.Multicast(
                Observable.Create<ShaderWindow>((observer, cancellationToken) =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        var configuration = LoadConfiguration();
                        using (var window = new ShaderWindow(configuration))
                        using (var notification = cancellationToken.Register(window.Close))
                        {
                            observer.OnNext(window);
                            window.Run();
                        }
                    },
                    cancellationToken,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
                }),
                () => new ReplaySubject<ShaderWindow>(1))
                .RefCount();
        }

        public static IObservable<Shader> ReserveShader(string shaderName)
        {
            if (string.IsNullOrEmpty(shaderName))
            {
                throw new ArgumentException("A shader name must be specified.", "shaderName");
            }

            return windowSource.Select(window =>
            {
                var shader = window.Shaders.FirstOrDefault(s => s.Name == shaderName);
                if (shader == null)
                {
                    throw new ArgumentException("No matching shader configuration was found.", "shaderName");
                }
                return shader;
            });
        }

        public static ShaderConfigurationCollection LoadConfiguration()
        {
            if (!File.Exists(DefaultConfigurationFile))
            {
                return new ShaderConfigurationCollection();
            }

            var serializer = new XmlSerializer(typeof(ShaderConfigurationCollection));
            using (var reader = XmlReader.Create(DefaultConfigurationFile))
            {
                return (ShaderConfigurationCollection)serializer.Deserialize(reader);
            }
        }

        public static void SaveConfiguration(ShaderConfigurationCollection configuration)
        {
            var serializer = new XmlSerializer(typeof(ShaderConfigurationCollection));
            using (var writer = XmlWriter.Create(DefaultConfigurationFile, new XmlWriterSettings { Indent = true }))
            {
                serializer.Serialize(writer, configuration);
            }
        }
    }
}
