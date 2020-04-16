using System;

namespace Bonsai64
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        static int Main(string[] args)
        {
            return Bonsai.Program.Main(args);
        }
    }
}
