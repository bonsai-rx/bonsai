using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Video.Design
{
    public class VideoCaptureDeviceEditor : WorkflowComponentEditor
    {
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            var videoCaptureDevice = (VideoCaptureDevice)component;
            var videoSource = (AForge.Video.DirectShow.VideoCaptureDevice)videoCaptureDevice.VideoSource;
            if (videoSource != null)
            {
                videoSource.DisplayPropertyPage(owner.Handle);
                return true;
            }
            else return false;
        }
    }
}
