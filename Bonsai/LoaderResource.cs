using Bonsai.Configuration;
using System;
using System.Reflection;

namespace Bonsai
{
    class LoaderResource<TLoader> : IDisposable where TLoader : MarshalByRefObject
    {
        AppDomain reflectionDomain;

        public LoaderResource(PackageConfiguration configuration)
        {
            var currentEvidence = AppDomain.CurrentDomain.Evidence;
            var setupInfo = AppDomain.CurrentDomain.SetupInformation;
            reflectionDomain = AppDomain.CreateDomain("ReflectionOnly", currentEvidence, setupInfo);
            Loader = (TLoader)reflectionDomain.CreateInstanceAndUnwrap(
                typeof(TLoader).Assembly.FullName,
                typeof(TLoader).FullName,
                false, (BindingFlags)0, null,
                new[] { configuration }, null, null);
        }

        public TLoader Loader { get; private set; }

        public void Dispose()
        {
            AppDomain.Unload(reflectionDomain);
        }
    }
}
