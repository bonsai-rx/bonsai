﻿using OpenTK;
using OpenTK.Graphics;
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
            return Observable.Create<ShaderWindow>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    GraphicsContext.ShareContexts = false;
                    var configuration = LoadConfiguration();
                    using (var window = new ShaderWindow(configuration))
                    using (var notification = cancellationToken.Register(window.Close))
                    {
                        observer.OnNext(window);
                        window.Run();
                        observer.OnCompleted();
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            })
            .ReplayReconnectable()
            .RefCount();
        }

        public static IObservable<ShaderWindow> WindowSource
        {
            get { return windowSource; }
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

        public static ShaderWindowSettings LoadConfiguration()
        {
            if (!File.Exists(DefaultConfigurationFile))
            {
                return new ShaderWindowSettings();
            }

            var serializer = new XmlSerializer(typeof(ShaderWindowSettings));
            using (var reader = XmlReader.Create(DefaultConfigurationFile))
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
