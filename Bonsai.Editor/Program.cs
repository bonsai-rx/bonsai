using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Bonsai.Editor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var initialFileName = args.Length > 0 ? args[0] : null;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm { InitialFileName = initialFileName });
        }
    }
}
