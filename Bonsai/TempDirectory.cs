using System;
using System.IO;

namespace Bonsai
{
    class TempDirectory : IDisposable
    {
        string path;

        public TempDirectory(string path)
        {
            this.path = path;
            if (path != null)
            {
                Directory.CreateDirectory(path);
            }
        }

        public string Path
        {
            get { return path; }
        }

        ~TempDirectory()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (path != null)
            {
                try { Directory.Delete(path, true); }
                catch { } // best effort
                path = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
