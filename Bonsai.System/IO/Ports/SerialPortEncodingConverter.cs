using System.ComponentModel;
using System.Text;

namespace Bonsai.IO
{
    class SerialPortEncodingConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new[]
            {
                Encoding.ASCII.WebName,
                Encoding.UTF8.WebName,
                Encoding.UTF32.WebName,
                Encoding.Unicode.WebName,
                Encoding.GetEncoding(28591).WebName
            });
        }
    }
}
