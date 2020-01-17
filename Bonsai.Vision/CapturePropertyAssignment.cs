using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    public class CapturePropertyAssignment
    {
        public CaptureProperty Property { get; set; }

        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        public double Value { get; set; }

        public override string ToString()
        {
            return string.Format("{0} = {1}", Property, Value);
        }
    }
}
