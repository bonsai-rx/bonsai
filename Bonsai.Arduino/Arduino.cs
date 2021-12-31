using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace Bonsai.Arduino
{
    /// <summary>
    /// Represents an Arduino board communicating with the host computer using
    /// the Firmata protocol.
    /// </summary>
    public sealed class Arduino : IDisposable
    {
        #region Constants

        /// <summary>
        /// Represents the default serial baud rate used to communicate with the Arduino.
        /// </summary>
        public const int DefaultBaudRate = 57600;

        /// <summary>
        /// Represents the default sampling interval for analog pins.
        /// </summary>
        public const int DefaultSamplingInterval = 19;

        const int MaxDataBytes = 32;
        const int ConnectionDelay = 2000;

        const byte DIGITAL_MESSAGE   = 0x90; // send data for a digital port
        const byte ANALOG_MESSAGE    = 0xE0; // send data for an analog pin (or PWM)
        const byte REPORT_ANALOG     = 0xC0; // enable analog input by pin #
        const byte REPORT_DIGITAL    = 0xD0; // enable digital input by port
        const byte SET_PIN_MODE      = 0xF4; // set a pin to INPUT/OUTPUT/PWM/etc
        const byte REPORT_VERSION    = 0xF9; // report firmware version
        const byte SYSTEM_RESET      = 0xFF; // reset from MIDI
        const byte START_SYSEX       = 0xF0; // start a MIDI SysEx message
        const byte END_SYSEX         = 0xF7; // end a MIDI SysEx message

        const byte I2C_REQUEST       = 0x76; // send an I2C request message
        const byte I2C_REPLY         = 0x77; // receive an I2C reply message
        const byte I2C_CONFIG        = 0x78; // set an I2C config message
        const byte SAMPLING_INTERVAL = 0x7A; // set the sampling interval

        #endregion

        bool disposed;
        bool parsingSysex;
        int dataToReceive;
        int multiByteCommand;
        int multiByteChannel;
        int sysexBytesRead;
        readonly Dictionary<int, PinMode> pinModes;
        readonly SerialPort serialPort;
        readonly byte[] responseBuffer;
        readonly byte[] commandBuffer;
        readonly byte[] sysexBuffer;
        readonly byte[] readBuffer;
        int[] reportAnalog;
        int[] reportDigital;
        int[] analogInput;
        byte[] digitalInput;
        byte[] digitalOutput;

        /// <summary>
        /// Initializes a new instance of the <see cref="Arduino"/> class using the
        /// specified port name.
        /// </summary>
        /// <param name="portName">The port to use (for example, COM1).</param>
        public Arduino(string portName)
            : this(portName, DefaultBaudRate)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Arduino"/> class using the
        /// specified port name and baud rate.
        /// </summary>
        /// <param name="portName">The port to use (for example, COM1).</param>
        /// <param name="baudRate">The serial baud rate.</param>
        public Arduino(string portName, int baudRate)
        {
            serialPort = new SerialPort(portName);
            serialPort.DtrEnable = true;
            serialPort.BaudRate = baudRate;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;

            pinModes = new Dictionary<int, PinMode>();
            responseBuffer = new byte[2];
            commandBuffer = new byte[MaxDataBytes];
            sysexBuffer = new byte[MaxDataBytes];
            readBuffer = new byte[serialPort.ReadBufferSize];
            serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
        }

        /// <summary>
        /// Occurs when the <see cref="Arduino"/> object receives a new analog input event.
        /// </summary>
        public event EventHandler<AnalogInputReceivedEventArgs> AnalogInputReceived;

        /// <summary>
        /// Occurs when the <see cref="Arduino"/> object receives a new digital input event.
        /// </summary>
        public event EventHandler<DigitalInputReceivedEventArgs> DigitalInputReceived;

        /// <summary>
        /// Occurs when the <see cref="Arduino"/> object receives a new MIDI SysEx message.
        /// </summary>
        public event EventHandler<SysexReceivedEventArgs> SysexReceived;

        /// <summary>
        /// Gets the major version of the Firmata firmware reported by the board on initialization.
        /// </summary>
        public int MajorVersion { get; private set; }

        /// <summary>
        /// Gets the minor version of the Firmata firmware reported by the board on initialization.
        /// </summary>
        public int MinorVersion { get; private set; }

        /// <summary>
        /// Gets a value indicating the open or closed status of the <see cref="Arduino"/> object.
        /// </summary>
        public bool IsOpen
        {
            get { return serialPort.IsOpen; }
        }

        void OnAnalogInputReceived(AnalogInputReceivedEventArgs e)
        {
            AnalogInputReceived?.Invoke(this, e);
        }

        void OnDigitalInputReceived(DigitalInputReceivedEventArgs e)
        {
            DigitalInputReceived?.Invoke(this, e);
        }

        void OnSysexReceived(SysexReceivedEventArgs e)
        {
            SysexReceived?.Invoke(this, e);
        }

        void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var bytesToRead = serialPort.BytesToRead;
            while (serialPort.IsOpen && bytesToRead > 0)
            {
                var bytesRead = serialPort.Read(readBuffer, 0, Math.Min(bytesToRead, readBuffer.Length));
                for (int i = 0; i < bytesRead; i++)
                {
                    ProcessInput(readBuffer[i]);
                }
                bytesToRead -= bytesRead;
            }
        }

        /// <summary>
        /// Opens a new serial port connection to the Arduino board.
        /// </summary>
        public void Open()
        {
            serialPort.Open();
            Thread.Sleep(ConnectionDelay);
            serialPort.ReadExisting();
        }

        void ReportInput(ref int[] reportInput, byte command, int index, bool state)
        {
            EnsureCapacity(ref reportInput, index);
            if (!state && reportInput[index] > 0) reportInput[index]--;

            if (reportInput[index] == 0)
            {
                commandBuffer[0] = (byte)(command | index);
                commandBuffer[1] = (byte)(state ? 1 : 0);
                serialPort.Write(commandBuffer, 0, 2);
            }

            if (state) reportInput[index]++;
        }

        /// <summary>
        /// Enables or disables reporting of analog pin values for
        /// the specified pin number.
        /// </summary>
        /// <param name="pin">The number of the pin to configure.</param>
        /// <param name="state">
        /// <see langword="true"/> if analog reporting for the pin should be enabled;
        /// <see langword="false"/> otherwise.
        /// </param>
        public void ReportAnalog(int pin, bool state)
        {
            ReportInput(ref reportAnalog, REPORT_ANALOG, pin, state);
        }

        /// <summary>
        /// Enables or disables reporting of digital pin changes for
        /// the specified digital port in the Arduino.
        /// </summary>
        /// <param name="port">The digital port to configure.</param>
        /// <param name="state">
        /// <see langword="true"/> if reporting of digital pin changes on the
        /// specified port should be enabled; <see langword="false"/> otherwise.
        /// </param>
        public void ReportDigital(int port, bool state)
        {
            ReportInput(ref reportDigital, REPORT_DIGITAL, port, state);
        }

        /// <summary>
        /// Sets the mode of an individual Arduino pin.
        /// </summary>
        /// <param name="pin">The number of the pin to configure.</param>
        /// <param name="mode">The pin mode.</param>
        public void PinMode(int pin, PinMode mode)
        {
            if (!pinModes.TryGetValue(pin, out PinMode previousMode) || previousMode != mode)
            {
                commandBuffer[0] = SET_PIN_MODE;
                commandBuffer[1] = (byte)pin;
                commandBuffer[2] = (byte)mode;
                serialPort.Write(commandBuffer, 0, 3);
                pinModes[pin] = mode;
            }
        }

        /// <summary>
        /// Reads the current state of the specified digital input pin.
        /// </summary>
        /// <param name="pin">The number of the digital pin to read.</param>
        /// <returns>
        /// <see langword="true"/> if the pin is HIGH; <see langword="false"/> if the pin is LOW.
        /// </returns>
        public bool DigitalRead(int pin)
        {
            var port = GetPortNumber(pin);
            EnsureCapacity(ref digitalInput, port);
            return ((digitalInput[port] >> (pin & 0x07)) & 0x01) != 0;
        }

        /// <summary>
        /// Sets the state of the specified digital output pin.
        /// </summary>
        /// <param name="pin">The number of the digital pin to write.</param>
        /// <param name="value">
        /// <see langword="true"/> to set the pin HIGH; <see langword="false"/> to set the pin LOW.
        /// </param>
        public void DigitalWrite(int pin, bool value)
        {
            var port = GetPortNumber(pin);
            EnsureCapacity(ref digitalOutput, port);
            if (value) digitalOutput[port] |= (byte)(1 << (pin & 0x07));
            else digitalOutput[port] &= (byte)~(1 << (pin & 0x07));
            WritePort(port, digitalOutput[port]);
        }

        /// <summary>
        /// Reads the current state of all the digital pins in the specified port.
        /// </summary>
        /// <param name="port">The number of the digital port (i.e. collection of 8 pins) to read.</param>
        /// <returns>
        /// A <see cref="byte"/> value where each bit represents the state of one pin in the digital port.
        /// </returns>
        public byte DigitalPortRead(int port)
        {
            EnsureCapacity(ref digitalInput, port);
            return digitalInput[port];
        }

        /// <summary>
        /// Sets the state of all the digital output pins in the specified
        /// port simultaneously.
        /// </summary>
        /// <param name="port">The number of the digital port (i.e. collection of 8 pins) to write.</param>
        /// <param name="value">
        /// A <see cref="byte"/> value where each bit will be used to set the state of one pin in the digital port.
        /// </param>
        public void DigitalPortWrite(int port, byte value)
        {
            EnsureCapacity(ref digitalOutput, port);
            digitalOutput[port] = value;
            WritePort(port, digitalOutput[port]);
        }

        void WritePort(int port, byte value)
        {
            commandBuffer[0] = (byte)(DIGITAL_MESSAGE | port);
            commandBuffer[1] = (byte)(value & 0x7F);
            commandBuffer[2] = (byte)(value >> 7);
            serialPort.Write(commandBuffer, 0, 3);
        }

        /// <summary>
        /// Returns the current value of the specified analog pin.
        /// </summary>
        /// <param name="pin">The number of the analog pin to read.</param>
        /// <returns>A <see cref="int"/> value representing a digitized analog measurement.</returns>
        public int AnalogRead(int pin)
        {
            EnsureCapacity(ref analogInput, pin);
            return analogInput[pin];
        }

        /// <summary>
        /// Writes an analog value as a PWM wave to the specified digital output pin.
        /// </summary>
        /// <param name="pin">The number of the digital pin to write.</param>
        /// <param name="value">A <see cref="int"/> value used to update the PWM signal.</param>
        public void AnalogWrite(int pin, int value)
        {
            commandBuffer[0] = (byte)(ANALOG_MESSAGE | (pin & 0x0F));
            commandBuffer[1] = (byte)(value & 0x7F);
            commandBuffer[2] = (byte)(value >> 7);
            serialPort.Write(commandBuffer, 0, 3);
        }

        /// <summary>
        /// Sets the sampling rate for reporting analog and I2C data in the main firmware loop.
        /// </summary>
        /// <param name="milliseconds">
        /// The sampling interval, in milliseconds, between analog and I2C measurements.
        /// </param>
        public void SamplingInterval(int milliseconds)
        {
            commandBuffer[0] = START_SYSEX;
            commandBuffer[1] = SAMPLING_INTERVAL;
            commandBuffer[2] = (byte)(milliseconds & 0x7F);
            commandBuffer[3] = (byte)(milliseconds >> 7);
            commandBuffer[4] = END_SYSEX;
            serialPort.Write(commandBuffer, 0, 5);
        }

        /// <summary>
        /// Sends the specified MIDI SysEx command using the specified arguments.
        /// </summary>
        /// <param name="command">A <see cref="byte"/> value indicating the SysEx command ID.</param>
        /// <param name="args">The optional extended payload sent to configure the SysEx command.</param>
        public void SendSysex(byte command, params byte[] args)
        {
            commandBuffer[0] = START_SYSEX;
            commandBuffer[1] = command;
            Array.Copy(args, 0, commandBuffer, 2, args.Length);
            commandBuffer[args.Length + 2] = END_SYSEX;
            serialPort.Write(commandBuffer, 0, args.Length + 3);
        }

        /// <summary>
        /// Configures I2C settings such as delay time and power pins.
        /// </summary>
        /// <param name="args">
        /// The I2C configuration arguments. The first two bytes are used
        /// to configure the optional delay time, in microseconds, between
        /// writing to the I2C register, and reading the data from the device.
        /// </param>
        public void I2CConfig(params byte[] args)
        {
            SendSysex(I2C_CONFIG, args);
        }

        /// <summary>
        /// Writes a data payload to the I2C device with the specified address.
        /// </summary>
        /// <param name="address">The address of the slave device in the I2C bus.</param>
        /// <param name="data">The data payload to write to the device.</param>
        public void I2CWrite(int address, params byte[] data)
        {
            I2CRequest(address, I2CRequestMode.Write, data);
        }

        /// <summary>
        /// Sends a request to the I2C device with the specified address.
        /// </summary>
        /// <param name="address">The address of the slave device in the I2C bus.</param>
        /// <param name="mode">The read/write mode of the request.</param>
        /// <param name="data">The data payload for the I2C request.</param>
        public void I2CRequest(int address, I2CRequestMode mode, params byte[] data)
        {
            const byte ExtendedAddressBit = 0x1 << 5;
            var extendedMode = address > 127 ? ExtendedAddressBit : 0;
            commandBuffer[0] = START_SYSEX;
            commandBuffer[1] = I2C_REQUEST;
            commandBuffer[2] = (byte)(address & 0x7F);
            commandBuffer[3] = (byte)((address >> 7) & 0x7 | (byte)mode << 3 | extendedMode);
            Array.Copy(data, 0, commandBuffer, 4, data.Length);
            commandBuffer[data.Length + 4] = END_SYSEX;
            serialPort.Write(commandBuffer, 0, data.Length + 5);
        }

        static void EnsureCapacity<TElement>(ref TElement[] array, int index)
        {
            if (array == null || index >= array.Length)
            {
                Array.Resize(ref array, index + 1);
            }
        }

        /// <summary>
        /// Gets the digital port number for the specified pin.
        /// </summary>
        /// <param name="pin">The pin number for which to retrieve the digital port.</param>
        /// <returns>A <see cref="int"/> identifier for the digital port containing the specified pin.</returns>
        public static int GetPortNumber(int pin)
        {
            return (pin >> 3) & 0x0F;
        }

        void SetDigitalInput(int port, byte data)
        {
            EnsureCapacity(ref digitalInput, port);
            digitalInput[port] = data;
            OnDigitalInputReceived(new DigitalInputReceivedEventArgs(port, data));
        }

        void SetAnalogInput(int pin, int value)
        {
            EnsureCapacity(ref analogInput, pin);
            analogInput[pin] = value;
            OnAnalogInputReceived(new AnalogInputReceivedEventArgs(pin, value));
        }

        void SetVersion(int majorVersion, int minorVersion)
        {
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
        }

        void ProcessInput(byte inputData)
        {
            if (parsingSysex)
            {
                if (inputData == END_SYSEX)
                {
                    parsingSysex = false;
                    var feature = sysexBuffer[0];
                    var args = new byte[sysexBytesRead - 1];
                    Array.Copy(sysexBuffer, 1, args, 0, args.Length);
                    OnSysexReceived(new SysexReceivedEventArgs(feature, args));
                }
                else if (sysexBytesRead < sysexBuffer.Length)
                {
                    sysexBuffer[sysexBytesRead++] = inputData;
                }
                else parsingSysex = false;
            }
            else if (dataToReceive > 0 && inputData < 128)
            {
                dataToReceive--;
                responseBuffer[dataToReceive] = inputData;

                if (multiByteCommand != 0 && dataToReceive == 0)
                {
                    switch (multiByteCommand)
                    {
                        case DIGITAL_MESSAGE: SetDigitalInput(multiByteChannel, (byte)((responseBuffer[0] << 7) + responseBuffer[1])); break;
                        case ANALOG_MESSAGE: SetAnalogInput(multiByteChannel, (responseBuffer[0] << 7) + responseBuffer[1]); break;
                        case REPORT_VERSION: SetVersion(responseBuffer[1], responseBuffer[0]); break;
                    }
                }
            }
            else
            {
                int command;
                if (inputData < 0xF0)
                {
                    command = inputData & 0xF0;
                    multiByteChannel = inputData & 0x0F;
                }
                else command = inputData;

                switch (command)
                {
                    case DIGITAL_MESSAGE:
                    case ANALOG_MESSAGE:
                    case REPORT_VERSION:
                        dataToReceive = 2;
                        multiByteCommand = command;
                        break;
                    case START_SYSEX:
                        parsingSysex = true;
                        sysexBytesRead = 0;
                        break;
                }
            }
        }

        /// <summary>
        /// Closes the port connection, sets the <see cref="IsOpen"/>
        /// property to <see langword="false"/> and disposes of the
        /// internal <see cref="SerialPort"/> object.
        /// </summary>
        public void Close()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    serialPort.Close();
                    disposed = true;
                }
            }
        }

        void IDisposable.Dispose()
        {
            Close();
        }
    }

    /// <summary>
    /// Specifies the mode of an individual Arduino pin.
    /// </summary>
    public enum PinMode : byte
    {
        /// <summary>
        /// The digital pin is configured as INPUT.
        /// </summary>
        Input = 0x00,

        /// <summary>
        /// The digital pin is configured as OUTPUT.
        /// </summary>
        Output = 0x01,

        /// <summary>
        /// The analog pin is configured in analog input mode.
        /// </summary>
        Analog = 0x02,

        /// <summary>
        /// The digital pin is configured in PWM output mode.
        /// </summary>
        Pwm = 0x03,

        /// <summary>
        /// The digital pin is configured in Servo output mode.
        /// </summary>
        Servo = 0x04,

        /// <summary>
        /// The pin is configured as a data pin in shiftOut/shiftIn mode.
        /// </summary>
        Shift = 0x05,

        /// <summary>
        /// The pin is configured to access I2C devices.
        /// </summary>
        I2C = 0x06,

        /// <summary>
        /// The pin is configured as a 1-wire bus master.
        /// </summary>
        OneWire = 0x07,

        /// <summary>
        /// The pin is configured for stepper motor control.
        /// </summary>
        Stepper = 0x08,

        /// <summary>
        /// The pin is configured for a rotary encoder.
        /// </summary>
        Encoder = 0x09,

        /// <summary>
        /// The pin is configured for serial communication.
        /// </summary>
        Serial = 0x0A,

        /// <summary>
        /// The digital pin is configured as INPUT_PULLUP.
        /// </summary>
        InputPullUp = 0x0B
    }

    /// <summary>
    /// Specifies the read/write mode for I2C requests.
    /// </summary>
    public enum I2CRequestMode : byte
    {
        /// <summary>
        /// A request to write data to the device.
        /// </summary>
        Write = 0x0,

        /// <summary>
        /// A request to read one data sample from the device.
        /// </summary>
        ReadOnce = 0x1,

        /// <summary>
        /// A request to read and report data continuously from the device.
        /// </summary>
        ReadContinuously = 0x2,

        /// <summary>
        /// A request to stop reading data from the device.
        /// </summary>
        StopReading = 0x3
    }
}
