using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    public struct CameraCalibration
    {
        public Intrinsics Intrinsics;
        public double ReprojectionError;

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{{Intrinsics: {0}, ReprojectionError: {1}}}",
                Intrinsics,
                ReprojectionError);
        }
    }
}
