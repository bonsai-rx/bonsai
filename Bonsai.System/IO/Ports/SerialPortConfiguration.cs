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
        [Description("The name of the serial port.")]
        public string PortName { get; set; }

        [TypeConverter(typeof(BaudRateConverter))]
        [Description("The baud rate used by the serial port.")]
        public int BaudRate { get; set; }

        [TypeConverter(typeof(SerialPortEncodingConverter))]
        public string Encoding { get; set; }

        public Parity Parity { get; set; }

        [Description("The byte with which to replace bytes received with parity errors.")]
        public byte ParityReplace { get; set; }

        [Description("The standard number of data bits per byte.")]
        public int DataBits { get; set; }

        [Description("The standard number of stop bits per byte.")]
        public StopBits StopBits { get; set; }

        [Description("The handshaking protocol for flow control in data exchange, which can be None.")]
        public Handshake Handshake { get; set; }

        [Description("A flag indicating whether to discard null bytes received on the port before adding to serial buffer.")]
        public bool DiscardNull { get; set; }

        [Description("A flag indicating whether the Data Terminal Ready (DTR) signal should be enabled.")]
        public bool DtrEnable { get; set; }

        [Description("A flag indicating whether the Request to Send (RTS) signal should be enabled.")]
        public bool RtsEnable { get; set; }

        [Description("The size of the read buffer, in bytes. This is the maximum number of read bytes which can be buffered.")]
        public int ReadBufferSize { get; set; }

        [Description("The size of the write buffer, in bytes. This is the maximum number of bytes which can be queued for write.")]
        public int WriteBufferSize { get; set; }

        [Description("The number of bytes required to be available before the read event is fired.")]
        public int ReceivedBytesThreshold { get; set; }
    }
}
