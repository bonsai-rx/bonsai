using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp.Design
{
    class ReplayRandom : Random
    {
        int current;
        ReplayMode mode;
        List<int> replayBuffer = new List<int>();

        public ReplayMode Mode
        {
            get { return mode; }
            set
            {
                current = 0;
                mode = value;
                if (mode == ReplayMode.None)
                {
                    replayBuffer.Clear();
                }
            }
        }

        public override int Next(int minValue, int maxValue)
        {
            if (mode == ReplayMode.Replaying)
            {
                var number = replayBuffer[current];
                current = (current + 1) % replayBuffer.Count;
                return number;
            }
            else
            {
                var number = base.Next(minValue, maxValue);
                if (mode == ReplayMode.Recording) replayBuffer.Add(number);
                return number;
            }
        }
    }

    enum ReplayMode
    {
        None,
        Recording,
        Replaying
    }
}
