using Bonsai.Shaders.Configuration;
using OpenTK.Graphics;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    public static class ShaderManager
    {
        public const string DefaultConfigurationFile = "Shaders.config";
        static readonly IObservable<ShaderWindow> windowSource = CreateWindow();

        internal static IObservable<ShaderWindow> CreateWindow(ShaderWindowSettings configuration)
        {
            return Observable.Create<ShaderWindow>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    GraphicsContext.ShareContexts = false;
                    using (var window = new ShaderWindow(configuration))
                    using (var notification = cancellationToken.Register(window.Close))
                    using (var disposable = SubjectManager.ReserveSubject())
                    {
                        var subject = disposable.Subject;
                        window.Load += delegate
                        {
                            observer.OnNext(window);
                            subject.OnNext(window);
                        };

                        window.Unload += delegate
                        {
                            subject.OnCompleted();
                            observer.OnCompleted();
                        };

                        try { window.Run(); }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                            subject.OnError(ex);
                        }
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            });
        }

        static IObservable<ShaderWindow> CreateWindow()
        {
            return Observable.Defer(() =>
            {
                if (File.Exists(DefaultConfigurationFile))
                {
                    var configuration = LoadConfiguration();
                    return CreateWindow(configuration);
                }
                else
                {
                    var disposable = SubjectManager.ReserveSubject();
                    return disposable.Subject.Finally(disposable.Dispose);
                }
            })
            .ReplayReconnectable()
            .RefCount();
        }

        public static IObservable<ShaderWindow> WindowSource
        {
            get { return windowSource; }
        }

        public static IObservable<ShaderWindow> WindowUpdate(Action<ShaderWindow> update)
        {
            return ShaderManager.WindowSource.SelectMany(window => window.UpdateFrameAsync.Take(1)).Select(evt =>
            {
                var window = (ShaderWindow)evt.Sender;
                update(window);
                return window;
            });
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

        public static IObservable<Material> ReserveMaterial(string shaderName)
        {
            if (string.IsNullOrEmpty(shaderName))
            {
                throw new ArgumentException("A material name must be specified.", "shaderName");
            }

            return windowSource.Select(window =>
            {
                var material = window.Shaders.Select(shader => shader as Material)
                                             .FirstOrDefault(m => m != null && m.Name == shaderName);
                if (material == null)
                {
                    throw new ArgumentException("No matching material configuration was found.", "shaderName");
                }
                return material;
            });
        }

        public static IObservable<ComputeProgram> ReserveComputeProgram(string shaderName)
        {
            if (string.IsNullOrEmpty(shaderName))
            {
                throw new ArgumentException("A compute shader name must be specified.", "shaderName");
            }

            return windowSource.Select(window =>
            {
                var computeProgram = window.Shaders.Select(shader => shader as ComputeProgram)
                                                   .FirstOrDefault(m => m != null && m.Name == shaderName);
                if (computeProgram == null)
                {
                    throw new ArgumentException("No matching compute program configuration was found.", "shaderName");
                }
                return computeProgram;
            });
        }

        public static ShaderWindowSettings LoadConfiguration()
        {
            if (!File.Exists(DefaultConfigurationFile))
            {
                return new ShaderWindowSettings();
            }

            var nameTable = new NameTable();
            var namespaceManager = new XmlNamespaceManager(nameTable);
            namespaceManager.AddNamespace(string.Empty, Constants.XmlNamespace);
            var context = new XmlParserContext(nameTable, namespaceManager, null, XmlSpace.None);

            var serializer = new XmlSerializer(typeof(ShaderWindowSettings));
            using (var reader = XmlReader.Create(DefaultConfigurationFile, null, context))
            {
                return (ShaderWindowSettings)serializer.Deserialize(reader);
            }
        }

        public static void SaveConfiguration(ShaderWindowSettings configuration)
        {
            var serializer = new XmlSerializer(typeof(ShaderWindowSettings));
            using (var writer = XmlWriter.Create(DefaultConfigurationFile, new XmlWriterSettings { Indent = true }))
            {
                serializer.Serialize(writer, configuration);
            }
        }
    }
}
