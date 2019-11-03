using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bonsai.Configuration
{
    [Serializable]
    public class AssemblyLocationCollection : SortedKeyedCollection<Tuple<string, ProcessorArchitecture>, AssemblyLocation>
    {
        public void Add(string name, ProcessorArchitecture processorArchitecture, string path)
        {
            Add(new AssemblyLocation(name, processorArchitecture, path));
        }

        protected override Tuple<string, ProcessorArchitecture> GetKeyForItem(AssemblyLocation item)
        {
            return Tuple.Create(item.AssemblyName, item.ProcessorArchitecture);
        }
    }
}
