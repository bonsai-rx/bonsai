using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.IO
{
    [DefaultProperty("Name")]
    [Description("Creates and configures a connection to a system serial port.")]
    public class CreateSerialPort : Source<SerialPort>, INamedElement
    {
        readonly SerialPortConfiguration configuration = new SerialPortConfiguration();

        [Category("Connection")]
        [Description("The optional alias for the serial port connection.")]
        public string Name { get; set; }

        [Category("Connection")]
        [TypeConverter(typeof(SerialPortNameConverter))]
        [Description("The name of the serial port.")]
        public string PortName
        {
            get { return configuration.PortName; }
            set { configuration.PortName = value; }
        }

        [Category("Connection")]
        [TypeConverter(typeof(BaudRateConverter))]
        [Description("The baud rate used by the serial port.")]
        public int BaudRate
        {
            get { return configuration.BaudRate; }
            set { configuration.BaudRate = value; }
        }

        [TypeConverter(typeof(SerialPortEncodingConverter))]
        [Description("The byte encoding used for pre- and post-transmission conversion of text.")]
        public string Encoding
        {
            get { return configuration.Encoding; }
            set { configuration.Encoding = value; }
        }

        [Description("The parity checking protocol.")]
        public Parity Parity
        {
            get { return configuration.Parity; }
            set { configuration.Parity = value; }
        }

        [Description("The byte with which to replace bytes received with parity errors.")]
        public byte ParityReplace
        {
            get { return configuration.ParityReplace; }
            set { configuration.ParityReplace = value; }
        }

        [Description("The standard number of data bits per byte.")]
        public int DataBits
        {
            get { return configuration.DataBits; }
            set { configuration.DataBits = value; }
        }

        [Description("The standard number of stop bits per byte.")]
        public StopBits StopBits
        {
            get { return configuration.StopBits; }
            set { configuration.StopBits = value; }
        }

        [Description("The handshaking protocol for flow control in data exchange, which can be None.")]
        public Handshake Handshake
        {
            get { return configuration.Handshake; }
            set { configuration.Handshake = value; }
        }

        [Description("A flag indicating whether to discard null bytes received on the port before adding to serial buffer.")]
        public bool DiscardNull
        {
            get { return configuration.DiscardNull; }
            set { configuration.DiscardNull = value; }
        }

        [Description("A flag indicating whether the Data Terminal Ready (DTR) signal should be enabled.")]
        public bool DtrEnable
        {
            get { return configuration.DtrEnable; }
            set { configuration.DtrEnable = value; }
        }

        [Description("A flag indicating whether the Request to Send (RTS) signal should be enabled.")]
        public bool RtsEnable
        {
            get { return configuration.RtsEnable; }
            set { configuration.RtsEnable = value; }
        }

        [Description("The size of the read buffer, in bytes. This is the maximum number of read bytes which can be buffered.")]
        public int ReadBufferSize
        {
            get { return configuration.ReadBufferSize; }
            set { configuration.ReadBufferSize = value; }
        }

        [Description("The size of the write buffer, in bytes. This is the maximum number of bytes which can be queued for write.")]
        public int WriteBufferSize
        {
            get { return configuration.WriteBufferSize; }
            set { configuration.WriteBufferSize = value; }
        }

        [Description("The number of bytes required to be available before the read event is fired.")]
        public int ReceivedBytesThreshold
        {
            get { return configuration.ReceivedBytesThreshold; }
            set { configuration.ReceivedBytesThreshold = value; }
        }

        public override IObservable<SerialPort> Generate()
        {
            return Observable.Using(
                () => SerialPortManager.ReserveConnection(Name, configuration),
                connection => Observable.Return(connection.SerialPort).Concat(Observable.Never(connection.SerialPort)));
        }
    }
}
