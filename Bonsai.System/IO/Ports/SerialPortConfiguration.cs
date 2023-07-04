using System.ComponentModel;
using System.IO.Ports;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents the set of configuration parameters used to create a new serial port connection.
    /// </summary>
    public class SerialPortConfiguration
    {
        internal static readonly SerialPortConfiguration Default = new SerialPortConfiguration();

        /// <summary>
        /// Gets or sets the name of the serial port.
        /// </summary>
        [TypeConverter(typeof(SerialPortNameConverter))]
        public string PortName { get; set; }

        /// <summary>
        /// Gets or sets the serial baud rate.
        /// </summary>
        [TypeConverter(typeof(BaudRateConverter))]
        public int BaudRate { get; set; } = 9600;

        /// <summary>
        /// Gets or sets the byte encoding used for pre- and post-transmission conversion of text.
        /// </summary>
        [TypeConverter(typeof(SerialPortEncodingConverter))]
        public string Encoding { get; set; }

        /// <summary>
        /// Gets or sets the new line separator used to delimit reads from the serial port.
        /// </summary>
        public string NewLine { get; set; } = SerialPortManager.DefaultNewLine;

        /// <summary>
        /// Gets or sets the parity bit for the <see cref="SerialPort"/> object.
        /// </summary>
        public Parity Parity { get; set; }

        /// <summary>
        /// Gets or sets the byte that replaces invalid bytes in the data stream when a parity error occurs.
        /// </summary>
        public byte ParityReplace { get; set; } = 63;

        /// <summary>
        /// Gets or sets the number of data bits per byte.
        /// </summary>
        public int DataBits { get; set; } = 8;

        /// <summary>
        /// Gets or sets the number of stop bits per byte.
        /// </summary>
        public StopBits StopBits { get; set; } = StopBits.One;

        /// <summary>
        /// Gets or sets the handshaking protocol for serial port transmission of data.
        /// </summary>
        public Handshake Handshake { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether null bytes are ignored when transmitted
        /// between the port and the receive buffer.
        /// </summary>
        public bool DiscardNull { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Data Terminal Ready (DTR) signal should
        /// be enabled during serial communication.
        /// </summary>
        public bool DtrEnable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Request to Send (RTS) signal should be
        /// enabled during serial communication.
        /// </summary>
        public bool RtsEnable { get; set; }

        /// <summary>
        /// Gets or sets the size of the read buffer, in bytes. This is the maximum number of
        /// read bytes which can be buffered.
        /// </summary>
        public int ReadBufferSize { get; set; } = 4096;

        /// <summary>
        /// Gets or sets the size of the write buffer, in bytes. This is the maximum number of
        /// bytes which can be queued for write.
        /// </summary>
        public int WriteBufferSize { get; set; } = 2048;

        /// <summary>
        /// Gets or sets the number of bytes received into the internal input buffer before
        /// the read event is fired.
        /// </summary>
        public int ReceivedBytesThreshold { get; set; } = 1;
    }
}
