using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class ShaderWindowSettings
    {
        readonly ShaderConfigurationCollection shaders = new ShaderConfigurationCollection();

        public ShaderWindowSettings()
        {
            Width = 640;
            Height = 480;
            VSync = VSyncMode.On;
            WindowState = WindowState.Normal;
            DisplayDevice = DisplayIndex.Default;
        }

        public int Width { get; set; }

        public int Height { get; set; }

        public string Title { get; set; }

        public VSyncMode VSync { get; set; }

        public WindowState WindowState { get; set; }

        public DisplayIndex DisplayDevice { get; set; }

        public ShaderConfigurationCollection Shaders
        {
            get { return shaders; }
        }
    }
}
