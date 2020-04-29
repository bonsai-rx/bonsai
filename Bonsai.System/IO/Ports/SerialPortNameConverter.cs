using System.ComponentModel;
using System.IO.Ports;

namespace Bonsai.IO
{
    public class SerialPortNameConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(SerialPort.GetPortNames());
        }
    }
}
