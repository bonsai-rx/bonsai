using Bonsai.Resources;
using OpenTK.Audio;
using System;

namespace Bonsai.Audio
{
    public class AudioContextManager : IDisposable
    {
        readonly AudioContext context;
        readonly ResourceManager resourceManager;

        internal AudioContextManager(string deviceName, int sampleRate, int refresh)
        {
            context = new AudioContext(deviceName, sampleRate, refresh);
            resourceManager = new ResourceManager();
        }

        public AudioContext AudioContext
        {
            get { return context; }
        }

        public ResourceManager ResourceManager
        {
            get { return resourceManager; }
        }

        public void Dispose()
        {
            resourceManager.Dispose();
            context.Dispose();
        }
    }
}
