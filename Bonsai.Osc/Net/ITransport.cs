using Bonsai.Osc.IO;
using System;

namespace Bonsai.Osc.Net
{
    interface ITransport : IDisposable
    {
        IObservable<Message> MessageReceived { get; }

        void SendPacket(Action<BigEndianWriter> writePacket);
    }
}
