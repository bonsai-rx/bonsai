using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace Bonsai.Arduino
{
    public sealed class Arduino : IDisposable
    {
        #region Constants

        public const int DefaultBaudRate = 57600;
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

        public Arduino(string portName)
            : this(portName, DefaultBaudRate)
        {
        }

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

        public event EventHandler<AnalogInputReceivedEventArgs> AnalogInputReceived;

        public event EventHandler<DigitalInputReceivedEventArgs> DigitalInputReceived;

        public event EventHandler<SysexReceivedEventArgs> SysexReceived;

        public int MajorVersion { get; private set; }

        public int MinorVersion { get; private set; }

        public bool IsOpen
        {
            get { return serialPort.IsOpen; }
        }

        void OnAnalogInputReceived(AnalogInputReceivedEventArgs e)
        {
            var handler = AnalogInputReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void OnDigitalInputReceived(DigitalInputReceivedEventArgs e)
        {
            var handler = DigitalInputReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void OnSysexReceived(SysexReceivedEventArgs e)
        {
            var handler = SysexReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var bytesToRead = serialPort.BytesToRead;
            if (serialPort.IsOpen && bytesToRead > 0)
            {
                bytesToRead = serialPort.Read(readBuffer, 0, bytesToRead);
                for (int i = 0; i < bytesToRead; i++)
                {
                    ProcessInput(readBuffer[i]);
                }
            }
        }

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

        public void ReportAnalog(int pin, bool state)
        {
            ReportInput(ref reportAnalog, REPORT_ANALOG, pin, state);
        }

        public void ReportDigital(int port, bool state)
        {
            ReportInput(ref reportDigital, REPORT_DIGITAL, port, state);
        }

        public void PinMode(int pin, PinMode mode)
        {
            PinMode previousMode;
            if (!pinModes.TryGetValue(pin, out previousMode) || previousMode != mode)
            {
                commandBuffer[0] = SET_PIN_MODE;
                commandBuffer[1] = (byte)pin;
                commandBuffer[2] = (byte)mode;
                serialPort.Write(commandBuffer, 0, 3);
                pinModes[pin] = mode;
            }
        }

        public int DigitalRead(int pin)
        {
            var portNumber = GetPortNumber(pin);
            EnsureCapacity(ref digitalInput, portNumber);
            return ((digitalInput[portNumber] >> (pin & 0x07)) & 0x01);
        }

        public void DigitalWrite(int pin, bool value)
        {
            var portNumber = GetPortNumber(pin);
            EnsureCapacity(ref digitalOutput, portNumber);
            if (value) digitalOutput[portNumber] |= (byte)(1 << (pin & 0x07));
            else digitalOutput[portNumber] &= (byte)~(1 << (pin & 0x07));
            WritePort(portNumber, digitalOutput[portNumber]);
        }

        public void DigitalWrite(int portNumber, int value)
        {
            EnsureCapacity(ref digitalOutput, portNumber);
            digitalOutput[portNumber] = (byte)value;
            WritePort(portNumber, digitalOutput[portNumber]);
        }

        void WritePort(int portNumber, byte value)
        {
            commandBuffer[0] = (byte)(DIGITAL_MESSAGE | portNumber);
            commandBuffer[1] = (byte)(value & 0x7F);
            commandBuffer[2] = (byte)(value >> 7);
            serialPort.Write(commandBuffer, 0, 3);
        }

        public int AnalogRead(int pin)
        {
            EnsureCapacity(ref analogInput, pin);
            return analogInput[pin];
        }

        public void AnalogWrite(int pin, int value)
        {
            commandBuffer[0] = (byte)(ANALOG_MESSAGE | (pin & 0x0F));
            commandBuffer[1] = (byte)(value & 0x7F);
            commandBuffer[2] = (byte)(value >> 7);
            serialPort.Write(commandBuffer, 0, 3);
        }

        public void SamplingInterval(int milliseconds)
        {
            commandBuffer[0] = START_SYSEX;
            commandBuffer[1] = SAMPLING_INTERVAL;
            commandBuffer[2] = (byte)(milliseconds & 0x7F);
            commandBuffer[3] = (byte)(milliseconds >> 7);
            commandBuffer[4] = END_SYSEX;
            serialPort.Write(commandBuffer, 0, 5);
        }

        public void SendSysex(byte command, params byte[] args)
        {
            commandBuffer[0] = START_SYSEX;
            commandBuffer[1] = command;
            Array.Copy(args, 0, commandBuffer, 2, args.Length);
            commandBuffer[args.Length + 2] = END_SYSEX;
            serialPort.Write(commandBuffer, 0, args.Length + 3);
        }

        public void I2CConfig(params byte[] args)
        {
            SendSysex(I2C_CONFIG, args);
        }

        public void I2CWrite(int address, params byte[] data)
        {
            I2CRequest(address, 0, data);
        }

        public void I2CRequest(int address, byte mode, params byte[] data)
        {
            commandBuffer[0] = START_SYSEX;
            commandBuffer[1] = I2C_REQUEST;
            commandBuffer[2] = (byte)(address & 0x7F);
            commandBuffer[3] = (byte)((address >> 7) & 0x7 | mode);
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

        public static int GetPortNumber(int pin)
        {
            return (pin >> 3) & 0x0F;
        }

        void SetDigitalInput(int port, int data)
        {
            EnsureCapacity(ref digitalInput, port);
            digitalInput[port] = (byte)data;
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
                        case DIGITAL_MESSAGE: SetDigitalInput(multiByteChannel, (responseBuffer[0] << 7) + responseBuffer[1]); break;
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

        public void Close()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Arduino()
        {
            Dispose(false);
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

    public enum PinMode : byte
    {
        Input = 0x00,
        Output = 0x01,
        Analog = 0x02,
        Pwm = 0x03,
        Servo = 0x04,
        Shift = 0x05,
        I2C = 0x06,
        OneWire = 0x07,
        Stepper = 0x08,
        Encoder = 0x09,
        Serial = 0x0A,
        InputPullUp = 0x0B
    }
}
