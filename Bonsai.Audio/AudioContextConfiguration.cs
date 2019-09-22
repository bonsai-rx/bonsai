using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    public class AudioContextConfiguration
    {
        public string DeviceName { get; set; }

        public int SampleRate { get; set; }

        public int Refresh { get; set; }
    }
}
