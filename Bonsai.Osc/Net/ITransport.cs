using Bonsai.Osc.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Osc.Net
{
    interface ITransport : IDisposable
    {
        IObservable<Message> MessageReceived { get; }

        void SendPacket(Action<BigEndianWriter> writePacket);
    }
}
