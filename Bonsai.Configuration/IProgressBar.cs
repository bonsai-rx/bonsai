using System;

namespace Bonsai.Configuration
{
    public interface IProgressBar : IProgress<int>, IDisposable
    {
    }
}
