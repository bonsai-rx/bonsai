using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.IO.Ports
{
    /// <summary>
    /// Represents an operator that creates and configures a connection to a system serial port.
    /// </summary>
    [DefaultProperty(nameof(Name))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Creates and configures a connection to a system serial port.")]
    public class CreateSerialPort : Source<SerialPort>, INamedElement
    {
        readonly SerialPortConfiguration configuration = new SerialPortConfiguration();

        /// <summary>
        /// Gets or sets the optional alias for the serial port connection.
        /// </summary>
        [Category("Connection")]
        [Description("The optional alias for the serial port connection.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the serial port.
        /// </summary>
        [Category("Connection")]
        [TypeConverter(typeof(SerialPortNameConverter))]
        [Description("The name of the serial port.")]
        public string PortName
        {
            get { return configuration.PortName; }
            set { configuration.PortName = value; }
        }

        /// <summary>
        /// Gets or sets the serial baud rate.
        /// </summary>
        [Category("Connection")]
        [TypeConverter(typeof(BaudRateConverter))]
        [Description("The serial baud rate.")]
        public int BaudRate
        {
            get { return configuration.BaudRate; }
            set { configuration.BaudRate = value; }
        }

        /// <summary>
        /// Gets or sets the byte encoding used for pre- and post-transmission conversion of text.
        /// </summary>
        [TypeConverter(typeof(SerialPortEncodingConverter))]
        [Description("The byte encoding used for pre- and post-transmission conversion of text.")]
        public string Encoding
        {
            get { return configuration.Encoding; }
            set { configuration.Encoding = value; }
        }

        /// <summary>
        /// Gets or sets the new line separator used to delimit reads from the serial port.
        /// </summary>
        [Description("The new line separator used to delimit reads from the serial port.")]
        public string NewLine
        {
            get { return configuration.NewLine; }
            set { configuration.NewLine = value; }
        }

        /// <summary>
        /// Gets or sets the parity bit for the <see cref="SerialPort"/> object.
        /// </summary>
        [Description("The parity checking protocol.")]
        public Parity Parity
        {
            get { return configuration.Parity; }
            set { configuration.Parity = value; }
        }

        /// <summary>
        /// Gets or sets the byte that replaces invalid bytes in the data stream when a parity error occurs.
        /// </summary>
        [Description("The byte that replaces invalid bytes in the data stream when a parity error occurs.")]
        public byte ParityReplace
        {
            get { return configuration.ParityReplace; }
            set { configuration.ParityReplace = value; }
        }

        /// <summary>
        /// Gets or sets the number of data bits per byte.
        /// </summary>
        [Description("The number of data bits per byte.")]
        public int DataBits
        {
            get { return configuration.DataBits; }
            set { configuration.DataBits = value; }
        }

        /// <summary>
        /// Gets or sets the number of stop bits per byte.
        /// </summary>
        [Description("The number of stop bits per byte.")]
        public StopBits StopBits
        {
            get { return configuration.StopBits; }
            set { configuration.StopBits = value; }
        }

        /// <summary>
        /// Gets or sets the handshaking protocol for serial port transmission of data.
        /// </summary>
        [Description("The handshaking protocol for serial port transmission of data.")]
        public Handshake Handshake
        {
            get { return configuration.Handshake; }
            set { configuration.Handshake = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether null bytes are ignored when transmitted
        /// between the port and the receive buffer.
        /// </summary>
        [Description("Indicates whether null bytes are ignored when transmitted between the port and the receive buffer.")]
        public bool DiscardNull
        {
            get { return configuration.DiscardNull; }
            set { configuration.DiscardNull = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Data Terminal Ready (DTR) signal should
        /// be enabled during serial communication.
        /// </summary>
        [Description("Indicates whether the Data Terminal Ready (DTR) signal should be enabled during serial communication.")]
        public bool DtrEnable
        {
            get { return configuration.DtrEnable; }
            set { configuration.DtrEnable = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Request to Send (RTS) signal should be
        /// enabled during serial communication.
        /// </summary>
        [Description("Indicates whether the Request to Send (RTS) signal should be enabled during serial communication.")]
        public bool RtsEnable
        {
            get { return configuration.RtsEnable; }
            set { configuration.RtsEnable = value; }
        }

        /// <summary>
        /// Gets or sets the size of the read buffer, in bytes. This is the maximum number of
        /// read bytes which can be buffered.
        /// </summary>
        [Description("The size of the read buffer, in bytes. This is the maximum number of read bytes which can be buffered.")]
        public int ReadBufferSize
        {
            get { return configuration.ReadBufferSize; }
            set { configuration.ReadBufferSize = value; }
        }

        /// <summary>
        /// Gets or sets the size of the write buffer, in bytes. This is the maximum number of
        /// bytes which can be queued for write.
        /// </summary>
        [Description("The size of the write buffer, in bytes. This is the maximum number of bytes which can be queued for write.")]
        public int WriteBufferSize
        {
            get { return configuration.WriteBufferSize; }
            set { configuration.WriteBufferSize = value; }
        }

        /// <summary>
        /// Gets or sets the number of bytes received into the internal input buffer before
        /// the read event is fired.
        /// </summary>
        [Description("The number of bytes received into the internal input buffer before the read event is fired.")]
        public int ReceivedBytesThreshold
        {
            get { return configuration.ReceivedBytesThreshold; }
            set { configuration.ReceivedBytesThreshold = value; }
        }

        /// <summary>
        /// Generates an observable sequence that contains the serial port connection object.
        /// </summary>
        /// <returns>
        /// A sequence containing a single instance of the <see cref="SerialPort"/> class
        /// representing the serial connection.
        /// </returns>
        public override IObservable<SerialPort> Generate()
        {
            return Observable.Using(
                () => SerialPortManager.ReserveConnection(Name, configuration),
                connection => Observable.Return(connection.SerialPort).Concat(Observable.Never(connection.SerialPort)));
        }
    }
}
