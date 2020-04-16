using System.ComponentModel;

namespace Bonsai.IO
{
    public class BaudRateConverter : Int32Converter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new[]
            {
                9600,
                14400,
                19200,
                38400,
                57600,
                115200,
                128000
            });
        }
    }
}
