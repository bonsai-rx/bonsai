﻿using System.Linq;
using System.ComponentModel;

namespace Bonsai.Audio
{
    internal class CaptureDeviceNameConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(OpenTK.Audio.AudioCapture.AvailableDevices.ToArray());
        }
    }
}
