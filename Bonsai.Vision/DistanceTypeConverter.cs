using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    class DistanceTypeConverter : EnumConverter
    {
        internal DistanceTypeConverter()
            : base(typeof(DistanceType))
        {
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new[]
            {
                DistanceType.L1,
                DistanceType.L2,
                DistanceType.C,
                DistanceType.L12,
                DistanceType.Fair,
                DistanceType.Welsch,
                DistanceType.Huber
            });
        }
    }
}
