using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    class DepthConverter : EnumConverter
    {
        public DepthConverter()
            : base(typeof(Depth))
        {
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new[]
            {
                Depth.U8,
                Depth.S8,
                Depth.U16,
                Depth.S16,
                Depth.S32,
                Depth.F32,
                Depth.F64
            });
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
