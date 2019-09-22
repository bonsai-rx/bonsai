using OpenTK.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Audio
{
    public static class AudioManager
    {
        public const string DefaultConfigurationFile = "Audio.config";
        static readonly Dictionary<string, Tuple<AudioContext, RefCountDisposable>> activeContexts = new Dictionary<string, Tuple<AudioContext, RefCountDisposable>>();
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

            Tuple<AudioContext, RefCountDisposable> activeContext;
            lock (activeContextLock)
            {
                if (!activeContexts.TryGetValue(deviceName, out activeContext))
                {
                    AudioContext context;
                    var configuration = LoadConfiguration();
                    if (configuration.Contains(deviceName))
                    {
                        var contextConfiguration = configuration[deviceName];
                        context = new AudioContext(
                            deviceName,
                            contextConfiguration.SampleRate,
                            contextConfiguration.Refresh);
                    }
                    else context = new AudioContext(deviceName, sampleRate, refresh);
                    
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

        public static AudioContextConfigurationCollection LoadConfiguration()
        {
            if (!File.Exists(DefaultConfigurationFile))
            {
                return new AudioContextConfigurationCollection();
            }

            var serializer = new XmlSerializer(typeof(AudioContextConfigurationCollection));
            using (var reader = XmlReader.Create(DefaultConfigurationFile))
            {
                return (AudioContextConfigurationCollection)serializer.Deserialize(reader);
            }
        }

        public static void SaveConfiguration(AudioContextConfigurationCollection configuration)
        {
            var serializer = new XmlSerializer(typeof(AudioContextConfigurationCollection));
            using (var writer = XmlWriter.Create(DefaultConfigurationFile, new XmlWriterSettings { Indent = true }))
            {
                serializer.Serialize(writer, configuration);
            }
        }
    }
}
