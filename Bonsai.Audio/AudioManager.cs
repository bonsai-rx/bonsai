using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Audio
{
    internal static class AudioManager
    {
        public const string DefaultConfigurationFile = "Audio.config";
        static readonly Dictionary<string, Tuple<AudioContextManager, RefCountDisposable>> activeContexts = new Dictionary<string, Tuple<AudioContextManager, RefCountDisposable>>();
        static readonly object activeContextLock = new object();

        public static AudioContextDisposable ReserveContext(string deviceName)
        {
            return ReserveContext(deviceName, 0, 0);
        }

        internal static AudioContextDisposable ReserveContext(string deviceName, int sampleRate, int refresh)
        {
            if (string.IsNullOrEmpty(deviceName))
            {
                var currentContext = OpenTK.Audio.AudioContext.CurrentContext;
                if (currentContext != null) deviceName = currentContext.CurrentDevice;
                else deviceName = OpenTK.Audio.AudioContext.DefaultDevice;
            }

            Tuple<AudioContextManager, RefCountDisposable> activeContext;
            lock (activeContextLock)
            {
                if (!activeContexts.TryGetValue(deviceName, out activeContext))
                {
                    AudioContextManager context;
#pragma warning disable CS0612 // Type or member is obsolete
                    var configuration = LoadConfiguration();
                    if (configuration.Contains(deviceName))
                    {
                        var contextConfiguration = configuration[deviceName];
                        context = new AudioContextManager(
                            deviceName,
                            contextConfiguration.SampleRate,
                            contextConfiguration.Refresh);
                    }
#pragma warning restore CS0612 // Type or member is obsolete
                    else context = new AudioContextManager(deviceName, sampleRate, refresh);
                    
                    var dispose = Disposable.Create(() =>
                    {
                        context.Dispose();
                        activeContexts.Remove(deviceName);
                    });

                    var refCount = new RefCountDisposable(dispose);
                    activeContext = Tuple.Create(context, refCount);
                    activeContexts.Add(deviceName, activeContext);
                    return new AudioContextDisposable(context, refCount);
                }
            }

            return new AudioContextDisposable(activeContext.Item1, activeContext.Item2.GetDisposable());
        }

        [Obsolete]
        public static AudioContextConfigurationCollection LoadConfiguration()
        {
            if (!File.Exists(DefaultConfigurationFile))
            {
                return new AudioContextConfigurationCollection();
            }

            var serializer = new XmlSerializer(typeof(AudioContextConfigurationCollection));
            using var reader = XmlReader.Create(DefaultConfigurationFile);
            return (AudioContextConfigurationCollection)serializer.Deserialize(reader);
        }

        [Obsolete]
        public static void SaveConfiguration(AudioContextConfigurationCollection configuration)
        {
            var serializer = new XmlSerializer(typeof(AudioContextConfigurationCollection));
            using var writer = XmlWriter.Create(DefaultConfigurationFile, new XmlWriterSettings { Indent = true });
            serializer.Serialize(writer, configuration);
        }
    }
}
