using Bonsai.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    public class AudioContext : IDisposable
    {
        readonly OpenTK.Audio.AudioContext context;
        readonly ResourceManager resourceManager;

        internal AudioContext(string deviceName, int sampleRate, int refresh)
        {
            context = new OpenTK.Audio.AudioContext(deviceName, sampleRate, refresh);
            resourceManager = new ResourceManager();
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
