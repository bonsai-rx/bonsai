using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO.Ports;

namespace Bonsai.IO
{
    public class SerialPortConfiguration
    {
        internal static readonly SerialPortConfiguration Default = new SerialPortConfiguration();

        public SerialPortConfiguration()
        {
            BaudRate = 9600;
            ParityReplace = 63;
            DataBits = 8;
            StopBits = StopBits.One;
            ReadBufferSize = 4096;
            WriteBufferSize = 2048;
            ReceivedBytesThreshold = 1;
        }

        [TypeConverter(typeof(SerialPortNameConverter))]
        public string PortName { get; set; }

        [TypeConverter(typeof(BaudRateConverter))]
        public int BaudRate { get; set; }

        [TypeConverter(typeof(SerialPortEncodingConverter))]
        public string Encoding { get; set; }

        public Parity Parity { get; set; }

        public byte ParityReplace { get; set; }

        public int DataBits { get; set; }

        public StopBits StopBits { get; set; }

        public Handshake Handshake { get; set; }

        public bool DiscardNull { get; set; }

        public bool DtrEnable { get; set; }

        public bool RtsEnable { get; set; }

        public int ReadBufferSize { get; set; }

        public int WriteBufferSize { get; set; }

        public int ReceivedBytesThreshold { get; set; }
    }
}
