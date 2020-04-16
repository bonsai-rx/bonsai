using OpenCV.Net;
using System.ComponentModel;
using System.Linq;

namespace Bonsai.Dsp
{
    class DepthConverter : NullableConverter
    {
        public DepthConverter()
            : base(typeof(Depth?))
        {
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var values = base.GetStandardValues(context);
            return new StandardValuesCollection(values.Cast<object>().Where(value => (Depth?)value != Depth.UserType).ToArray());
        }
    }
}
