using System.Runtime.InteropServices;

namespace Bonsai.Configuration
{
    static class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern int AddDllDirectory(string NewDirectory);
    }
}
