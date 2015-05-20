using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
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
        static readonly Dictionary<string, Tuple<Shader, RefCountDisposable>> activeShaders = new Dictionary<string, Tuple<Shader, RefCountDisposable>>();
        static readonly object activeShadersLock = new object();
        static CancellationTokenSource windowTokenSource;
        static ShaderWindow window;

        public static ShaderDisposable ReserveShader(string shaderName)
        {
            if (string.IsNullOrEmpty(shaderName))
            {
                throw new ArgumentException("A shader name must be specified.", "shaderName");
            }

            Tuple<Shader, RefCountDisposable> activeShader;
            lock (activeShadersLock)
            {
                if (!activeShaders.TryGetValue(shaderName, out activeShader))
                {
                    Shader shader;
                    var configuration = LoadConfiguration();
                    if (!configuration.Contains(shaderName))
                    {
                        throw new ArgumentException("No matching shader configuration was found.", "shaderName");
                    }

                    if (window == null)
                    {
                        var waitEvent = new ManualResetEvent(false);
                        windowTokenSource = new CancellationTokenSource();
                        Task.Factory.StartNew(() =>
                        {
                            using (window = new ShaderWindow())
                            using (var notification = windowTokenSource.Token.Register(window.Close))
                            {
                                waitEvent.Set();
                                window.Run();
                            }
                        },
                        windowTokenSource.Token,
                        TaskCreationOptions.LongRunning,
                        TaskScheduler.Default);
                        waitEvent.WaitOne();
                    }

                    var shaderConfiguration = configuration[shaderName];
                    shader = new Shader(window, shaderConfiguration.VertexShader, shaderConfiguration.FragmentShader);
                    shader.Visible = shaderConfiguration.Visible;
                    shader.Update(() =>
                    {
                        GL.BindTexture(TextureTarget.Texture2D, shader.Texture);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                        GL.BindTexture(TextureTarget.Texture2D, 0);
                    });

                    var dispose = Disposable.Create(() =>
                    {
                        shader.Dispose();
                        lock (activeShadersLock)
                        {
                            activeShaders.Remove(shaderName);
                            if (activeShaders.Count == 0)
                            {
                                windowTokenSource.Cancel();
                                window = null;
                            }
                        }
                    });

                    var refCount = new RefCountDisposable(dispose);
                    activeShader = Tuple.Create(shader, refCount);
                    activeShaders.Add(shaderName, activeShader);
                    return new ShaderDisposable(shader, refCount);
                }
            }

            return new ShaderDisposable(activeShader.Item1, activeShader.Item2.GetDisposable());
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
