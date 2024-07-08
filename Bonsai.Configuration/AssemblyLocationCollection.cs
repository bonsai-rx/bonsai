using System;
using System.Reflection;

namespace Bonsai.Configuration
{
    [Serializable]
    public class AssemblyLocationCollection : SortedKeyedCollection<(string, ProcessorArchitecture), AssemblyLocation>
    {
        public void Add(string name, ProcessorArchitecture processorArchitecture, string path)
        {
            Add(new AssemblyLocation(name, processorArchitecture, path));
        }

        protected override (string, ProcessorArchitecture) GetKeyForItem(AssemblyLocation item)
        {
            return (item.AssemblyName, item.ProcessorArchitecture);
        }
    }
}
